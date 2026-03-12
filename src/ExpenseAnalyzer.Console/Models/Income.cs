namespace ExpenseAnalyzer.Models;

public class Income
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = "";
    public string Account { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string? Comment { get; set; }
}
