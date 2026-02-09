using ExpenseAnalyzer.Domain;

namespace ExpenseAnalyzer.Infra.Repos;

public interface ITransferRepository : IRepository<Transfer>
{
    Task<IEnumerable<Transfer>> GetByOutgoingAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transfer>> GetByIncomingAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transfer>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalOutgoingByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalIncomingByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
}
