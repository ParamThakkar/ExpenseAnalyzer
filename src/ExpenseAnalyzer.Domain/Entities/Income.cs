namespace ExpenseAnalyzer.Domain;

public class Income
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public Guid AccountId { get; set; }
    public virtual Account Account { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Comment { get; set; }
}
