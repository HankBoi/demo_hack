using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TenderEvidenceChecker.Api.Data;

public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var busy = connection.CreateCommand();
        busy.CommandText = "PRAGMA busy_timeout=5000;";
        busy.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA busy_timeout=5000;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
