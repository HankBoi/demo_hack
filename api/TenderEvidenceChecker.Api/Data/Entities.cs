namespace TenderEvidenceChecker.Api.Data;

public sealed class AnalysisEntity
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
    public string Status { get; set; } = "queued";
    public string ProgressStage { get; set; } = "queued";
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ErrorRetryable { get; set; }
    public string? ProviderId { get; set; }
    public string? ModelId { get; set; }
    public string PromptVersion { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<StoredFileEntity> Files { get; set; } = [];
    public List<RequirementEntity> Requirements { get; set; } = [];
    public List<JobEntity> Jobs { get; set; } = [];
}

public sealed class StoredFileEntity
{
    public Guid Id { get; set; }
    public Guid AnalysisId { get; set; }
    public AnalysisEntity? Analysis { get; set; }
    public string Role { get; set; } = "tender";
    public string OriginalName { get; set; } = "";
    public string StoredName { get; set; } = "";
    public string Sha256 { get; set; } = "";
    public long SizeBytes { get; set; }
    public int PageCount { get; set; }
    public string Status { get; set; } = "stored";
    public Guid? DuplicateOfFileId { get; set; }
    public List<PageTextEntity> Pages { get; set; } = [];
}

public sealed class PageTextEntity
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public StoredFileEntity? File { get; set; }
    public int PageNumber { get; set; }
    public string Text { get; set; } = "";
    public string ExtractionMethod { get; set; } = "none";
    public string OcrStatus { get; set; } = "not_needed";
    public bool Usable { get; set; }
}

public sealed class RequirementEntity
{
    public Guid Id { get; set; }
    public Guid AnalysisId { get; set; }
    public AnalysisEntity? Analysis { get; set; }
    public string Statement { get; set; } = "";
    public string? EditedStatement { get; set; }
    public string RequirementClass { get; set; } = "uncertain";
    public string MandatoryLabel { get; set; } = "unclear";
    public Guid? SourceFileId { get; set; }
    public int? PageNumber { get; set; }
    public string? Quote { get; set; }
    public bool QuoteVerified { get; set; }
    public string? DueDateText { get; set; }
    public string? EvidenceType { get; set; }
    public bool NeedsHumanReview { get; set; } = true;
    public string? ReviewReason { get; set; }
    public string ReviewStatus { get; set; } = "not_reviewed";
    public string? ReviewerDecision { get; set; }
    public string? HumanNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string AiSnapshotJson { get; set; } = "";
    public List<EvidenceMatchEntity> Matches { get; set; } = [];
}

public sealed class EvidenceMatchEntity
{
    public Guid Id { get; set; }
    public Guid RequirementId { get; set; }
    public RequirementEntity? Requirement { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public int? PageNumber { get; set; }
    public string? Quote { get; set; }
    public bool QuoteVerified { get; set; }
    public string? Explanation { get; set; }
    public string? DateObservation { get; set; }
    public string Status { get; set; } = "unclear";
    public bool NeedsHumanReview { get; set; } = true;
    public string? ReviewReason { get; set; }
}

public sealed class JobEntity
{
    public Guid Id { get; set; }
    public Guid AnalysisId { get; set; }
    public AnalysisEntity? Analysis { get; set; }
    public string Kind { get; set; } = "extract";
    public string Status { get; set; } = "queued";
    public int Attempts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? ErrorCode { get; set; }
}
