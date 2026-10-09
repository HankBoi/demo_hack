using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace TenderEvidenceChecker.Api.Data;

/// <summary>
/// EnsureCreated does not alter an existing database. This brings a database created by an earlier
/// build up to the current model without dropping any data.
/// </summary>
public static class SchemaPatcher
{
    private const string LocalWorkspace = "00000000-0000-0000-0000-000000000001";

    public static void Apply(AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        Exec(connection, """
            CREATE TABLE IF NOT EXISTS "Workspaces" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_Workspaces" PRIMARY KEY,
                "Name" TEXT NOT NULL DEFAULT '',
                "CreatedAt" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00',
                "CompletedAnalysisCount" INTEGER NOT NULL DEFAULT 0,
                "SubscriptionStatus" TEXT NOT NULL DEFAULT 'none',
                "SubscriptionActivatedAt" TEXT NULL,
                "SubscriptionExpiresAt" TEXT NULL
            );
            """);
        Exec(connection, """
            CREATE TABLE IF NOT EXISTS "LibraryDocuments" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_LibraryDocuments" PRIMARY KEY,
                "WorkspaceId" TEXT NOT NULL,
                "OriginalName" TEXT NOT NULL DEFAULT '',
                "StoredName" TEXT NOT NULL DEFAULT '',
                "Sha256" TEXT NOT NULL DEFAULT '',
                "SizeBytes" INTEGER NOT NULL DEFAULT 0,
                "PageCount" INTEGER NOT NULL DEFAULT 0,
                "CreatedAt" TEXT NOT NULL DEFAULT '0001-01-01 00:00:00'
            );
            """);
        Exec(connection, """CREATE INDEX IF NOT EXISTS "IX_LibraryDocuments_WorkspaceId" ON "LibraryDocuments" ("WorkspaceId");""");

        AddColumn(connection, "Analyses", "WorkspaceId", $"TEXT NOT NULL DEFAULT '{LocalWorkspace}'");
        AddColumn(connection, "Analyses", "Language", "TEXT NOT NULL DEFAULT 'az'");
        AddColumn(connection, "Analyses", "IsSample", "INTEGER NOT NULL DEFAULT 0");
        AddColumn(connection, "Analyses", "QuotaCounted", "INTEGER NOT NULL DEFAULT 0");
        AddColumn(connection, "Analyses", "QuotaCountedAt", "TEXT NULL");
        Exec(connection, """CREATE INDEX IF NOT EXISTS "IX_Analyses_WorkspaceId" ON "Analyses" ("WorkspaceId");""");

        AddColumn(connection, "Requirements", "Kind", "TEXT NOT NULL DEFAULT 'requirement'");
        AddColumn(connection, "Requirements", "Category", "TEXT NOT NULL DEFAULT 'other'");
        AddColumn(connection, "Requirements", "Severity", "TEXT NOT NULL DEFAULT 'uncertain'");
        AddColumn(connection, "Requirements", "Explanation", "TEXT NOT NULL DEFAULT ''");
        AddColumn(connection, "Requirements", "PossibleImpact", "TEXT NOT NULL DEFAULT ''");
        AddColumn(connection, "Requirements", "NextStep", "TEXT NOT NULL DEFAULT ''");
    }

    public static void SeedWorkspace(AppDbContext db)
    {
        if (db.Workspaces.Any(item => item.Id == Workspaces.LocalId))
        {
            return;
        }

        db.Workspaces.Add(new WorkspaceEntity
        {
            Id = Workspaces.LocalId,
            Name = "Local demo workspace",
            CreatedAt = DateTime.UtcNow
        });
        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateException)
        {
            // The API and the worker can start together; the other process seeded first.
            db.ChangeTracker.Clear();
        }
    }

    private static void AddColumn(DbConnection connection, string table, string column, string definition)
    {
        using var probe = connection.CreateCommand();
        probe.CommandText = $"PRAGMA table_info(\"{table}\");";
        using (var reader = probe.ExecuteReader())
        {
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
        }

        try
        {
            Exec(connection, $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition};");
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
        {
            // The other process added it first.
        }
    }

    private static void Exec(DbConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
