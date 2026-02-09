using ExpenseAnalyzer.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Infra.Repos;

public class IncomeRepository : Repository<Income>, IIncomeRepository
{
    private readonly ExpenseContext _context;

    public IncomeRepository(ExpenseContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Income>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.AccountId == accountId)
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Income>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.CategoryId == categoryId)
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Income>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.Timestamp >= startDate && i.Timestamp <= endDate)
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.AccountId == accountId)
            .SumAsync(i => i.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.CategoryId == categoryId)
            .SumAsync(i => i.Amount, cancellationToken);
    }

    public async Task<IEnumerable<Income>> GetByAccountAndDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.AccountId == accountId && i.Timestamp >= startDate && i.Timestamp <= endDate)
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Income>> GetAllOrderedByDateAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
