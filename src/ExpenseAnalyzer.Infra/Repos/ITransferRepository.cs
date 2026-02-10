using ExpenseAnalyzer.Domain;

namespace ExpenseAnalyzer.Infra.Repos;

/// <summary>
/// Repository interface for Transfer entity operations.
/// </summary>
public interface ITransferRepository : IRepository<Transfer>
{
    /// <summary>
    /// Gets all transfers from a specific outgoing account, ordered by timestamp descending.
    /// </summary>
    Task<IEnumerable<Transfer>> GetByOutgoingAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transfers to a specific incoming account, ordered by timestamp descending.
    /// </summary>
    Task<IEnumerable<Transfer>> GetByIncomingAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transfers where the specified account is either the outgoing or incoming account, ordered by timestamp descending.
    /// </summary>
    Task<IEnumerable<Transfer>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transfers within a specific date range, ordered by timestamp descending.
    /// </summary>
    Task<IEnumerable<Transfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transfers ordered by timestamp descending.
    /// </summary>
    Task<IEnumerable<Transfer>> GetAllOrderedByDateAsync(CancellationToken cancellationToken = default);
}
