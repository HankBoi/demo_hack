using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TenderEvidenceChecker.Api.Api;
using TenderEvidenceChecker.Api.Data;
using TenderEvidenceChecker.Api.Options;
using TenderEvidenceChecker.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var workerMode = args.Contains("--worker");
MapEnvironment(builder.Configuration);

var repoRoot = FindRepoRoot(builder.Environment.ContentRootPath);
if (string.IsNullOrWhiteSpace(builder.Configuration["App:DatabaseUrl"]))
{
    builder.Configuration["App:DatabaseUrl"] = $"Data Source={Path.Combine(repoRoot, "data", "tender.db")}";
}

if (string.IsNullOrWhiteSpace(builder.Configuration["App:PrivateUploadDir"]))
{
    builder.Configuration["App:PrivateUploadDir"] = Path.Combine(repoRoot, "data", "uploads");
}

var appOptions = new AppOptions();
builder.Configuration.GetSection("App").Bind(appOptions);
Directory.CreateDirectory(Path.GetDirectoryName(SqlitePath(appOptions.DatabaseUrl))!);
Directory.CreateDirectory(appOptions.PrivateUploadDir);

builder.Services.AddSingleton<IOptions<AppOptions>>(Options.Create(appOptions));
builder.Services.AddSingleton<SqlitePragmaInterceptor>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(appOptions.DatabaseUrl, sqlite => sqlite.MaxBatchSize(1));
    options.AddInterceptors(new SqlitePragmaInterceptor());
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = appOptions.MaxUploadBytes * (appOptions.MaxEvidenceFiles + 2);
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = appOptions.MaxUploadBytes * (appOptions.MaxEvidenceFiles + 2);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://127.0.0.1:43123", "http://localhost:43123")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddSingleton<PdfTextService>();
builder.Services.AddSingleton<FileStore>();
builder.Services.AddScoped<DocumentUpload>();
builder.Services.AddSingleton<DemoModelProvider>();
builder.Services.AddHttpClient<AnthropicModelProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(Math.Max(5, appOptions.ModelTimeoutSeconds));
});
builder.Services.AddSingleton<IModelProvider>(services =>
    string.IsNullOrWhiteSpace(appOptions.AnthropicApiKey)
        ? services.GetRequiredService<DemoModelProvider>()
        : services.GetRequiredService<AnthropicModelProvider>());
builder.Services.AddSingleton<AnalysisProcessor>();
if (workerMode)
{
    builder.Services.AddHostedService<JobWorker>();
    // launchSettings.json gives every `dotnet run` the API port. The worker only polls jobs.
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["urls"] = "http://127.0.0.1:0"
    });
}

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
    }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
    {
        // The API and worker can start together and both create the empty database.
    }

    var connection = db.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
    {
        connection.Open();
    }

    using var wal = connection.CreateCommand();
    wal.CommandText = "PRAGMA journal_mode=WAL;";
    wal.ExecuteScalar();
}

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var known = error as AppException;
        context.Response.StatusCode = known?.StatusCode ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            code = known?.Code ?? "pdf_unreadable",
            user_message = known?.UserMessage ?? "Something went wrong before a checklist was saved. Refresh and try again.",
            retryable = known?.Retryable ?? false,
            request_id = Guid.NewGuid().ToString("n")
        });
    });
});
app.UseCors();
app.MapAppEndpoints();
app.Run();

static void MapEnvironment(ConfigurationManager configuration)
{
    void Set(string variable, string key)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        if (!string.IsNullOrWhiteSpace(value))
        {
            configuration[key] = value;
        }
    }

    Set("ANTHROPIC_API_KEY", "App:AnthropicApiKey");
    Set("CLAUDE_MODEL_ID", "App:ClaudeModelId");
    Set("DATABASE_URL", "App:DatabaseUrl");
    Set("PRIVATE_UPLOAD_DIR", "App:PrivateUploadDir");
    Set("MAX_UPLOAD_MB", "App:MaxUploadMb");
    Set("MAX_TENDER_PAGES", "App:MaxTenderPages");
    Set("OCR_ENABLED", "App:OcrEnabled");
    Set("RETENTION_HOURS", "App:RetentionHours");
    Set("REFERENCE_DATE", "App:ReferenceDate");
}

static string FindRepoRoot(string contentRoot)
{
    var dir = new DirectoryInfo(contentRoot);
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "CLAUDE.md")))
        {
            return dir.FullName;
        }

        dir = dir.Parent;
    }

    return contentRoot;
}

static string SqlitePath(string connectionString)
{
    const string marker = "Data Source=";
    var start = connectionString.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
    if (start < 0)
    {
        return Path.Combine("data", "tender.db");
    }

    var value = connectionString[(start + marker.Length)..].Trim().Trim('"');
    var end = value.IndexOf(';');
    return end >= 0 ? value[..end] : value;
}

public partial class Program;
