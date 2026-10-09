using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TenderEvidenceChecker.Api.Data;
using TenderEvidenceChecker.Api.Options;

namespace TenderEvidenceChecker.Api.Services;

public sealed record QuotaStatus(
    int FreeLimit,
    int FreeUsed,
    int FreeRemaining,
    bool SubscriptionActive,
    string SubscriptionStatus,
    DateTime? SubscriptionExpiresAt,
    bool CanStart,
    string PaymentsMode,
    decimal DemoPriceAzn);

/// <summary>
/// Server-side free-analysis quota and the simulated monthly subscription for the local demo workspace.
/// State lives in the database. The demo subscription never touches a payment provider or card data.
/// </summary>
public sealed class QuotaService(IOptions<AppOptions> options)
{
    public const string PaymentsDemo = "demo";

    public async Task<QuotaStatus> GetAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var workspace = await db.Workspaces.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == Workspaces.LocalId, cancellationToken)
            ?? new WorkspaceEntity { Id = Workspaces.LocalId };
        return Describe(workspace, DateTime.UtcNow);
    }

    public QuotaStatus Describe(WorkspaceEntity workspace, DateTime nowUtc)
    {
        var limit = Math.Max(0, options.Value.FreeAnalysisLimit);
        var used = Math.Min(workspace.CompletedAnalysisCount, int.MaxValue);
        var active = workspace.SubscriptionExpiresAt is DateTime expires && expires > nowUtc;
        var status = active
            ? "active"
            : workspace.SubscriptionExpiresAt is null ? "none" : "expired";
        return new QuotaStatus(
            limit,
            used,
            Math.Max(0, limit - used),
            active,
            status,
            workspace.SubscriptionExpiresAt,
            used < limit || active,
            options.Value.PaymentsMode,
            options.Value.DemoMonthlyPriceAzn);
    }

    public async Task<QuotaStatus> ActivateDemoAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (!string.Equals(options.Value.PaymentsMode, PaymentsDemo, StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException(
                "payments_disabled",
                "The demo subscription is available only when PAYMENTS_MODE=demo. No real payment provider is connected.",
                retryable: false,
                statusCode: 403);
        }

        var workspace = await db.Workspaces.FirstAsync(item => item.Id == Workspaces.LocalId, cancellationToken);
        var now = DateTime.UtcNow;
        workspace.SubscriptionStatus = "active";
        workspace.SubscriptionActivatedAt = now;
        workspace.SubscriptionExpiresAt = now.AddDays(Math.Max(1, options.Value.DemoSubscriptionDays));
        await db.SaveChangesAsync(cancellationToken);
        return Describe(workspace, now);
    }

    /// <summary>
    /// Counts a successfully completed live analysis exactly once. Sample runs, failed runs,
    /// incomplete runs, and repeated calls for the same analysis do not consume the allowance.
    /// </summary>
    public async Task<bool> TryCountAsync(AppDbContext db, AnalysisEntity analysis, CancellationToken cancellationToken)
    {
        if (analysis.IsSample || analysis.QuotaCounted)
        {
            return false;
        }

        if (analysis.Status is not ("needs_review" or "completed" or "completed_with_unresolved_items"))
        {
            return false;
        }

        if (!await db.Requirements.AnyAsync(item => item.AnalysisId == analysis.Id, cancellationToken))
        {
            return false;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var claimed = await db.Analyses
            .Where(item => item.Id == analysis.Id && !item.QuotaCounted)
            .ExecuteUpdateAsync(set => set
                .SetProperty(item => item.QuotaCounted, true)
                .SetProperty(item => item.QuotaCountedAt, DateTime.UtcNow), cancellationToken);
        if (claimed == 0)
        {
            return false;
        }

        await db.Workspaces
            .Where(item => item.Id == analysis.WorkspaceId)
            .ExecuteUpdateAsync(set => set.SetProperty(item => item.CompletedAnalysisCount, item => item.CompletedAnalysisCount + 1), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        analysis.QuotaCounted = true;
        analysis.QuotaCountedAt = DateTime.UtcNow;
        return true;
    }
}
