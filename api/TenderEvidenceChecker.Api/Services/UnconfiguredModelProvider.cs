namespace TenderEvidenceChecker.Api.Services;

/// <summary>
/// Registered when no live model key is configured. It never fabricates findings: any call fails with a setup message.
/// </summary>
public sealed class UnconfiguredModelProvider : IModelProvider
{
    public const string SetupMessage =
        "No AI model key is configured on the server. Set GEMINI_API_KEY (and optionally GEMINI_MODEL_ID) in the server environment or the local .env file, then restart the API and the worker.";

    public string ProviderId => "none";
    public string ModelId => "";
    public bool IsConfigured => false;

    public Task<ModelRequirementDocument> ExtractRequirementsAsync(
        IReadOnlyList<SourcePage> pages,
        string language,
        CancellationToken cancellationToken) =>
        throw NotConfigured();

    public Task<ModelEvidenceDocument> MatchEvidenceAsync(
        IReadOnlyList<RequirementPrompt> requirements,
        IReadOnlyList<SourcePage> evidencePages,
        string language,
        CancellationToken cancellationToken) =>
        throw NotConfigured();

    public static AppException NotConfigured() =>
        new("model_not_configured", SetupMessage, retryable: false, statusCode: 503);
}
