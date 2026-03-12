using System.Text.Json;
using ExpenseAnalyzer.Query;
using OpenAI.Chat;

namespace ExpenseAnalyzer.Nlu;

public class ChatService(SqlExecutor sqlExecutor, string apiKey, string model = "gpt-4o")
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
  };

  private readonly ChatClient _client = new(model, apiKey);
  private readonly List<ChatMessage> _history = [];
  private bool _initialized;

  private static readonly ChatTool ExecuteSqlTool = ChatTool.CreateFunctionTool(
      functionName: "execute_sql",
      functionDescription: "Execute a read-only SQL query against the SQLite expense database. Only SELECT queries are allowed.",
      functionParameters: BinaryData.FromString("""
            {
              "type": "object",
              "properties": {
                "sql": {
                  "type": "string",
                  "description": "The SQL SELECT query to execute. Use SQLite syntax. Date column stores dates as 'YYYY-MM-DD HH:MM:SS' text format."
                },
                "explanation": {
                  "type": "string",
                  "description": "Brief one-line explanation of what this query does"
                }
              },
              "required": ["sql"]
            }
            """)
  );

  public async Task<string> AskAsync(string question)
  {
    if (!_initialized)
      await InitializeAsync();

    _history.Add(new UserChatMessage(question));

    var options = new ChatCompletionOptions { Tools = { ExecuteSqlTool } };

    const int maxToolIterations = 5;
    var iteration = 0;

    // Agentic loop: GPT may call the tool multiple times (retries on error, or multi-step queries)
    while (true)
    {
      var completion = await _client.CompleteChatAsync(_history, options);

      if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
      {
        iteration++;
        _history.Add(new AssistantChatMessage(completion.Value));

        if (iteration >= maxToolIterations)
        {
          // Force GPT to answer with whatever data it has so far
          foreach (var toolCall in completion.Value.ToolCalls)
          {
            var errorJson = JsonSerializer.Serialize(new
            {
              success = false,
              error = "Maximum retry limit (5) reached. Please provide the best answer you can with the data collected so far."
            });
            _history.Add(new ToolChatMessage(toolCall.Id, errorJson));
          }
          continue;
        }

        foreach (var toolCall in completion.Value.ToolCalls)
        {
          var resultJson = await HandleToolCallAsync(toolCall);
          _history.Add(new ToolChatMessage(toolCall.Id, resultJson));
        }

        continue;
      }

      var answer = completion.Value.Content[0].Text;
      _history.Add(new AssistantChatMessage(answer));

      TrimHistory();
      return answer;
    }
  }

  private async Task InitializeAsync()
  {
    var schema = await sqlExecutor.GetSchemaAsync();

    var systemPrompt = $"""
            You are a personal finance assistant. You answer questions by querying a SQLite database using the execute_sql tool.

            DATABASE SCHEMA:
            {schema}

            KEY DETAILS:
            - Dates are stored as text in 'YYYY-MM-DD HH:MM:SS' format. Use date() or strftime() for date operations.
            - Amounts are in INR (Indian Rupees).
            - Expenses table has a Tags column with values "Need" or "Want" (or NULL).
            - Use LIKE for case-insensitive partial matching on text columns.
            - Results are capped at 100 rows. Use LIMIT and aggregation to keep results concise.

            RULES:
            - Only generate SELECT queries. No INSERT, UPDATE, DELETE, DROP, etc.
            - Always use the exact column names from the schema above.
            - For date filtering, use: Date >= '2025-01-01' AND Date < '2025-02-01'
            - You may call execute_sql multiple times if a question needs several queries.
            - If a query returns an error, read the error message, fix the SQL, and retry.

            RESPONSE FORMAT:
            - After getting data, give a clear, concise answer in natural language.
            - Format amounts with ₹ symbol (e.g., ₹5,000 or ₹1.5 lakh for ≥1,00,000).
            - For grouped results, present as a readable list or markdown table.
            - Mention the query timeframe if relevant.
            """;

    _history.Add(new SystemChatMessage(systemPrompt));
    _initialized = true;
  }

  private async Task<string> HandleToolCallAsync(ChatToolCall toolCall)
  {
    try
    {
      using var doc = JsonDocument.Parse(toolCall.FunctionArguments.ToString());
      var sql = doc.RootElement.GetProperty("sql").GetString()
          ?? throw new InvalidOperationException("Missing 'sql' parameter.");

      var explanation = doc.RootElement.TryGetProperty("explanation", out var exp)
          ? exp.GetString() : null;

      if (explanation != null)
        Console.WriteLine($"  [SQL] {explanation}");

      Console.WriteLine($"Executing SQL : [{sql}]");

      var result = await sqlExecutor.ExecuteAsync(sql);

      if (!result.Success)
        Console.WriteLine($"  [SQL Error] {result.Error}");

      return JsonSerializer.Serialize(result, JsonOptions);
    }
    catch (Exception ex)
    {
      return JsonSerializer.Serialize(new { success = false, error = ex.Message });
    }
  }

  private void TrimHistory()
  {
    if (_history.Count > 21)
    {
      var system = _history[0];
      var recent = _history.TakeLast(20).ToList();
      _history.Clear();
      _history.Add(system);
      _history.AddRange(recent);
    }
  }
}
