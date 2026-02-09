namespace ExpenseAnalyzer.Domain;

/// <summary>
/// Represents a money transfer between two accounts
/// </summary>
public class Transfer
{
    /// <summary>
    /// Unique identifier for the transfer
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the transfer occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Account from which money is transferred
    /// </summary>
    public Guid OutgoingAccountId { get; set; }
    public virtual Account OutgoingAccount { get; set; } = null!;

    /// <summary>
    /// Account to which money is transferred
    /// </summary>
    public Guid IncomingAccountId { get; set; }
    public virtual Account IncomingAccount { get; set; } = null!;

    /// <summary>
    /// Amount of money transferred
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Optional comment describing the transfer
    /// </summary>
    public string? Comment { get; set; }
}
