using ExpenseAnalyzer.Data;
using ExpenseAnalyzer.Ingestion;
using ExpenseAnalyzer.Nlu;
using ExpenseAnalyzer.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

const string DbFile = "expenses.db";
var connectionString = $"Data Source={DbFile}";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(AppContext.BaseDirectory);
        config.AddJsonFile("appsettings.json", optional: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));
        services.AddScoped<ExcelIngestionService>();
    })
    .Build();

// ── Resolve configuration ────────────────────────────────────────────────────

var config = host.Services.GetRequiredService<IConfiguration>();
var apiKey = GetRequiredApiKey(config);

static string GetRequiredApiKey(IConfiguration config)
{
    var key = config["OpenAI:ApiKey"];
    if (!string.IsNullOrWhiteSpace(key)) return key;

    key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (!string.IsNullOrWhiteSpace(key)) return key;

    throw new InvalidOperationException(
        "OpenAI API key not found. Set it in appsettings.json (OpenAI:ApiKey) " +
        "or via the OPENAI_API_KEY environment variable.");
}

var model = config["OpenAI:Model"] ?? "gpt-4o";
var excelPath = config["ExcelFilePath"] ?? "2025_personal_expenses.xlsx";

if (!Path.IsPathRooted(excelPath))
    excelPath = Path.Combine(Directory.GetCurrentDirectory(), excelPath);

// ── Initialise database ───────────────────────────────────────────────────────

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var ingestion = scope.ServiceProvider.GetRequiredService<ExcelIngestionService>();
    if (await ingestion.IsEmptyAsync())
        await ingestion.IngestAsync(excelPath);
    else
        Console.WriteLine("Database already populated. Using existing data. (Delete expenses.db to re-import.)");
}

// ── Chat loop ─────────────────────────────────────────────────────────────────

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║       Personal Expense Analyzer          ║");
Console.WriteLine("║  Ask anything about your 2025 finances   ║");
Console.WriteLine("║  Type 'exit' or press Ctrl+C to quit     ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

var sqlExecutor = new SqlExecutor(connectionString);
var chat = new ChatService(sqlExecutor, apiKey, model);

while (true)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("You: ");
    Console.ResetColor();

    var input = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(input)) continue;
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("Thinking...");
    Console.ResetColor();

    try
    {
        var answer = await chat.AskAsync(input);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Assistant: ");
        Console.ResetColor();
        Console.WriteLine(answer);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine();
}
