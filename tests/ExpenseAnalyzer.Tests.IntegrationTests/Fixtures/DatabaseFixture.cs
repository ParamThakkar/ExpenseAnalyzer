using Microsoft.EntityFrameworkCore;
using ExpenseAnalyzer.Domain;

namespace ExpenseAnalyzer.Tests.IntegrationTests.Fixtures;

/// <summary>
/// Database fixture for integration tests with in-memory database and seeded test data
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    public ExpenseContext Context { get; private set; } = null!;

    // Test data identifiers
    public Guid TestCategoryId { get; private set; }
    public Guid TestAccountId { get; private set; }
    public Guid SecondCategoryId { get; private set; }
    public Guid SecondAccountId { get; private set; }

    public async Task InitializeAsync()
    {
        // Create unique database for each test run
        var dbName = $"ExpenseTest_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=true;MultipleActiveResultSets=true;";

        var options = new DbContextOptionsBuilder<ExpenseContext>()
            .UseSqlServer(connectionString)
            .UseLazyLoadingProxies()
            .Options;

        Context = new ExpenseContext(options);
        await Context.Database.EnsureCreatedAsync();

        // Seed test data
        await SeedTestDataAsync();
    }

    private async Task SeedTestDataAsync()
    {
        // Create test categories
        var category1 = new Category { Id = Guid.NewGuid(), Name = "TestCategory1" };
        var category2 = new Category { Id = Guid.NewGuid(), Name = "TestCategory2" };
        Context.Category.AddRange(category1, category2);
        TestCategoryId = category1.Id;
        SecondCategoryId = category2.Id;

        // Create test accounts
        var account1 = new Account { Id = Guid.NewGuid(), Name = "TestAccount1" };
        var account2 = new Account { Id = Guid.NewGuid(), Name = "TestAccount2" };
        Context.Account.AddRange(account1, account2);
        TestAccountId = account1.Id;
        SecondAccountId = account2.Id;

        await Context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.DisposeAsync();
        }
    }
}

/// <summary>
/// Collection definition for sharing database fixture across test classes
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
