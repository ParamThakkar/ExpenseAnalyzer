namespace ExpenseAnalyzer.Domain;

/// <summary>
/// Represents a transfer of funds between two accounts.
/// </summary>
public class Transfer
{
    /// <summary>
    /// Gets or sets the unique identifier for this transfer.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the transfer occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the ID of the account from which funds are transferred.
    /// </summary>
    public Guid OutgoingAccountId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the outgoing account.
    /// </summary>
    public virtual Account OutgoingAccount { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ID of the account to which funds are transferred.
    /// </summary>
    public Guid IncomingAccountId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the incoming account.
    /// </summary>
    public virtual Account IncomingAccount { get; set; } = null!;

    /// <summary>
    /// Gets or sets the amount of the transfer.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets an optional comment describing the transfer.
    /// </summary>
    public string? Comment { get; set; }
}
