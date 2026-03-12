namespace ExpenseAnalyzer.Models;

public class Transfer
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Outgoing { get; set; } = "";
    public string Incoming { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string? Comment { get; set; }
}
