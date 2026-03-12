# Personal Expense Analyzer

A .NET console app that lets you ask questions about your personal expenses in plain English. It reads your expense data from an Excel file, stores it in SQLite, and uses OpenAI GPT-4o to understand your questions, generate SQL queries, and return natural language answers.

> This app was entirely vibe coded with [Claude Code](https://claude.ai/claude-code).

## How It Works

```
"Where did I spend most money in 2025?"
        |
        v
   OpenAI GPT-4o
   (generates SQL)
        |
        v
   SQL Validation
   (read-only check + SQLite EXPLAIN)
        |
        v
   Execute against SQLite
        |
        v
   GPT-4o formats answer
        |
        v
"You spent the most on Transportation - Rs 48,955"
```

## Features

- Natural language Q&A over your expense data
- LLM-generated SQL queries (not hardcoded query templates)
- 2-layer SQL validation: static blocklist + SQLite `EXPLAIN` parse check
- Auto-retry: if the LLM generates invalid SQL, it sees the error and retries (up to 5 times)
- Conversation history so you can ask follow-up questions
- Imports Expenses, Income, and Transfers from a single Excel file

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 9 / C# |
| Excel Parsing | ClosedXML |
| Database | SQLite + EF Core |
| NLU | OpenAI GPT-4o (function calling) |
| OpenAI SDK | [OpenAI .NET SDK](https://www.nuget.org/packages/OpenAI) |

## Project Structure

```
src/ExpenseAnalyzer.Console/
  Program.cs                         # Entry point, DI setup, chat loop
  appsettings.json                   # OpenAI API key and config
  Models/
    Expense.cs                       # Expense entity
    Income.cs                        # Income entity
    Transfer.cs                      # Transfer entity
  Data/
    AppDbContext.cs                   # EF Core SQLite context
  Ingestion/
    ExcelIngestionService.cs         # Excel parser (handles serial dates, 3 sheets)
  Nlu/
    ChatService.cs                   # OpenAI integration with tool calling + retry loop
  Query/
    SqlExecutor.cs                   # SQL validation and execution
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- An [OpenAI API key](https://platform.openai.com/api-keys)

## Setup

1. Clone the repo:
   ```bash
   git clone <repo-url>
   cd ExpenseAnalyzer
   ```

2. Set your OpenAI API key (pick one):

   **Option A** - Edit `src/ExpenseAnalyzer.Console/appsettings.json`:
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-proj-...",
       "Model": "gpt-4o"
     }
   }
   ```

   **Option B** - Environment variable:
   ```bash
   export OPENAI_API_KEY=sk-proj-...
   ```

3. Place your expense Excel file in the repo root (or update `ExcelFilePath` in appsettings).

4. Run:
   ```bash
   dotnet run --project src/ExpenseAnalyzer.Console
   ```

On first run, the app imports your Excel data into `expenses.db` (SQLite). To re-import after changing the Excel file, delete `expenses.db` and run again.

## Excel File Format

The app expects an `.xlsx` file with three sheets:

**Expenses** / **Income** sheets:
| Date and time | Category | Account | Amount in default currency | Default currency | ... | Tags | Comment |
|---|---|---|---|---|---|---|---|

**Transfers** sheet:
| Date and time | Outgoing | Incoming | Amount in outgoing currency | Outgoing currency | ... | Comment |
|---|---|---|---|---|---|---|

## Example Questions

- "How much did I spend in total in 2025?"
- "What are my top 5 expense categories?"
- "Show me monthly spending breakdown"
- "How much did I spend on needs vs wants?"
- "What was my biggest single expense?"
- "How much income did I earn from interest?"
- "Compare my spending in Q1 vs Q2"

## SQL Safety

All LLM-generated SQL goes through two validation layers before execution:

1. **Static analysis** - Rejects any query containing `INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `PRAGMA`, etc. Only `SELECT` and `WITH` (CTEs) are allowed.
2. **SQLite EXPLAIN** - The query is parsed by SQLite's own query planner without execution, catching syntax errors, invalid column/table names, and malformed queries.

Results are capped at 100 rows to prevent token overflow.
