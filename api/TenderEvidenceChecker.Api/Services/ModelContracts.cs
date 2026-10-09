namespace TenderEvidenceChecker.Api.Services;

public sealed class SourcePage
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = "";
    public int PageNumber { get; set; }
    public string Text { get; set; } = "";
    public bool Usable { get; set; }
}

/// <summary>
/// One suggested finding returned by a model. Nothing here is trusted until the processor validates
/// the page and quote against extracted page text.
/// </summary>
public sealed class ModelRequirement
{
    public string Kind { get; set; } = "requirement";
    public string Category { get; set; } = "other";
    public string Severity { get; set; } = "uncertain";
    public string Statement { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string PossibleImpact { get; set; } = "";
    public string NextStep { get; set; } = "";
    public string RequirementClass { get; set; } = "uncertain";
    public string MandatoryLabel { get; set; } = "unclear";
    public string SourceFileId { get; set; } = "";
    public int PageNumber { get; set; }
    public string Quote { get; set; } = "";
    public string? DueDateText { get; set; }
    public string? EvidenceType { get; set; }
    public bool NeedsHumanReview { get; set; } = true;
    public string? ReviewReason { get; set; }
}

public sealed class ModelRequirementDocument
{
    public List<ModelRequirement> Requirements { get; set; } = [];
}

public sealed class ModelEvidenceLink
{
    public string RequirementId { get; set; } = "";
    public string? EvidenceFileId { get; set; }
    public int? PageNumber { get; set; }
    public string? Quote { get; set; }
    public string? Explanation { get; set; }
    public string? DateObservation { get; set; }
    public bool NeedsHumanReview { get; set; } = true;
    public string? ReviewReason { get; set; }
    public string Status { get; set; } = "unclear";
}

public sealed class ModelEvidenceDocument
{
    public List<ModelEvidenceLink> Matches { get; set; } = [];
}

public sealed class RequirementPrompt
{
    public Guid RequirementId { get; set; }
    public string Statement { get; set; } = "";
    public string RequirementClass { get; set; } = "";
    public string Quote { get; set; } = "";
}

public static class FindingVocabulary
{
    public static readonly HashSet<string> Kinds = new(StringComparer.OrdinalIgnoreCase) { "requirement", "risk" };

    public static readonly HashSet<string> Categories = new(StringComparer.OrdinalIgnoreCase)
    {
        "required_document", "deadline", "technical_mismatch", "contract_terms", "conflicting_unclear", "other"
    };

    public static readonly HashSet<string> Severities = new(StringComparer.OrdinalIgnoreCase)
    {
        "high", "medium", "low", "uncertain"
    };

    public static readonly HashSet<string> Languages = new(StringComparer.OrdinalIgnoreCase) { "az", "ru", "en" };

    public static string NormalizeLanguage(string? value) =>
        value is not null && Languages.Contains(value.Trim()) ? value.Trim().ToLowerInvariant() : "az";

    public static string LanguageName(string language) => language switch
    {
        "ru" => "Russian",
        "en" => "English",
        _ => "Azerbaijani"
    };
}

public interface IModelProvider
{
    string ProviderId { get; }
    string ModelId { get; }

    /// <summary>False when no live model is configured. The app then shows a setup message instead of results.</summary>
    bool IsConfigured { get; }

    Task<ModelRequirementDocument> ExtractRequirementsAsync(
        IReadOnlyList<SourcePage> pages,
        string language,
        CancellationToken cancellationToken);

    Task<ModelEvidenceDocument> MatchEvidenceAsync(
        IReadOnlyList<RequirementPrompt> requirements,
        IReadOnlyList<SourcePage> evidencePages,
        string language,
        CancellationToken cancellationToken);
}
