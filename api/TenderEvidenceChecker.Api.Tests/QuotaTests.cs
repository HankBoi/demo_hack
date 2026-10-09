using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TenderEvidenceChecker.Api.Data;
using TenderEvidenceChecker.Api.Services;
using TenderEvidenceChecker.Api.Tests.Support;
using static TenderEvidenceChecker.Api.Tests.Support.TestHelpers;

namespace TenderEvidenceChecker.Api.Tests;

public class QuotaTests
{
    [Fact]
    public async Task Fourth_analysis_requires_the_demo_plan_and_activation_unlocks_it()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var ids = new List<Guid>();
        for (var index = 0; index < 3; index++)
        {
            ids.Add(await CreateAndDrain(factory, client, $"Tender {index}"));
        }

        var quota = await Json(client, "/api/quota");
        Assert.Equal(3, quota.GetProperty("free_used").GetInt32());
        Assert.Equal(0, quota.GetProperty("free_remaining").GetInt32());
        Assert.False(quota.GetProperty("can_start").GetBoolean());
        Assert.False(quota.GetProperty("real_payment_processed").GetBoolean());
        Assert.True(quota.GetProperty("price_is_demo").GetBoolean());
        Assert.Equal(19m, quota.GetProperty("demo_price_azn").GetDecimal());

        var blocked = await CreateAnalysis(client, "Fourth");
        Assert.Equal(HttpStatusCode.PaymentRequired, blocked.StatusCode);
        Assert.Equal("plan_required", (await ReadJson(blocked)).GetProperty("code").GetString());
        var listed = await Json(client, "/api/analyses");
        Assert.Equal(3, listed.GetProperty("analyses").GetArrayLength());

        // Deleting a finished analysis does not hand the free allowance back.
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/api/analyses/{ids[0]}")).StatusCode);
        Assert.Equal(HttpStatusCode.PaymentRequired, (await CreateAnalysis(client, "After delete")).StatusCode);

        var activated = await client.PostAsync("/api/subscription/demo-activate", null);
        Assert.Equal(HttpStatusCode.OK, activated.StatusCode);
        var active = await ReadJson(activated);
        Assert.True(active.GetProperty("subscription_active").GetBoolean());
        var expires = active.GetProperty("subscription_expires_at").GetDateTime();
        Assert.InRange((expires - DateTime.UtcNow).TotalDays, 29.9, 30.1);
        Assert.Equal(HttpStatusCode.Created, (await CreateAnalysis(client, "Paid demo")).StatusCode);

        // The stored expiry is what gates access: once it passes, the plan screen is required again.
        factory.WithDb(db =>
        {
            db.Workspaces.Where(item => item.Id == Workspaces.LocalId)
                .ExecuteUpdate(set => set.SetProperty(item => item.SubscriptionExpiresAt, DateTime.UtcNow.AddMinutes(-1)));
            return 0;
        });
        var expired = await Json(client, "/api/quota");
        Assert.Equal("expired", expired.GetProperty("subscription_status").GetString());
        Assert.False(expired.GetProperty("can_start").GetBoolean());
        Assert.Equal(HttpStatusCode.PaymentRequired, (await CreateAnalysis(client, "Expired")).StatusCode);
    }

    [Fact]
    public async Task Failed_run_does_not_use_the_allowance_and_retry_counts_once()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();
        factory.Provider.Fail = new AppException("model_timeout", "The model timed out.", true, 504);
        var created = await CreateAnalysis(client, "Timeout tender");
        var id = (await ReadJson(created)).GetProperty("analysis_id").GetGuid();
        await factory.DrainAsync();

        var failed = await Json(client, $"/api/analyses/{id}");
        Assert.Equal("failed", failed.GetProperty("status").GetString());
        Assert.Equal("model_timeout", failed.GetProperty("error").GetProperty("code").GetString());
        Assert.False(failed.GetProperty("quota_counted").GetBoolean());
        Assert.Equal(0, (await Json(client, "/api/quota")).GetProperty("free_used").GetInt32());

        factory.Provider.Fail = null;
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync($"/api/analyses/{id}/retry", null)).StatusCode);
        await factory.DrainAsync();
        Assert.Equal(1, (await Json(client, "/api/quota")).GetProperty("free_used").GetInt32());

        // Running the matcher again on the same completed analysis must not count it a second time.
        Assert.Equal(HttpStatusCode.Accepted, (await client.PostAsync($"/api/analyses/{id}/match-evidence", null)).StatusCode);
        await factory.DrainAsync();
        var final = await Json(client, "/api/quota");
        Assert.Equal(1, final.GetProperty("free_used").GetInt32());
    }

    [Fact]
    public async Task Unreadable_tender_is_incomplete_and_not_counted()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var response = await CreateAnalysis(client, "Scan", SimplePdf.Create(""));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await factory.DrainAsync();
        Assert.Equal(0, (await Json(client, "/api/quota")).GetProperty("free_used").GetInt32());
    }

    [Fact]
    public async Task Sample_runs_neither_need_nor_use_the_allowance()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();
        factory.WithDb(db =>
        {
            db.Workspaces.Where(item => item.Id == Workspaces.LocalId)
                .ExecuteUpdate(set => set.SetProperty(item => item.CompletedAnalysisCount, 3));
            return 0;
        });

        Assert.Equal(HttpStatusCode.PaymentRequired, (await CreateAnalysis(client, "Live")).StatusCode);
        var id = await CreateAndDrain(factory, client, "Sample pack", sample: true);
        var analysis = await Json(client, $"/api/analyses/{id}");
        Assert.True(analysis.GetProperty("is_sample").GetBoolean());
        Assert.Equal("needs_review", analysis.GetProperty("status").GetString());
        Assert.Equal(3, (await Json(client, "/api/quota")).GetProperty("free_used").GetInt32());
    }

    [Fact]
    public async Task Activation_is_refused_unless_payments_mode_is_demo()
    {
        using var factory = new OffPaymentsFactory();
        var client = factory.CreateClient();
        var response = await client.PostAsync("/api/subscription/demo-activate", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("payments_disabled", (await ReadJson(response)).GetProperty("code").GetString());
        Assert.False((await Json(client, "/api/quota")).GetProperty("subscription_active").GetBoolean());
    }

    [Fact]
    public async Task Missing_model_key_shows_setup_message_and_creates_nothing()
    {
        using var factory = new ApiFactory();
        factory.Provider.Configured = false;
        var client = factory.CreateClient();

        var config = await Json(client, "/api/config");
        Assert.False(config.GetProperty("model_configured").GetBoolean());

        var response = await CreateAnalysis(client, "No key");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJson(response);
        Assert.Equal("model_not_configured", body.GetProperty("code").GetString());
        Assert.Contains("GEMINI_API_KEY", body.GetProperty("user_message").GetString(), StringComparison.Ordinal);
        Assert.Equal(0, (await Json(client, "/api/analyses")).GetProperty("analyses").GetArrayLength());
        Assert.Equal(0, factory.Provider.Calls);
    }

    [Fact]
    public async Task Unconfigured_provider_never_returns_findings()
    {
        var provider = new UnconfiguredModelProvider();
        Assert.False(provider.IsConfigured);
        var error = await Assert.ThrowsAsync<AppException>(() => provider.ExtractRequirementsAsync([], "az", CancellationToken.None));
        Assert.Equal("model_not_configured", error.Code);
    }

    private sealed class OffPaymentsFactory : ApiFactory
    {
        protected override Dictionary<string, string?> Settings()
        {
            var settings = base.Settings();
            settings["App:PaymentsMode"] = "off";
            return settings;
        }
    }
}
