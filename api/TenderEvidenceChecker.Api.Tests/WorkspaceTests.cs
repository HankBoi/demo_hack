using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TenderEvidenceChecker.Api.Data;
using TenderEvidenceChecker.Api.Services;
using TenderEvidenceChecker.Api.Tests.Support;
using static TenderEvidenceChecker.Api.Tests.Support.TestHelpers;

namespace TenderEvidenceChecker.Api.Tests;

public sealed class RoomyQuotaFactory : ApiFactory
{
    protected override Dictionary<string, string?> Settings()
    {
        var settings = base.Settings();
        settings["App:FreeAnalysisLimit"] = "100";
        return settings;
    }
}

public class WorkspaceTests : IClassFixture<RoomyQuotaFactory>
{
    private readonly ApiFactory _factory;

    public WorkspaceTests(RoomyQuotaFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Library_validates_stores_views_and_deletes_company_documents()
    {
        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        content.Add(Pdf(SimplePdf.Create("Company registration certificate issued for the supplier.")), "files", "sertifikat.pdf");
        content.Add(new ByteArrayContent("hello"u8.ToArray()), "files", "notes.txt");
        content.Add(Pdf("%PDF-1.4\nnot-a-real-pdf"u8.ToArray()), "files", "broken.pdf");
        content.Add(Pdf(SimplePdf.CreateEncryptedPlaceholder()), "files", "locked.pdf");
        var big = new byte[(1024 * 1024) + 4096];
        "%PDF-1.4\n"u8.CopyTo(big);
        content.Add(Pdf(big), "files", "big.pdf");
        content.Add(Pdf(Array.Empty<byte>()), "files", "empty.pdf");
        var response = await client.PostAsync("/api/documents", content);
        var body = await ReadJson(response);

        Assert.Single(body.GetProperty("saved").EnumerateArray());
        var codes = body.GetProperty("rejected").EnumerateArray()
            .ToDictionary(item => item.GetProperty("original_name").GetString()!, item => item.GetProperty("code").GetString());
        Assert.Equal("unsupported_file_type", codes["notes.txt"]);
        Assert.Equal("pdf_unreadable", codes["broken.pdf"]);
        Assert.True(codes["locked.pdf"] is "pdf_encrypted" or "pdf_unreadable");
        Assert.Equal("file_too_large", codes["big.pdf"]);
        Assert.Equal("pdf_unreadable", codes["empty.pdf"]);

        var documentId = body.GetProperty("saved")[0].GetProperty("document_id").GetGuid();
        var stored = await client.GetAsync($"/api/documents/{documentId}/content");
        Assert.Equal(HttpStatusCode.OK, stored.StatusCode);
        Assert.Equal("application/pdf", stored.Content.Headers.ContentType?.MediaType);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString((await stored.Content.ReadAsByteArrayAsync())[..4]));
        Assert.Equal("nosniff", stored.Headers.GetValues("X-Content-Type-Options").Single());

        var duplicate = await ReadJson(await PostDocument(client, "again.pdf", SimplePdf.Create("Company registration certificate issued for the supplier.")));
        Assert.Equal("duplicate_file", duplicate.GetProperty("rejected")[0].GetProperty("code").GetString());

        var file = _factory.WithDb(db => db.LibraryDocuments.AsNoTracking().Single(item => item.Id == documentId));
        var path = Path.Combine(_factory.Root, "uploads", "library", file.StoredName);
        Assert.True(File.Exists(path));
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/api/documents/{documentId}")).StatusCode);
        Assert.False(File.Exists(path));
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/documents/{documentId}/content")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/documents/{documentId}")).StatusCode);
    }

    [Fact]
    public async Task Library_documents_are_copied_into_an_analysis_and_matched_in_one_run()
    {
        var client = _factory.CreateClient();
        var documentId = await UploadDocument(client, "tecrube.pdf", SimplePdf.Create("Registration certificate of the supplier was issued for the tender."));
        var created = await CreateAnalysis(client, "With documents", documentIds: [documentId], language: "ru");
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await ReadJson(created)).GetProperty("analysis_id").GetGuid();
        await _factory.DrainAsync();

        var analysis = await Json(client, $"/api/analyses/{id}");
        Assert.Equal("needs_review", analysis.GetProperty("status").GetString());
        Assert.Equal("ru", analysis.GetProperty("language").GetString());
        Assert.Contains(analysis.GetProperty("files").EnumerateArray(), file => file.GetProperty("role").GetString() == "evidence");

        var requirements = (await Json(client, $"/api/analyses/{id}/requirements")).GetProperty("requirements").EnumerateArray().ToList();
        Assert.NotEmpty(requirements);
        Assert.All(requirements, row => Assert.Single(row.GetProperty("evidence").EnumerateArray()));

        // Deleting the library copy leaves the analysis intact and its stored file still viewable.
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/api/documents/{documentId}")).StatusCode);
        var evidenceFile = analysis.GetProperty("files").EnumerateArray().First(file => file.GetProperty("role").GetString() == "evidence");
        var view = await client.GetAsync($"/api/files/{evidenceFile.GetProperty("file_id").GetGuid()}/content");
        Assert.Equal(HttpStatusCode.OK, view.StatusCode);

        var missing = await CreateAnalysis(client, "Missing document", documentIds: [Guid.NewGuid()]);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("document_not_found", (await ReadJson(missing)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Other_workspaces_are_invisible()
    {
        var client = _factory.CreateClient();
        var foreign = Guid.NewGuid();
        var foreignDocument = Guid.NewGuid();
        _factory.WithDb(db =>
        {
            db.Analyses.Add(new AnalysisEntity
            {
                Id = foreign,
                WorkspaceId = Guid.NewGuid(),
                Label = "Someone else",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            db.LibraryDocuments.Add(new LibraryDocumentEntity
            {
                Id = foreignDocument,
                WorkspaceId = Guid.NewGuid(),
                OriginalName = "secret.pdf",
                StoredName = "x.pdf",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
            return 0;
        });

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/analyses/{foreign}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/analyses/{foreign}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/analyses/{foreign}/export.csv")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/documents/{foreignDocument}/content")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/documents/{foreignDocument}")).StatusCode);
        var listed = await Json(client, "/api/analyses");
        Assert.DoesNotContain(listed.GetProperty("analyses").EnumerateArray(), item => item.GetProperty("analysis_id").GetGuid() == foreign);
        var documents = await Json(client, "/api/documents");
        Assert.DoesNotContain(documents.GetProperty("documents").EnumerateArray(), item => item.GetProperty("document_id").GetGuid() == foreignDocument);
        var withForeign = await CreateAnalysis(client, "Borrow", documentIds: [foreignDocument]);
        Assert.Equal(HttpStatusCode.NotFound, withForeign.StatusCode);
    }

    [Fact]
    public async Task Review_decisions_comments_and_localized_csv_are_saved()
    {
        var client = _factory.CreateClient();
        var id = await CreateAndDrain(_factory, client, "Review me");
        var rows = (await Json(client, $"/api/analyses/{id}/requirements")).GetProperty("requirements").EnumerateArray().ToList();
        var row = rows.First();
        var requirementId = row.GetProperty("requirement_id").GetGuid();

        var comment = await client.PatchAsJsonAsync($"/api/requirements/{requirementId}", new { decision = "comment", note = "Müştəriyə zəng et" });
        Assert.Equal(HttpStatusCode.OK, comment.StatusCode);
        var afterComment = await ReadJson(comment);
        Assert.Equal("Müştəriyə zəng et", afterComment.GetProperty("human_note").GetString());
        Assert.False(afterComment.TryGetProperty("reviewer_decision", out var earlier) && earlier.ValueKind != JsonValueKind.Null);

        var noNote = await client.PatchAsJsonAsync($"/api/requirements/{requirementId}", new { decision = "comment" });
        Assert.Equal(HttpStatusCode.BadRequest, noNote.StatusCode);

        var rejected = await ReadJson(await client.PatchAsJsonAsync($"/api/requirements/{requirementId}", new { decision = "reject" }));
        Assert.Equal("rejected", rejected.GetProperty("review_status").GetString());
        Assert.Equal("reject", rejected.GetProperty("reviewer_decision").GetString());
        Assert.Equal("Müştəriyə zəng et", rejected.GetProperty("human_note").GetString());

        var reloaded = (await Json(client, $"/api/analyses/{id}/requirements")).GetProperty("requirements").EnumerateArray()
            .Single(item => item.GetProperty("requirement_id").GetGuid() == requirementId);
        Assert.Equal("rejected", reloaded.GetProperty("review_status").GetString());
        Assert.Equal(row.GetProperty("statement").GetString(), reloaded.GetProperty("ai_suggestion").GetProperty("statement").GetString());

        var csv = await (await client.GetAsync($"/api/analyses/{id}/export.csv?lang=ru")).Content.ReadAsStringAsync();
        Assert.Contains("Решение человека", csv, StringComparison.Ordinal);
        Assert.Contains("отклонено", csv, StringComparison.Ordinal);
        Assert.Contains("Müştəriyə zəng et", csv, StringComparison.Ordinal);
        var csvAz = await (await client.GetAsync($"/api/analyses/{id}/export.csv?lang=az")).Content.ReadAsStringAsync();
        Assert.Contains("İnsan qərarı", csvAz, StringComparison.Ordinal);
    }

    [Fact]
    public async Task History_lists_analyses_with_status_and_survives_delete()
    {
        var client = _factory.CreateClient();
        var id = await CreateAndDrain(_factory, client, "History item");
        var listed = (await Json(client, "/api/analyses")).GetProperty("analyses").EnumerateArray().ToList();
        var item = listed.Single(entry => entry.GetProperty("analysis_id").GetGuid() == id);
        Assert.Equal("needs_review", item.GetProperty("status").GetString());
        Assert.Equal("History item", item.GetProperty("label").GetString());
        Assert.True(item.GetProperty("findings").GetInt32() > 0);

        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/api/analyses/{id}")).StatusCode);
        var after = (await Json(client, "/api/analyses")).GetProperty("analyses").EnumerateArray();
        Assert.DoesNotContain(after, entry => entry.GetProperty("analysis_id").GetGuid() == id);
        Assert.False(Directory.Exists(Path.Combine(_factory.Root, "uploads", id.ToString("N"))));
    }

    [Fact]
    public async Task Older_database_is_upgraded_without_losing_rows()
    {
        var path = Path.Combine(Path.GetTempPath(), "tec-old-" + Guid.NewGuid().ToString("N") + ".db");
        try
        {
            await using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                await connection.OpenAsync();
                await using var create = connection.CreateCommand();
                create.CommandText = """
                    CREATE TABLE Analyses (Id TEXT PRIMARY KEY, Label TEXT NOT NULL, Status TEXT NOT NULL);
                    CREATE TABLE Requirements (Id TEXT PRIMARY KEY, AnalysisId TEXT NOT NULL, Statement TEXT NOT NULL);
                    INSERT INTO Analyses VALUES ('11111111-1111-1111-1111-111111111111', 'Old analysis', 'needs_review');
                    INSERT INTO Requirements VALUES ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', 'Old statement');
                    """;
                await create.ExecuteNonQueryAsync();
            }

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={path}").Options;
            await using (var db = new AppDbContext(options))
            {
                SchemaPatcher.Apply(db);
                SchemaPatcher.Apply(db);
                SchemaPatcher.SeedWorkspace(db);
            }

            await using var check = new SqliteConnection($"Data Source={path}");
            await check.OpenAsync();
            await using var query = check.CreateCommand();
            query.CommandText = "SELECT WorkspaceId, Language, QuotaCounted FROM Analyses";
            await using var reader = await query.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(Workspaces.LocalId.ToString(), reader.GetString(0), ignoreCase: true);
            Assert.Equal("az", reader.GetString(1));
            Assert.Equal(0, reader.GetInt32(2));
            await reader.CloseAsync();
            query.CommandText = "SELECT Kind, Severity FROM Requirements";
            await using var rows = await query.ExecuteReaderAsync();
            Assert.True(await rows.ReadAsync());
            Assert.Equal("requirement", rows.GetString(0));
            Assert.Equal("uncertain", rows.GetString(1));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            foreach (var file in Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(path) + "*"))
            {
                try { File.Delete(file); } catch (IOException) { }
            }
        }
    }

    private static async Task<HttpResponseMessage> PostDocument(HttpClient client, string name, byte[] bytes)
    {
        using var content = new MultipartFormDataContent();
        content.Add(Pdf(bytes), "files", name);
        return await client.PostAsync("/api/documents", content);
    }
}
