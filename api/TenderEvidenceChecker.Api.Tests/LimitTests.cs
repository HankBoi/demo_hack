using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TenderEvidenceChecker.Api.Tests.Support;

namespace TenderEvidenceChecker.Api.Tests;

public class LimitTests : IClassFixture<TinyLimitFactory>
{
    private readonly TinyLimitFactory _factory;

    public LimitTests(TinyLimitFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Oversized_file_is_rejected()
    {
        var bytes = new byte[(1024 * 1024) + 4096];
        "%PDF-1.4\n"u8.CopyTo(bytes);
        var response = await Post(bytes, "big.pdf");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("file_too_large", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Too_many_pages_is_rejected()
    {
        var response = await Post(SimplePdf.Create("Page one must include a registration certificate.", "Page two must include a second certificate."), "two.pdf");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("page_limit_exceeded", body.GetProperty("code").GetString());
    }

    private async Task<HttpResponseMessage> Post(byte[] pdf, string name)
    {
        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Limit tender"), "label");
        var file = new ByteArrayContent(pdf);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(file, "file", name);
        var response = await client.PostAsync("/api/analyses", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return response;
    }
}
