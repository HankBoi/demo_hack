namespace TenderEvidenceChecker.Api.Services;

public sealed class SourcePage
{
    public Guid FileId { get; set; }
    public string FileName { get; set; } = "";
    public int PageNumber { get; set; }
    public string Text { get; set; } = "";
    public bool Usable { get; set; }
}

public sealed class ModelRequirement
{
    public string Statement { get; set; } = "";
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

public interface IModelProvider
{
    string ProviderId { get; }
    string ModelId { get; }
    Task<ModelRequirementDocument> ExtractRequirementsAsync(IReadOnlyList<SourcePage> pages, CancellationToken cancellationToken);
    Task<ModelEvidenceDocument> MatchEvidenceAsync(
        IReadOnlyList<RequirementPrompt> requirements,
        IReadOnlyList<SourcePage> evidencePages,
        CancellationToken cancellationToken);
}
