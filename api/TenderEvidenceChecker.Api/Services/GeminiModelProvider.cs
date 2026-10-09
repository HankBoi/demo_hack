using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TenderEvidenceChecker.Api.Options;

namespace TenderEvidenceChecker.Api.Services;

/// <summary>
/// Server-side Google Gemini provider (generateContent with a JSON response schema).
/// The API key is sent in a request header and is never logged or returned to the browser.
/// Model output is untrusted: it is parsed again here and validated by the processor.
/// </summary>
public sealed class GeminiModelProvider(HttpClient http, IOptions<AppOptions> options) : IModelProvider
{
    private const string Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/";
    private const int MaxAttempts = 3;
    private const int ExtractionChunkChars = 40_000;

    public string ProviderId => "gemini";
    public string ModelId => options.Value.GeminiModelId;
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.GeminiApiKey);

    public async Task<ModelRequirementDocument> ExtractRequirementsAsync(
        IReadOnlyList<SourcePage> pages,
        string language,
        CancellationToken cancellationToken)
    {
        var system = ModelPrompts.ExtractionSystem(language);
        var combined = new ModelRequirementDocument();
        foreach (var group in Chunk(pages, ExtractionChunkChars))
        {
            var payload = group.Select(page => new
            {
                source_file_id = page.FileId,
                file_name = page.FileName,
                page_number = page.PageNumber,
                text = page.Text
            });
            var user = "pages:\n" + JsonSerializer.Serialize(payload);
            var parsed = await CompleteAsync(system, user, RequirementSchema(), ModelOutputParser.ParseRequirements, cancellationToken);
            combined.Requirements.AddRange(parsed.Requirements);
        }

        return combined;
    }

    public async Task<ModelEvidenceDocument> MatchEvidenceAsync(
        IReadOnlyList<RequirementPrompt> requirements,
        IReadOnlyList<SourcePage> evidencePages,
        string language,
        CancellationToken cancellationToken)
    {
        var system = ModelPrompts.MatchSystem(language);
        var user = "requirements:\n" + JsonSerializer.Serialize(requirements.Select(requirement => new
        {
            requirement_id = requirement.RequirementId,
            requirement_class = requirement.RequirementClass,
            statement = requirement.Statement,
            quote = requirement.Quote
        })) + "\nevidence_pages:\n" + JsonSerializer.Serialize(evidencePages.Where(page => page.Usable).Select(page => new
        {
            evidence_file_id = page.FileId,
            file_name = page.FileName,
            page_number = page.PageNumber,
            text = page.Text
        }));
        return await CompleteAsync(system, user, EvidenceSchema(), ModelOutputParser.ParseEvidence, cancellationToken);
    }

    private async Task<T> CompleteAsync<T>(
        string system,
        string user,
        object schema,
        Func<string, T> parse,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw UnconfiguredModelProvider.NotConfigured();
        }

        if (user.Length + system.Length > options.Value.MaxModelRequestChars)
        {
            throw new AppException(
                "request_too_large",
                "The documents are too large to send to the model in one request. Use fewer or shorter documents.",
                retryable: false,
                statusCode: 413);
        }

        AppException? last = null;
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            if (attempt > 0)
            {
                await Task.Delay(options.Value.ModelRetryDelayMs * attempt, cancellationToken);
            }

            var prompt = last is { Code: "model_output_invalid" }
                ? user + "\n\nThe previous response was not valid for the schema. Return only the JSON object."
                : user;
            try
            {
                var text = await SendAsync(system, prompt, schema, cancellationToken);
                return parse(text);
            }
            catch (AppException ex) when (ex.Code is "model_output_invalid" or "model_timeout" or "model_rate_limited" && attempt < MaxAttempts - 1)
            {
                last = ex;
            }
        }

        throw last ?? new AppException("model_output_invalid", "The model response could not be used. You can retry.", true, 502);
    }

    private async Task<string> SendAsync(string system, string user, object schema, CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?>
        {
            ["systemInstruction"] = new { parts = new[] { new { text = system } } },
            ["contents"] = new[] { new { role = "user", parts = new[] { new { text = user } } } },
            ["generationConfig"] = new Dictionary<string, object?>
            {
                ["temperature"] = 0,
                ["maxOutputTokens"] = 16384,
                ["responseMimeType"] = "application/json",
                ["responseSchema"] = schema
            }
        };

        var url = Endpoint + Uri.EscapeDataString(options.Value.GeminiModelId) + ":generateContent";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("x-goog-api-key", options.Value.GeminiApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw new AppException("model_timeout", "The model timed out. You can retry. No partial checklist was saved.", true, 504);
        }
        catch (HttpRequestException)
        {
            throw new AppException("model_timeout", "The model service could not be reached. You can retry.", true, 502);
        }

        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            switch (response.StatusCode)
            {
                case HttpStatusCode.TooManyRequests:
                    throw new AppException("model_rate_limited", "The model provider rate-limited this request. Wait a moment and retry.", true, 429);
                case HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden:
                    throw new AppException("model_auth_failed", "The model provider rejected the server API key. Check GEMINI_API_KEY and its permissions.", false, 502);
                case HttpStatusCode.NotFound:
                    throw new AppException("model_rejected", "The model provider does not know the configured model id. Check GEMINI_MODEL_ID.", false, 502);
            }

            if ((int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout)
            {
                throw new AppException("model_timeout", "The model provider returned an error. You can retry.", true, 502);
            }

            if (!response.IsSuccessStatusCode)
            {
                var invalidKey = raw.Contains("API_KEY_INVALID", StringComparison.Ordinal);
                throw invalidKey
                    ? new AppException("model_auth_failed", "The model provider rejected the server API key. Check GEMINI_API_KEY and its permissions.", false, 502)
                    : new AppException("model_rejected", "The model request was rejected. Check the configured model id and retry.", false, 502);
            }

            return ReadText(raw);
        }
    }

    private static string ReadText(string raw)
    {
        try
        {
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;
            if (!root.TryGetProperty("candidates", out var candidates)
                || candidates.ValueKind != JsonValueKind.Array
                || candidates.GetArrayLength() == 0)
            {
                throw new JsonException("no candidates");
            }

            var candidate = candidates[0];
            if (candidate.TryGetProperty("finishReason", out var reason)
                && reason.GetString() is "MAX_TOKENS" or "SAFETY" or "RECITATION" or "PROHIBITED_CONTENT" or "BLOCKLIST")
            {
                throw new JsonException("unfinished");
            }

            var text = new StringBuilder();
            if (candidate.TryGetProperty("content", out var content)
                && content.TryGetProperty("parts", out var parts)
                && parts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var node) && node.ValueKind == JsonValueKind.String)
                    {
                        text.Append(node.GetString());
                    }
                }
            }

            if (text.Length == 0)
            {
                throw new JsonException("empty");
            }

            return text.ToString();
        }
        catch (JsonException)
        {
            throw new AppException("model_output_invalid", "The model response could not be read. You can retry.", true, 502);
        }
    }

    private static Dictionary<string, object?> Str(string? description = null, bool nullable = false, string[]? values = null)
    {
        var node = new Dictionary<string, object?> { ["type"] = "STRING" };
        if (nullable)
        {
            node["nullable"] = true;
        }

        if (values is not null)
        {
            node["enum"] = values;
        }

        if (description is not null)
        {
            node["description"] = description;
        }

        return node;
    }

    private static Dictionary<string, object?> Obj(Dictionary<string, object?> properties, string[] required) => new()
    {
        ["type"] = "OBJECT",
        ["properties"] = properties,
        ["required"] = required
    };

    private static object RequirementSchema()
    {
        var item = Obj(new Dictionary<string, object?>
        {
            ["kind"] = Str(values: ["requirement", "risk"]),
            ["category"] = Str(values: ["required_document", "deadline", "technical_mismatch", "contract_terms", "conflicting_unclear", "other"]),
            ["severity"] = Str(values: ["high", "medium", "low", "uncertain"]),
            ["statement"] = Str(),
            ["explanation"] = Str(),
            ["possible_impact"] = Str(),
            ["next_step"] = Str(),
            ["requirement_class"] = Str(values: ["mandatory", "evaluated", "informational", "uncertain"]),
            ["mandatory_label"] = Str(values: ["explicit", "conditional", "unclear"]),
            ["source_file_id"] = Str(),
            ["page_number"] = new Dictionary<string, object?> { ["type"] = "INTEGER" },
            ["quote"] = Str("Exact text copied from the page"),
            ["due_date_text"] = Str(nullable: true),
            ["evidence_type"] = Str(nullable: true),
            ["needs_human_review"] = new Dictionary<string, object?> { ["type"] = "BOOLEAN" },
            ["review_reason"] = Str()
        },
        [
            "kind", "category", "severity", "statement", "explanation", "possible_impact", "next_step",
            "requirement_class", "mandatory_label", "source_file_id", "page_number", "quote",
            "needs_human_review", "review_reason"
        ]);
        return Obj(new Dictionary<string, object?>
        {
            ["requirements"] = new Dictionary<string, object?> { ["type"] = "ARRAY", ["items"] = item }
        }, ["requirements"]);
    }

    private static object EvidenceSchema()
    {
        var item = Obj(new Dictionary<string, object?>
        {
            ["requirement_id"] = Str(),
            ["status"] = Str(values: ["possible_match", "possible_mismatch", "not_found", "unclear"]),
            ["evidence_file_id"] = Str(nullable: true),
            ["page_number"] = new Dictionary<string, object?> { ["type"] = "INTEGER", ["nullable"] = true },
            ["quote"] = Str(nullable: true),
            ["explanation"] = Str(),
            ["date_observation"] = Str(nullable: true),
            ["needs_human_review"] = new Dictionary<string, object?> { ["type"] = "BOOLEAN" },
            ["review_reason"] = Str()
        },
        ["requirement_id", "status", "explanation", "needs_human_review", "review_reason"]);
        return Obj(new Dictionary<string, object?>
        {
            ["matches"] = new Dictionary<string, object?> { ["type"] = "ARRAY", ["items"] = item }
        }, ["matches"]);
    }

    private static List<List<SourcePage>> Chunk(IReadOnlyList<SourcePage> pages, int maxChars)
    {
        var groups = new List<List<SourcePage>>();
        var current = new List<SourcePage>();
        var size = 0;
        foreach (var page in pages.Where(page => page.Usable))
        {
            if (current.Count > 0 && size + page.Text.Length > maxChars)
            {
                groups.Add(current);
                current = [];
                size = 0;
            }

            current.Add(page);
            size += page.Text.Length;
        }

        if (current.Count > 0)
        {
            groups.Add(current);
        }

        return groups.Count == 0 ? [[]] : groups;
    }
}
