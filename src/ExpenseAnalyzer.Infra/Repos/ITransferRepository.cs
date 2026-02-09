using ExpenseAnalyzer.Domain;

namespace ExpenseAnalyzer.Infra.Repos;

/// <summary>
/// Repository interface for Transfer entity with domain-specific query methods
/// </summary>
public interface ITransferRepository : IRepository<Transfer>
{
    /// <summary>
    /// Get all transfers where the specified account is the outgoing account
    /// </summary>
    Task<IEnumerable<Transfer>> GetByOutgoingAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all transfers where the specified account is the incoming account
    /// </summary>
    Task<IEnumerable<Transfer>> GetByIncomingAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all transfers involving the specified account (either outgoing or incoming)
    /// </summary>
    Task<IEnumerable<Transfer>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all transfers within the specified date range
    /// </summary>
    Task<IEnumerable<Transfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all transfers involving the specified account within the date range
    /// </summary>
    Task<IEnumerable<Transfer>> GetByAccountAndDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
