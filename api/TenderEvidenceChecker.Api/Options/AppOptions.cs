namespace TenderEvidenceChecker.Api.Options;

public sealed class AppOptions
{
    public string DatabaseUrl { get; set; } = "";
    public string PrivateUploadDir { get; set; } = "";
    public int MaxUploadMb { get; set; } = 15;
    public int MaxTenderPages { get; set; } = 40;
    public int MaxEvidenceFiles { get; set; } = 8;
    public bool OcrEnabled { get; set; }
    public int RetentionHours { get; set; } = 24;
    public string ClaudeModelId { get; set; } = "claude-sonnet-4-5";
    public string PromptVersion { get; set; } = "2026-10-09.1";
    public string AnthropicApiKey { get; set; } = "";
    public int ModelTimeoutSeconds { get; set; } = 45;
    public DateOnly ReferenceDate { get; set; } = new(2026, 10, 9);

    public long MaxUploadBytes => Math.Max(1, MaxUploadMb) * 1024L * 1024L;
}
