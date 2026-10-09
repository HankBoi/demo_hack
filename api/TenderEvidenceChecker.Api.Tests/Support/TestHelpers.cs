using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TenderEvidenceChecker.Api.Services;

namespace TenderEvidenceChecker.Api.Tests.Support;

public static class TestHelpers
{
    public const string TenderText = "Supplier must provide a registration certificate for the tender.";

    public static MultipartFormDataContent Form(
        string label,
        byte[] tender,
        string? language = null,
        bool sample = false,
        IEnumerable<Guid>? documentIds = null,
        IEnumerable<(string Name, byte[] Bytes)>? files = null,
        string tenderName = "tender.pdf")
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(label), "label");
        if (language is not null)
        {
            content.Add(new StringContent(language), "language");
        }

        if (sample)
        {
            content.Add(new StringContent("true"), "sample");
        }

        foreach (var id in documentIds ?? [])
        {
            content.Add(new StringContent(id.ToString()), "document_ids");
        }

        content.Add(Pdf(tender), "file", tenderName);
        foreach (var (name, bytes) in files ?? [])
        {
            content.Add(Pdf(bytes), "files", name);
        }

        return content;
    }

    public static ByteArrayContent Pdf(byte[] bytes)
    {
        var part = new ByteArrayContent(bytes);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        return part;
    }

    public static async Task<HttpResponseMessage> CreateAnalysis(
        HttpClient client,
        string label = "Tender",
        byte[]? tender = null,
        string? language = null,
        bool sample = false,
        IEnumerable<Guid>? documentIds = null,
        IEnumerable<(string Name, byte[] Bytes)>? files = null)
    {
        using var content = Form(label, tender ?? SimplePdf.Create(TenderText), language, sample, documentIds, files);
        return await client.PostAsync("/api/analyses", content);
    }

    public static async Task<Guid> CreateAndDrain(ApiFactory factory, HttpClient client, string label = "Tender", bool sample = false)
    {
        var response = await CreateAnalysis(client, label, sample: sample);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Created, body);
        var id = JsonSerializer.Deserialize<JsonElement>(body).GetProperty("analysis_id").GetGuid();
        await factory.DrainAsync();
        return id;
    }

    public static async Task<JsonElement> Json(HttpClient client, string url) =>
        await client.GetFromJsonAsync<JsonElement>(url);

    public static async Task<JsonElement> ReadJson(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    public static async Task<Guid> UploadDocument(HttpClient client, string name, byte[] bytes)
    {
        using var content = new MultipartFormDataContent();
        content.Add(Pdf(bytes), "files", name);
        var response = await client.PostAsync("/api/documents", content);
        var body = await ReadJson(response);
        return body.GetProperty("saved")[0].GetProperty("document_id").GetGuid();
    }
}

/// <summary>Returns exactly the findings a test supplies, so citation checks can be exercised with known good and bad quotes.</summary>
public sealed class ScriptedProvider(Func<IReadOnlyList<SourcePage>, string, List<ModelRequirement>> extract) : IModelProvider
{
    public string ProviderId => "scripted-test";
    public string ModelId => "scripted-test-1";
    public bool IsConfigured => true;
    public string? LastLanguage { get; private set; }

    public Task<ModelRequirementDocument> ExtractRequirementsAsync(IReadOnlyList<SourcePage> pages, string language, CancellationToken cancellationToken)
    {
        LastLanguage = language;
        return Task.FromResult(new ModelRequirementDocument { Requirements = extract(pages, language) });
    }

    public Task<ModelEvidenceDocument> MatchEvidenceAsync(IReadOnlyList<RequirementPrompt> requirements, IReadOnlyList<SourcePage> evidencePages, string language, CancellationToken cancellationToken) =>
        Task.FromResult(new ModelEvidenceDocument());
}
