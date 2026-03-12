using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace ExpenseAnalyzer.Query;

public class SqlExecutor(string connectionString)
{
    private const int MaxRows = 100;

    private static readonly HashSet<string> ForbiddenKeywords =
    [
        "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE",
        "TRUNCATE", "REPLACE", "ATTACH", "DETACH", "PRAGMA",
        "REINDEX", "VACUUM", "GRANT", "REVOKE"
    ];

    public async Task<SqlResult> ExecuteAsync(string sql)
    {
        // Step 1: Static validation — reject anything that isn't a SELECT
        var validationError = ValidateSql(sql);
        if (validationError != null)
            return SqlResult.Fail(validationError);

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        // Step 2: Use EXPLAIN to let SQLite parse-check the query before running it
        var explainError = await ExplainAsync(connection, sql);
        if (explainError != null)
            return SqlResult.Fail($"Invalid SQL: {explainError}");

        // Step 3: Execute the validated query
        return await RunQueryAsync(connection, sql);
    }

    /// <summary>
    /// Returns the DB schema as a string so the LLM knows the table/column structure.
    /// </summary>
    public async Task<string> GetSchemaAsync()
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";

        var schemas = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            schemas.Add(reader.GetString(0));

        return string.Join("\n\n", schemas);
    }

    private static string? ValidateSql(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return "SQL query is empty.";

        // Trim and normalize
        var normalized = sql.Trim().TrimEnd(';');

        // Must start with SELECT or WITH (for CTEs)
        if (!Regex.IsMatch(normalized, @"^\s*(SELECT|WITH)\s", RegexOptions.IgnoreCase))
            return "Only SELECT queries are allowed.";

        // Check for forbidden keywords as standalone words
        foreach (var keyword in ForbiddenKeywords)
        {
            if (Regex.IsMatch(normalized, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
                return $"Forbidden operation: {keyword} is not allowed. Only SELECT queries are permitted.";
        }

        return null;
    }

    private static async Task<string?> ExplainAsync(SqliteConnection connection, string sql)
    {
        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"EXPLAIN {sql}";
            await using var reader = await cmd.ExecuteReaderAsync();
            // Just reading through EXPLAIN results validates the query
            while (await reader.ReadAsync()) { }
            return null;
        }
        catch (SqliteException ex)
        {
            return ex.Message;
        }
    }

    private static async Task<SqlResult> RunQueryAsync(SqliteConnection connection, string sql)
    {
        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            await using var reader = await cmd.ExecuteReaderAsync();

            var columns = Enumerable.Range(0, reader.FieldCount)
                .Select(i => reader.GetName(i))
                .ToList();

            var rows = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync() && rows.Count < MaxRows)
            {
                var row = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                    row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }

            var hasMore = await reader.ReadAsync();

            return SqlResult.Ok(columns, rows, hasMore);
        }
        catch (SqliteException ex)
        {
            return SqlResult.Fail($"Query execution error: {ex.Message}");
        }
    }
}

public class SqlResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public List<string> Columns { get; init; } = [];
    public List<Dictionary<string, object?>> Rows { get; init; } = [];
    public int RowCount => Rows.Count;
    public bool Truncated { get; init; }

    public static SqlResult Ok(List<string> columns, List<Dictionary<string, object?>> rows, bool truncated) =>
        new() { Success = true, Columns = columns, Rows = rows, Truncated = truncated };

    public static SqlResult Fail(string error) =>
        new() { Success = false, Error = error };
}
