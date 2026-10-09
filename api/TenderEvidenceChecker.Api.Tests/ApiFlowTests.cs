using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TenderEvidenceChecker.Api.Tests.Support;

namespace TenderEvidenceChecker.Api.Tests;

public class ApiFlowTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ApiFlowTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_is_ok_without_secrets()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ok", json.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Valid_pdf_keeps_page_numbers_and_text()
    {
        var client = _factory.CreateClient();
        var id = await CreateAsync(client, "Kitabxana mebeli", SimplePdf.Create(
            "Supplier must provide a registration certificate for the tender.",
            "Delivery shall reach the library before the stated date."));
        await _factory.DrainAsync();

        var analysis = await client.GetFromJsonAsync<JsonElement>($"/api/analyses/{id}");
        Assert.Equal("needs_review", analysis.GetProperty("status").GetString());
        Assert.Equal(2, analysis.GetProperty("counts").GetProperty("usable_pages").GetInt32());

        var pages = await client.GetFromJsonAsync<JsonElement>($"/api/analyses/{id}/pages");
        var list = pages.GetProperty("pages").EnumerateArray().ToList();
        Assert.Equal(1, list[0].GetProperty("page_number").GetInt32());
        Assert.Contains("registration certificate", list[0].GetProperty("preview").GetString(), StringComparison.Ordinal);
        Assert.Equal(2, list[1].GetProperty("page_number").GetInt32());
    }

    [Fact]
    public async Task Rejects_bad_files()
    {
        var client = _factory.CreateClient();
        await AssertCode(client, "notes.txt", "hello"u8.ToArray(), "unsupported_file_type");
        await AssertCode(client, "broken.pdf", "%PDF-1.4\nnot-a-real-pdf"u8.ToArray(), "pdf_unreadable");
    }

    [Fact]
    public async Task Empty_text_pdf_fails_after_upload_without_a_success_checklist()
    {
        var client = _factory.CreateClient();
        var id = await CreateAsync(client, "Boş skan", SimplePdf.Create(""));
        await _factory.DrainAsync();
        var analysis = await client.GetFromJsonAsync<JsonElement>($"/api/analyses/{id}");
        Assert.Equal("failed", analysis.GetProperty("status").GetString());
        Assert.Equal("pdf_unreadable", analysis.GetProperty("error").GetProperty("code").GetString());
        Assert.Contains("No readable text", analysis.GetProperty("error").GetProperty("user_message").GetString(), StringComparison.Ordinal);
        var requirements = await client.GetFromJsonAsync<JsonElement>($"/api/analyses/{id}/requirements");
        Assert.Equal(0, requirements.GetProperty("requirements").GetArrayLength());
    }

    [Fact]
    public async Task Encrypted_pdf_is_rejected()
    {
        var client = _factory.CreateClient();
        var response = await PostPdf(client, "locked.pdf", SimplePdf.CreateEncryptedPlaceholder());
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(
            body.GetProperty("code").GetString() is "pdf_encrypted" or "pdf_unreadable",
            body.ToString());
        Assert.False(body.GetProperty("user_message").GetString()!.Contains("/tmp", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Delete_removes_the_analysis()
    {
        var client = _factory.CreateClient();
        var id = await CreateAsync(client, "Silinəcək", SimplePdf.Create("Supplier must provide a registration certificate for the tender."));
        var deleted = await client.DeleteAsync($"/api/analyses/{id}");
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        var missing = await client.GetAsync($"/api/analyses/{id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private async Task AssertCode(HttpClient client, string name, byte[] bytes, string code)
    {
        var response = await PostPdf(client, name, bytes);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(code, body.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("user_message").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("request_id").GetString()));
    }

    private static async Task<Guid> CreateAsync(HttpClient client, string label, byte[] pdf)
    {
        var response = await PostPdf(client, "tender.pdf", pdf, label);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("analysis_id").GetGuid();
    }

    private static async Task<HttpResponseMessage> PostPdf(HttpClient client, string name, byte[] pdf, string? label = null)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(label ?? "Tender"), "label");
        var file = new ByteArrayContent(pdf);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(file, "file", name);
        return await client.PostAsync("/api/analyses", content);
    }
}
