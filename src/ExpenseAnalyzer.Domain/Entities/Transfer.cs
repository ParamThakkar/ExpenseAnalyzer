namespace ExpenseAnalyzer.Domain;

public class Transfer
{
    public Guid Id { get; set; }

    public Guid OutgoingAccountId { get; set; }
    public virtual Account OutgoingAccount { get; set; } = null!;

    public Guid IncomingAccountId { get; set; }
    public virtual Account IncomingAccount { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Comment { get; set; }
}
