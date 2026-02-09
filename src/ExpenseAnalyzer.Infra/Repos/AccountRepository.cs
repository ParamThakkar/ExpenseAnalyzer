using ExpenseAnalyzer.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Infra.Repos;

/// <summary>
/// Repository implementation for Account entity
/// </summary>
public class AccountRepository : Repository<Account>, IAccountRepository
{
    private readonly ExpenseContext _context;

    public AccountRepository(ExpenseContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Account?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

        return await _context.Account
            .FirstOrDefaultAsync(a => a.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

        return await _context.Account
            .AnyAsync(a => a.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Account>> GetAllOrderedByNameAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Account
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Account>> SearchByNameAsync(string namePattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(namePattern))
            return await GetAllOrderedByNameAsync(cancellationToken);

        return await _context.Account
            .Where(a => EF.Functions.Like(a.Name, $"%{namePattern}%"))
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);
    }
}
