using ExpenseAnalyzer.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Infra.Repos;

public class TransferRepository : Repository<Transfer>, ITransferRepository
{
    private readonly ExpenseContext _context;

    public TransferRepository(ExpenseContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Transfer>> GetByOutgoingAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transfer
            .Where(t => t.OutgoingAccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transfer>> GetByIncomingAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transfer
            .Where(t => t.IncomingAccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transfer>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transfer
            .Where(t => t.OutgoingAccountId == accountId || t.IncomingAccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Transfer
            .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalOutgoingByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transfer
            .Where(t => t.OutgoingAccountId == accountId)
            .SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalIncomingByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transfer
            .Where(t => t.IncomingAccountId == accountId)
            .SumAsync(t => t.Amount, cancellationToken);
    }
}
