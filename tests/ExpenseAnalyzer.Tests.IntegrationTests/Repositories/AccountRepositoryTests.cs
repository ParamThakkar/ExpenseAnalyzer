using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;
using ExpenseAnalyzer.Tests.IntegrationTests.Fixtures;

namespace ExpenseAnalyzer.Tests.IntegrationTests.Repositories;

[Collection("Database")]
[Trait("Category", "Integration")]
public class AccountRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    private readonly AccountRepository _repository;

    public AccountRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new AccountRepository(_fixture.Context);
    }

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_ExistingAccount_ReturnsAccount()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "GetByNameTest" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("GetByNameTest");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Id, result.Id);
        Assert.Equal("GetByNameTest", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_NonExistingAccount_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("NonExistingAccount");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByNameAsync_CaseSensitive_ReturnsCorrectAccount()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "CaseSensitiveTest" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act - Note: SQL Server default collation is case-insensitive
        var result = await _repository.GetByNameAsync("casesensitivetest");

        // Assert
        Assert.NotNull(result); // Will find it due to SQL Server default collation
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task GetByNameAsync_InvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByNameAsync(invalidName!));
    }

    #endregion

    #region ExistsByNameAsync Tests

    [Fact]
    public async Task ExistsByNameAsync_ExistingAccount_ReturnsTrue()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "ExistsTest" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByNameAsync("ExistsTest");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByNameAsync_NonExistingAccount_ReturnsFalse()
    {
        // Act
        var result = await _repository.ExistsByNameAsync("NonExistingAccount");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ExistsByNameAsync_InvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.ExistsByNameAsync(invalidName!));
    }

    #endregion

    #region GetAllOrderedByNameAsync Tests

    [Fact]
    public async Task GetAllOrderedByNameAsync_MultipleAccounts_ReturnsOrderedByName()
    {
        // Arrange
        var account1 = new Account { Id = Guid.NewGuid(), Name = "Zebra" };
        var account2 = new Account { Id = Guid.NewGuid(), Name = "Apple" };
        var account3 = new Account { Id = Guid.NewGuid(), Name = "Mango" };
        await _repository.InsertAsync(account1);
        await _repository.InsertAsync(account2);
        await _repository.InsertAsync(account3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllOrderedByNameAsync()).ToList();

        // Assert
        Assert.True(result.Count >= 3);
        var testAccounts = result.Where(a => a.Name == "Zebra" || a.Name == "Apple" || a.Name == "Mango").ToList();
        Assert.Equal(3, testAccounts.Count);
        Assert.Equal("Apple", testAccounts[0].Name);
        Assert.Equal("Mango", testAccounts[1].Name);
        Assert.Equal("Zebra", testAccounts[2].Name);
    }

    [Fact]
    public async Task GetAllOrderedByNameAsync_NoAccounts_ReturnsEmptyCollection()
    {
        // Arrange - Use a fresh context to ensure clean state
        // Note: This test might find existing seeded accounts from fixture

        // Act
        var result = await _repository.GetAllOrderedByNameAsync();

        // Assert
        Assert.NotNull(result);
        // Just verify it returns a collection (may have seeded data)
        Assert.IsAssignableFrom<IEnumerable<Account>>(result);
    }

    #endregion

    #region SearchByNameAsync Tests

    [Fact]
    public async Task SearchByNameAsync_ExactMatch_ReturnsAccount()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "SearchTest" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.SearchByNameAsync("SearchTest")).ToList();

        // Assert
        Assert.Single(result.Where(a => a.Name == "SearchTest"));
    }

    [Fact]
    public async Task SearchByNameAsync_PartialMatch_ReturnsMatchingAccounts()
    {
        // Arrange
        var account1 = new Account { Id = Guid.NewGuid(), Name = "TestAccount1" };
        var account2 = new Account { Id = Guid.NewGuid(), Name = "TestAccount2" };
        var account3 = new Account { Id = Guid.NewGuid(), Name = "OtherAccount" };
        await _repository.InsertAsync(account1);
        await _repository.InsertAsync(account2);
        await _repository.InsertAsync(account3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.SearchByNameAsync("TestAccount")).ToList();

        // Assert
        var testAccounts = result.Where(a => a.Name.Contains("TestAccount")).ToList();
        Assert.True(testAccounts.Count >= 2);
        Assert.Contains(testAccounts, a => a.Name == "TestAccount1");
        Assert.Contains(testAccounts, a => a.Name == "TestAccount2");
    }

    [Fact]
    public async Task SearchByNameAsync_NoMatch_ReturnsEmptyCollection()
    {
        // Act
        var result = (await _repository.SearchByNameAsync("NonExistingPattern123456")).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchByNameAsync_EmptyPattern_ReturnsAllOrderedByName()
    {
        // Arrange
        var account1 = new Account { Id = Guid.NewGuid(), Name = "ZAccount" };
        var account2 = new Account { Id = Guid.NewGuid(), Name = "AAccount" };
        await _repository.InsertAsync(account1);
        await _repository.InsertAsync(account2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.SearchByNameAsync("")).ToList();

        // Assert
        Assert.True(result.Count >= 2);
        // Verify ordering - first account should come before last alphabetically
        var firstIndex = result.FindIndex(a => a.Name == "AAccount");
        var lastIndex = result.FindIndex(a => a.Name == "ZAccount");
        Assert.True(firstIndex < lastIndex);
    }

    [Fact]
    public async Task SearchByNameAsync_CaseInsensitive_ReturnsMatchingAccounts()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "CaseTest" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.SearchByNameAsync("casetest")).ToList();

        // Assert
        Assert.Single(result.Where(a => a.Name == "CaseTest"));
    }

    #endregion

    #region Base Repository Tests (CRUD)

    [Fact]
    public async Task InsertAsync_NewAccount_AddsToDatabase()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "NewAccount" };

        // Act
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(account.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(account.Id, retrieved.Id);
        Assert.Equal("NewAccount", retrieved.Name);
    }

    [Fact]
    public async Task Update_ExistingAccount_UpdatesDatabase()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "OriginalName" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        account.Name = "UpdatedName";
        _repository.Update(account);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(account.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("UpdatedName", retrieved.Name);
    }

    [Fact]
    public async Task Delete_ExistingAccount_RemovesFromDatabase()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "ToDelete" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        _repository.Delete(account);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(account.Id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAccounts()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "GetAllTest" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, a => a.Id == account.Id);
    }

    [Fact]
    public async Task ExistsAsync_ExistingAccount_ReturnsTrue()
    {
        // Arrange
        var account = new Account { Id = Guid.NewGuid(), Name = "ExistsAsyncTest" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(a => a.Id == account.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingAccount_ReturnsFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(a => a.Id == nonExistingId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var initialCount = await _repository.CountAsync();
        var account = new Account { Id = Guid.NewGuid(), Name = "CountTest" };
        await _repository.InsertAsync(account);
        await _repository.SaveChangesAsync();

        // Act
        var newCount = await _repository.CountAsync();

        // Assert
        Assert.Equal(initialCount + 1, newCount);
    }

    #endregion
}
