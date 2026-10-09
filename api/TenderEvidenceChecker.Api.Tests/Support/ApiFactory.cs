using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TenderEvidenceChecker.Api.Services;

namespace TenderEvidenceChecker.Api.Tests.Support;

public class ApiFactory : WebApplicationFactory<Program>
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "tec-" + Guid.NewGuid().ToString("N"));

    public ApiFactory()
    {
        Directory.CreateDirectory(Root);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        foreach (var (key, value) in Settings())
        {
            builder.UseSetting(key, value);
        }
    }

    protected virtual Dictionary<string, string?> Settings() => new()
    {
        ["App:DatabaseUrl"] = $"Data Source={Path.Combine(Root, "t.db")}",
        ["App:PrivateUploadDir"] = Path.Combine(Root, "uploads"),
        ["App:ReferenceDate"] = "2026-10-09",
        ["App:AnthropicApiKey"] = "",
        ["App:MaxUploadMb"] = "1",
        ["App:MaxTenderPages"] = "12",
        ["App:MaxEvidenceFiles"] = "8",
        ["App:RetentionHours"] = "48",
        ["App:OcrEnabled"] = "false"
    };

    public async Task DrainAsync()
    {
        using var scope = Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<AnalysisProcessor>();
        for (var attempt = 0; attempt < 8; attempt++)
        {
            if (!await processor.TryProcessNext(CancellationToken.None))
            {
                return;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
        catch (IOException)
        {
            // The test host can release the sqlite file a moment later.
        }
    }
}

public sealed class TinyLimitFactory : ApiFactory
{
    protected override Dictionary<string, string?> Settings()
    {
        var settings = base.Settings();
        settings["App:MaxUploadMb"] = "1";
        settings["App:MaxTenderPages"] = "1";
        return settings;
    }
}
