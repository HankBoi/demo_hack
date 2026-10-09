using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TenderEvidenceChecker.Api.Data;
using TenderEvidenceChecker.Api.Services;

namespace TenderEvidenceChecker.Api.Tests.Support;

/// <summary>
/// Test double for the model boundary. By default it delegates to the rule-based DemoModelProvider
/// (never registered by the live host). Tests can switch it to fail or to report "not configured".
/// </summary>
public sealed class SwitchableProvider : IModelProvider
{
    public IModelProvider Inner { get; set; } = new DemoModelProvider();
    public AppException? Fail { get; set; }
    public bool Configured { get; set; } = true;
    public int Calls { get; private set; }

    public string ProviderId => Inner.ProviderId;
    public string ModelId => Inner.ModelId;
    public bool IsConfigured => Configured;

    public Task<ModelRequirementDocument> ExtractRequirementsAsync(IReadOnlyList<SourcePage> pages, string language, CancellationToken cancellationToken)
    {
        Calls++;
        return Fail is not null ? throw Fail : Inner.ExtractRequirementsAsync(pages, language, cancellationToken);
    }

    public Task<ModelEvidenceDocument> MatchEvidenceAsync(IReadOnlyList<RequirementPrompt> requirements, IReadOnlyList<SourcePage> evidencePages, string language, CancellationToken cancellationToken)
    {
        Calls++;
        return Fail is not null ? throw Fail : Inner.MatchEvidenceAsync(requirements, evidencePages, language, cancellationToken);
    }
}

public class ApiFactory : WebApplicationFactory<Program>
{
    static ApiFactory()
    {
        // A developer's local .env must never reach a test run.
        Environment.SetEnvironmentVariable("TEC_SKIP_DOTENV", "1");
    }

    public string Root { get; } = Path.Combine(Path.GetTempPath(), "tec-" + Guid.NewGuid().ToString("N"));
    public SwitchableProvider Provider { get; } = new();

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

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IModelProvider>();
            services.AddSingleton<IModelProvider>(Provider);
        });
    }

    protected virtual Dictionary<string, string?> Settings() => new()
    {
        ["App:DatabaseUrl"] = $"Data Source={Path.Combine(Root, "t.db")}",
        ["App:PrivateUploadDir"] = Path.Combine(Root, "uploads"),
        ["App:ReferenceDate"] = "2026-10-09",
        ["App:MaxUploadMb"] = "1",
        ["App:MaxTenderPages"] = "12",
        ["App:MaxEvidenceFiles"] = "8",
        ["App:RetentionHours"] = "48",
        ["App:OcrEnabled"] = "false",
        ["App:PaymentsMode"] = "demo",
        ["App:DemoMonthlyPriceAzn"] = "19",
        ["App:FreeAnalysisLimit"] = "3"
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

    public T WithDb<T>(Func<AppDbContext, T> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return action(db);
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
