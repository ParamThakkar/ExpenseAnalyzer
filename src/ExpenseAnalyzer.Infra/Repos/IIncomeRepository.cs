using ExpenseAnalyzer.Domain;

namespace ExpenseAnalyzer.Infra.Repos;

public interface IIncomeRepository : IRepository<Income>
{
    Task<IEnumerable<Income>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Income>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Income>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Income>> GetByAccountAndDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Income>> GetAllOrderedByDateAsync(CancellationToken cancellationToken = default);
}
