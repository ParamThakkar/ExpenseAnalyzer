using ExpenseAnalyzer.Domain;

namespace ExpenseAnalyzer.Infra.Repos;

public interface ITransferRepository : IRepository<Transfer>
{
    Task<IEnumerable<Transfer>> GetByOutgoingAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transfer>> GetByIncomingAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transfer>> GetAllOrderedByTimestampAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalTransferredByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
}
