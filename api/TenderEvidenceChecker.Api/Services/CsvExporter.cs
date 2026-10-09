using System.Text;
using TenderEvidenceChecker.Api.Data;

namespace TenderEvidenceChecker.Api.Services;

public static class CsvExporter
{
    private static readonly string[] Headers =
    [
        "draft_label",
        "analysis_label",
        "requirement_id",
        "statement",
        "requirement_class",
        "mandatory_label",
        "source_file",
        "source_page",
        "source_quote",
        "quote_verified",
        "evidence_file",
        "evidence_page",
        "evidence_quote",
        "evidence_status",
        "human_decision",
        "human_note",
        "review_status",
        "limitations"
    ];

    public static byte[] Build(AnalysisEntity analysis)
    {
        var files = analysis.Files.ToDictionary(file => file.Id);
        var limitations = string.Join("; ", analysis.Files
            .SelectMany(file => file.Pages.Where(page => !page.Usable).Select(page => $"{file.OriginalName} p.{page.PageNumber} unreadable")));
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', Headers.Select(Escape)));

        foreach (var requirement in analysis.Requirements.OrderBy(item => item.PageNumber).ThenBy(item => item.Statement))
        {
            var matches = requirement.Matches.Count == 0 ? [null] : requirement.Matches.Cast<EvidenceMatchEntity?>().ToList();
            foreach (var match in matches)
            {
                StoredFileEntity? evidenceFile = null;
                if (match?.EvidenceFileId is Guid evidenceId)
                {
                    files.TryGetValue(evidenceId, out evidenceFile);
                }

                StoredFileEntity? sourceFile = null;
                if (requirement.SourceFileId is Guid sourceId)
                {
                    files.TryGetValue(sourceId, out sourceFile);
                }

                string?[] cells =
                [
                    "AI-assisted draft / human-reviewed",
                    analysis.Label,
                    requirement.Id.ToString(),
                    requirement.EditedStatement ?? requirement.Statement,
                    requirement.RequirementClass,
                    requirement.MandatoryLabel,
                    sourceFile?.OriginalName,
                    requirement.PageNumber?.ToString(),
                    requirement.QuoteVerified ? requirement.Quote : "",
                    requirement.QuoteVerified ? "verified" : "unverified",
                    evidenceFile?.OriginalName,
                    match?.PageNumber?.ToString(),
                    match is { QuoteVerified: true } ? match.Quote : "",
                    match?.Status,
                    requirement.ReviewerDecision,
                    requirement.HumanNote,
                    requirement.ReviewStatus,
                    limitations
                ];
                builder.AppendLine(string.Join(',', cells.Select(Escape)));
            }
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(builder.ToString());
        var bytes = new byte[preamble.Length + body.Length];
        preamble.CopyTo(bytes, 0);
        body.CopyTo(bytes, preamble.Length);
        return bytes;
    }

    public static string Escape(string? value)
    {
        var text = value ?? "";
        if (text.Length > 0 && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            text = "'" + text;
        }

        return "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}
