using ExpenseAnalyzer.Domain;

namespace ExpenseAnalyzer.Infra.Repos;

/// <summary>
/// Repository interface for Account entity with account-specific operations
/// </summary>
public interface IAccountRepository : IRepository<Account>
{
    /// <summary>
    /// Gets an account by its unique name
    /// </summary>
    Task<Account?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an account exists with the specified name
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all accounts ordered by name
    /// </summary>
    Task<IEnumerable<Account>> GetAllOrderedByNameAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches accounts by name pattern (case-insensitive)
    /// </summary>
    Task<IEnumerable<Account>> SearchByNameAsync(string namePattern, CancellationToken cancellationToken = default);
}
