using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TenderEvidenceChecker.Api.Data;
using TenderEvidenceChecker.Api.Services;

namespace TenderEvidenceChecker.Api.Api;

public static class Endpoints
{
    private static readonly HashSet<string> Decisions = new(StringComparer.OrdinalIgnoreCase)
    {
        "confirm", "edit", "reject", "uncertain", "not_applicable", "missing"
    };

    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "tender-evidence-checker" }));

        app.MapPost("/api/analyses", CreateAnalysis);
        app.MapGet("/api/analyses/{analysisId:guid}", GetAnalysis);
        app.MapGet("/api/analyses/{analysisId:guid}/requirements", GetRequirements);
        app.MapGet("/api/analyses/{analysisId:guid}/pages", GetPages);
        app.MapPost("/api/analyses/{analysisId:guid}/evidence-files", AddEvidence);
        app.MapPost("/api/analyses/{analysisId:guid}/match-evidence", MatchEvidence);
        app.MapPost("/api/analyses/{analysisId:guid}/retry", Retry);
        app.MapPost("/api/analyses/{analysisId:guid}/cancel", Cancel);
        app.MapPatch("/api/requirements/{requirementId:guid}", PatchRequirement);
        app.MapGet("/api/analyses/{analysisId:guid}/export.csv", Export);
        app.MapDelete("/api/analyses/{analysisId:guid}", DeleteAnalysis);
    }

    private static async Task<IResult> CreateAnalysis(
        HttpRequest request,
        AppDbContext db,
        DocumentUpload uploads,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            return Error("unsupported_file_type", "Upload a PDF with the form field named file.", false, 400);
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var label = (form["label"].ToString() ?? "").Trim();
        if (label.Length is < 2 or > 200)
        {
            return Error("unsupported_file_type", "Name the tender with 2 to 200 characters, then choose a PDF.", false, 400);
        }

        var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
        if (file is null)
        {
            return Error("unsupported_file_type", "Choose one tender PDF.", false, 400);
        }

        var analysis = new AnalysisEntity
        {
            Id = Guid.NewGuid(),
            Label = label,
            Status = "queued",
            ProgressStage = "queued",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        StoredFileEntity stored;
        try
        {
            stored = await uploads.SaveAsync(analysis.Id, file, "tender", extractText: false, cancellationToken);
        }
        catch (AppException ex)
        {
            return Error(ex.Code, ex.UserMessage, ex.Retryable, ex.StatusCode);
        }

        analysis.Files.Add(stored);
        analysis.Jobs.Add(new JobEntity
        {
            Id = Guid.NewGuid(),
            Kind = "extract",
            Status = "queued",
            CreatedAt = DateTime.UtcNow
        });
        db.Analyses.Add(analysis);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Json(new
        {
            analysis_id = analysis.Id,
            status = analysis.Status,
            accepted_files = new[]
            {
                FileDto(stored)
            },
            warnings = Array.Empty<string>()
        }, statusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetAnalysis(Guid analysisId, AppDbContext db, CancellationToken cancellationToken)
    {
        var analysis = await Load(db, analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        var pages = analysis.Files.SelectMany(file => file.Pages).ToList();
        var stale = analysis.Status is "queued" or "processing"
            && analysis.UpdatedAt < DateTime.UtcNow.AddSeconds(-90);
        return Results.Ok(new
        {
            analysis_id = analysis.Id,
            label = analysis.Label,
            status = analysis.Status,
            progress_stage = analysis.ProgressStage,
            provider_id = analysis.ProviderId,
            model_id = analysis.ModelId,
            prompt_version = analysis.PromptVersion,
            stale,
            counts = new
            {
                files = analysis.Files.Count,
                pages = pages.Count,
                usable_pages = pages.Count(page => page.Usable),
                unreadable_pages = pages.Count(page => !page.Usable),
                requirements = analysis.Requirements.Count
            },
            error = analysis.ErrorCode is null
                ? null
                : new
                {
                    code = analysis.ErrorCode,
                    user_message = analysis.ErrorMessage,
                    retryable = analysis.ErrorRetryable
                },
            warnings = Warnings(analysis),
            files = analysis.Files.Select(FileDto)
        });
    }

    private static async Task<IResult> GetRequirements(Guid analysisId, AppDbContext db, CancellationToken cancellationToken)
    {
        var analysis = await Load(db, analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        var files = analysis.Files.ToDictionary(file => file.Id);
        return Results.Ok(new
        {
            requirements = analysis.Requirements
                .OrderBy(item => item.PageNumber ?? int.MaxValue)
                .ThenBy(item => item.Statement)
                .Select(item => RequirementDto(item, files))
        });
    }

    private static async Task<IResult> GetPages(Guid analysisId, AppDbContext db, CancellationToken cancellationToken)
    {
        var analysis = await Load(db, analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        var pages = analysis.Files
            .OrderBy(file => file.Role)
            .ThenBy(file => file.OriginalName)
            .SelectMany(file => file.Pages.OrderBy(page => page.PageNumber).Select(page => new
            {
                file_id = file.Id,
                file_name = file.OriginalName,
                role = file.Role,
                page_number = page.PageNumber,
                usable = page.Usable,
                extraction_method = page.ExtractionMethod,
                ocr_status = page.OcrStatus,
                preview = page.Usable ? TextNorm.Preview(page.Text, 320) : ""
            }));
        return Results.Ok(new { pages });
    }

    private static async Task<IResult> AddEvidence(
        Guid analysisId,
        HttpRequest request,
        AppDbContext db,
        DocumentUpload uploads,
        CancellationToken cancellationToken)
    {
        var analysis = await Load(db, analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        if (analysis.Status is "queued" or "processing")
        {
            return Error("export_failed", "Wait until the tender checklist finishes before adding company documents.", true, 409);
        }

        if (!request.HasFormContentType)
        {
            return Error("unsupported_file_type", "Upload one or more company PDFs.", false, 400);
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var incoming = form.Files.GetFiles("files");
        if (incoming.Count == 0)
        {
            incoming = form.Files.ToList();
        }

        if (incoming.Count == 0)
        {
            return Error("unsupported_file_type", "Choose at least one company PDF.", false, 400);
        }

        var existing = analysis.Files.Count(file => file.Role == "evidence");
        var limit = uploads.FileLimit;
        if (existing + incoming.Count > limit)
        {
            return Error("file_too_large", $"This demo keeps at most {limit} company documents on one analysis.", false, 400);
        }

        var saved = new List<StoredFileEntity>();
        try
        {
            foreach (var file in incoming)
            {
                var stored = await uploads.SaveAsync(analysis.Id, file, "evidence", extractText: true, cancellationToken);
                analysis.Files.Add(stored);
                db.Entry(stored).State = EntityState.Added;
                foreach (var page in stored.Pages)
                {
                    db.Entry(page).State = EntityState.Added;
                }

                saved.Add(stored);
            }

            foreach (var file in saved)
            {
                var earlier = saved.FirstOrDefault(other => other.Id != file.Id && other.Sha256 == file.Sha256);
                if (earlier is not null && file.DuplicateOfFileId is null)
                {
                    file.DuplicateOfFileId = earlier.Id;
                }
            }

            analysis.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (AppException ex)
        {
            return Error(ex.Code, ex.UserMessage, ex.Retryable, ex.StatusCode);
        }

        return Results.Ok(new
        {
            files = saved.Select(FileDto),
            processing_status = "stored"
        });
    }

    private static async Task<IResult> MatchEvidence(Guid analysisId, AppDbContext db, CancellationToken cancellationToken)
    {
        var analysis = await Load(db, analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        if (analysis.Requirements.Count == 0 || analysis.Status is "queued" or "processing" or "failed")
        {
            return Error("export_failed", "Extract a checklist before asking for evidence suggestions.", true, 409);
        }

        if (analysis.Requirements.Any(requirement => requirement.ReviewerDecision is not null))
        {
            return Error("export_failed", "Review decisions are already saved. They were not overwritten.", false, 409);
        }

        if (analysis.Jobs.Any(job => job.Kind == "match" && job.Status is "queued" or "processing"))
        {
            return Results.Ok(new { job_id = analysis.Jobs.Last(job => job.Kind == "match").Id, status = "queued" });
        }

        var job = new JobEntity
        {
            Id = Guid.NewGuid(),
            Kind = "match",
            Status = "queued",
            CreatedAt = DateTime.UtcNow
        };
        analysis.Jobs.Add(job);
        analysis.Status = "queued";
        analysis.ProgressStage = "matching_evidence";
        analysis.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Results.Accepted($"/api/analyses/{analysis.Id}", new { job_id = job.Id, status = "queued" });
    }

    private static async Task<IResult> Retry(Guid analysisId, AppDbContext db, CancellationToken cancellationToken)
    {
        var analysis = await Load(db, analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        if (analysis.Status != "failed")
        {
            return Error("export_failed", "Retry is available after a failed run. This analysis is not failed.", false, 409);
        }

        if (analysis.Jobs.Any(job => job.Status is "queued" or "processing"))
        {
            return Error("export_failed", "A job is already queued for this analysis.", true, 409);
        }

        var kind = analysis.Requirements.Count == 0 ? "extract" : "match";
        analysis.Jobs.Add(new JobEntity
        {
            Id = Guid.NewGuid(),
            Kind = kind,
            Status = "queued",
            CreatedAt = DateTime.UtcNow
        });
        analysis.Status = "queued";
        analysis.ErrorCode = null;
        analysis.ErrorMessage = null;
        analysis.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { analysis_id = analysis.Id, status = "queued" });
    }

    private static async Task<IResult> Cancel(Guid analysisId, AppDbContext db, CancellationToken cancellationToken)
    {
        var analysis = await db.Analyses.Include(item => item.Jobs).FirstOrDefaultAsync(item => item.Id == analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        if (analysis.Status is not ("queued" or "processing"))
        {
            return Error("export_failed", "Only a queued or running analysis can be cancelled.", false, 409);
        }

        analysis.Status = "cancelled";
        analysis.ProgressStage = "cancelled";
        analysis.UpdatedAt = DateTime.UtcNow;
        foreach (var job in analysis.Jobs.Where(job => job.Status == "queued"))
        {
            job.Status = "cancelled";
            job.FinishedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { analysis_id = analysis.Id, status = "cancelled" });
    }

    private static async Task<IResult> PatchRequirement(
        Guid requirementId,
        ReviewPatch body,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var requirement = await db.Requirements
            .Include(item => item.Analysis)
            .FirstOrDefaultAsync(item => item.Id == requirementId, cancellationToken);
        if (requirement?.Analysis is null)
        {
            return Missing();
        }

        var decision = (body.Decision ?? "").Trim().ToLowerInvariant();
        if (!Decisions.Contains(decision))
        {
            return Error("export_failed", "Choose confirm, edit, reject, uncertain, missing, or not applicable.", false, 400);
        }

        if (decision == "not_applicable" && string.IsNullOrWhiteSpace(body.Note))
        {
            return Error("export_failed", "Marking a row not applicable needs a short note.", false, 400);
        }

        if (decision == "edit" && string.IsNullOrWhiteSpace(body.Statement))
        {
            return Error("export_failed", "Write the corrected requirement before saving an edit.", false, 400);
        }

        if (string.IsNullOrWhiteSpace(requirement.AiSnapshotJson))
        {
            requirement.AiSnapshotJson = JsonSerializer.Serialize(new { statement = requirement.Statement, review_status = requirement.ReviewStatus });
        }

        requirement.ReviewerDecision = decision;
        requirement.HumanNote = string.IsNullOrWhiteSpace(body.Note) ? requirement.HumanNote : body.Note.Trim();
        requirement.ReviewedAt = DateTime.UtcNow;
        if (decision == "edit")
        {
            requirement.EditedStatement = body.Statement!.Trim();
            requirement.ReviewStatus = "changed";
        }
        else if (decision == "confirm")
        {
            requirement.ReviewStatus = "confirmed";
        }
        else if (decision == "not_applicable")
        {
            requirement.ReviewStatus = "not_applicable";
        }
        else if (decision == "missing")
        {
            requirement.ReviewStatus = "missing";
        }
        else
        {
            requirement.ReviewStatus = "unclear";
        }

        requirement.Analysis.UpdatedAt = DateTime.UtcNow;
        var siblings = await db.Requirements.Where(item => item.AnalysisId == requirement.AnalysisId).ToListAsync(cancellationToken);
        var allDecided = siblings.All(item => item.Id == requirement.Id || item.ReviewerDecision is not null);
        if (allDecided)
        {
            var unresolved = siblings.Any(item =>
            {
                var status = item.Id == requirement.Id ? requirement.ReviewStatus : item.ReviewStatus;
                return status is "missing" or "expired" or "unclear" or "not_reviewed" or "evidence_suggested";
            });
            requirement.Analysis.Status = unresolved ? "completed_with_unresolved_items" : "completed";
            requirement.Analysis.ProgressStage = requirement.Analysis.Status;
        }
        else
        {
            requirement.Analysis.Status = "needs_review";
        }

        await db.SaveChangesAsync(cancellationToken);
        var files = await db.Files.Include(file => file.Pages).Where(file => file.AnalysisId == requirement.AnalysisId).ToListAsync(cancellationToken);
        return Results.Ok(RequirementDto(requirement, files.ToDictionary(file => file.Id)));
    }

    private static async Task<IResult> Export(Guid analysisId, AppDbContext db, CancellationToken cancellationToken)
    {
        var analysis = await Load(db, analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        if (analysis.Status is "queued" or "processing" or "failed" or "cancelled" || analysis.Requirements.Count == 0)
        {
            return Error(
                "export_failed",
                "Export is available after a checklist exists. A failed or unfinished analysis was not turned into an empty file.",
                analysis.ErrorRetryable,
                409);
        }

        var bytes = CsvExporter.Build(analysis);
        return Results.File(bytes, "text/csv; charset=utf-8", "tender-checklist.csv");
    }

    private static async Task<IResult> DeleteAnalysis(
        Guid analysisId,
        AppDbContext db,
        FileStore store,
        CancellationToken cancellationToken)
    {
        var analysis = await db.Analyses.FirstOrDefaultAsync(item => item.Id == analysisId, cancellationToken);
        if (analysis is null)
        {
            return Missing();
        }

        db.Analyses.Remove(analysis);
        await db.SaveChangesAsync(cancellationToken);
        store.DeleteAnalysis(analysisId);
        return Results.Ok(new { status = "deleted" });
    }

    private static async Task<AnalysisEntity?> Load(AppDbContext db, Guid id, CancellationToken cancellationToken)
    {
        return await db.Analyses
            .Include(item => item.Files)
            .ThenInclude(file => file.Pages)
            .Include(item => item.Requirements)
            .ThenInclude(requirement => requirement.Matches)
            .Include(item => item.Jobs)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private static object FileDto(StoredFileEntity file) => new
    {
        file_id = file.Id,
        original_name = file.OriginalName,
        role = file.Role,
        size_bytes = file.SizeBytes,
        page_count = file.PageCount,
        sha256 = file.Sha256,
        status = file.Status,
        duplicate_of_file_id = file.DuplicateOfFileId,
        unreadable_pages = file.Pages.Where(page => !page.Usable).Select(page => page.PageNumber).ToArray()
    };

    private static object RequirementDto(RequirementEntity requirement, IReadOnlyDictionary<Guid, StoredFileEntity> files)
    {
        files.TryGetValue(requirement.SourceFileId ?? Guid.Empty, out var source);
        return new
        {
            requirement_id = requirement.Id,
            statement = requirement.Statement,
            edited_statement = requirement.EditedStatement,
            requirement_class = requirement.RequirementClass,
            mandatory_label = requirement.MandatoryLabel,
            source_file_id = requirement.SourceFileId,
            source_file_name = source?.OriginalName,
            page_number = requirement.PageNumber,
            quote = requirement.Quote,
            quote_verified = requirement.QuoteVerified,
            needs_human_review = requirement.NeedsHumanReview,
            review_reason = requirement.ReviewReason,
            review_status = requirement.ReviewStatus,
            reviewer_decision = requirement.ReviewerDecision,
            human_note = requirement.HumanNote,
            reviewed_at = requirement.ReviewedAt,
            ai_suggestion = new
            {
                statement = requirement.Statement,
                review_status = requirement.ReviewerDecision is null ? requirement.ReviewStatus : "suggestion_preserved"
            },
            evidence = requirement.Matches.Select(match =>
            {
                files.TryGetValue(match.EvidenceFileId ?? Guid.Empty, out var evidence);
                return new
                {
                    evidence_file_id = match.EvidenceFileId,
                    evidence_file_name = evidence?.OriginalName,
                    page_number = match.PageNumber,
                    quote = match.Quote,
                    quote_verified = match.QuoteVerified,
                    explanation = match.Explanation,
                    date_observation = match.DateObservation,
                    status = match.Status,
                    needs_human_review = match.NeedsHumanReview,
                    review_reason = match.ReviewReason
                };
            })
        };
    }

    private static IReadOnlyList<string> Warnings(AnalysisEntity analysis)
    {
        var warnings = new List<string>();
        if (analysis.ProviderId == "demo-extractor")
        {
            warnings.Add("demo_extractor");
        }

        foreach (var file in analysis.Files)
        {
            foreach (var page in file.Pages.Where(page => !page.Usable))
            {
                warnings.Add($"unreadable:{file.OriginalName}:p{page.PageNumber}");
            }

            if (file.DuplicateOfFileId is not null)
            {
                warnings.Add($"duplicate:{file.OriginalName}");
            }
        }

        return warnings;
    }

    private static IResult Missing() =>
        Error("analysis_not_found", "That analysis does not exist, or it was deleted.", false, 404);

    private static IResult Error(string code, string message, bool retryable, int status) =>
        Results.Json(new
        {
            code,
            user_message = message,
            retryable,
            request_id = Guid.NewGuid().ToString("n")
        }, statusCode: status);
}

public sealed class ReviewPatch
{
    public string? Decision { get; set; }
    public string? Statement { get; set; }
    public string? Note { get; set; }
}
