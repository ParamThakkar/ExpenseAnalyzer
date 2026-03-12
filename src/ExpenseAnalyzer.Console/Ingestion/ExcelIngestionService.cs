using ClosedXML.Excel;
using ExpenseAnalyzer.Data;
using ExpenseAnalyzer.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Ingestion;

public class ExcelIngestionService(AppDbContext db)
{
    public async Task IngestAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Excel file not found: {filePath}");

        Console.WriteLine($"Loading data from {Path.GetFileName(filePath)}...");

        using var workbook = new XLWorkbook(filePath);

        var expenses = ParseExpenses(workbook);
        var incomes = ParseIncomes(workbook);
        var transfers = ParseTransfers(workbook);

        await db.Expenses.AddRangeAsync(expenses);
        await db.Incomes.AddRangeAsync(incomes);
        await db.Transfers.AddRangeAsync(transfers);
        await db.SaveChangesAsync();

        Console.WriteLine($"Imported: {expenses.Count} expenses, {incomes.Count} incomes, {transfers.Count} transfers.");
    }

    public async Task<bool> IsEmptyAsync() =>
        !await db.Expenses.AnyAsync() && !await db.Incomes.AnyAsync();

    private static List<Expense> ParseExpenses(XLWorkbook wb)
    {
        var results = new List<Expense>();
        if (!wb.TryGetWorksheet("Expenses", out var ws)) return results;

        // Row 1 = description, Row 2 = headers, Row 3+ = data
        foreach (var row in ws.RowsUsed().Skip(2))
        {
            var dateCell = row.Cell(1);
            if (!TryParseDate(dateCell, out var date)) continue;

            results.Add(new Expense
            {
                Date = date,
                Category = row.Cell(2).GetString().Trim(),
                Account = row.Cell(3).GetString().Trim(),
                Amount = ParseDecimal(row.Cell(4)),
                Currency = row.Cell(5).GetString().Trim().DefaultIfEmpty("INR"),
                Tags = row.Cell(10).GetString().Trim().NullIfEmpty(),
                Comment = row.Cell(11).GetString().Trim().NullIfEmpty()
            });
        }

        return results;
    }

    private static List<Income> ParseIncomes(XLWorkbook wb)
    {
        var results = new List<Income>();
        if (!wb.TryGetWorksheet("Income", out var ws)) return results;

        foreach (var row in ws.RowsUsed().Skip(2))
        {
            var dateCell = row.Cell(1);
            if (!TryParseDate(dateCell, out var date)) continue;

            results.Add(new Income
            {
                Date = date,
                Category = row.Cell(2).GetString().Trim(),
                Account = row.Cell(3).GetString().Trim(),
                Amount = ParseDecimal(row.Cell(4)),
                Currency = row.Cell(5).GetString().Trim().DefaultIfEmpty("INR"),
                Comment = row.Cell(11).GetString().Trim().NullIfEmpty()
            });
        }

        return results;
    }

    private static List<Transfer> ParseTransfers(XLWorkbook wb)
    {
        var results = new List<Transfer>();
        if (!wb.TryGetWorksheet("Transfers", out var ws)) return results;

        foreach (var row in ws.RowsUsed().Skip(2))
        {
            var dateCell = row.Cell(1);
            if (!TryParseDate(dateCell, out var date)) continue;

            results.Add(new Transfer
            {
                Date = date,
                Outgoing = row.Cell(2).GetString().Trim(),
                Incoming = row.Cell(3).GetString().Trim(),
                Amount = ParseDecimal(row.Cell(4)),
                Currency = row.Cell(5).GetString().Trim().DefaultIfEmpty("INR"),
                Comment = row.Cell(8).GetString().Trim().NullIfEmpty()
            });
        }

        return results;
    }

    private static bool TryParseDate(IXLCell cell, out DateTime date)
    {
        date = default;
        try
        {
            if (cell.DataType == XLDataType.DateTime)
            {
                date = cell.GetDateTime();
                return true;
            }

            var raw = cell.GetString().Trim();
            if (double.TryParse(raw, out var serial) && serial > 1000)
            {
                date = DateTime.FromOADate(serial);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static decimal ParseDecimal(IXLCell cell)
    {
        try { return (decimal)cell.GetDouble(); }
        catch { return 0m; }
    }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;

    public static string DefaultIfEmpty(this string s, string fallback) =>
        string.IsNullOrWhiteSpace(s) ? fallback : s;
}
