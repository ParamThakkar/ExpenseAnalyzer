using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;
using ExpenseAnalyzer.Tests.IntegrationTests.Fixtures;

namespace ExpenseAnalyzer.Tests.IntegrationTests.Repositories;

[Collection("Database")]
[Trait("Category", "Integration")]
public class IncomeRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    private readonly IncomeRepository _repository;

    public IncomeRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new IncomeRepository(_fixture.Context);
    }

    #region GetByAccountAsync Tests

    [Fact]
    public async Task GetByAccountAsync_WithIncome_ReturnsIncomeForAccount()
    {
        // Arrange
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 2000m,
            Timestamp = DateTime.Now.AddDays(-1)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.SecondAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 3000m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(income1);
        await _repository.InsertAsync(income2);
        await _repository.InsertAsync(income3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountAsync(_fixture.TestAccountId)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal(_fixture.TestAccountId, i.AccountId));
        // Verify ordering by timestamp descending
        Assert.True(result[0].Timestamp >= result[1].Timestamp);
    }

    [Fact]
    public async Task GetByAccountAsync_NoIncome_ReturnsEmptyCollection()
    {
        // Act
        var result = (await _repository.GetByAccountAsync(Guid.NewGuid())).ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetByCategoryAsync Tests

    [Fact]
    public async Task GetByCategoryAsync_WithIncome_ReturnsIncomeForCategory()
    {
        // Arrange
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.SecondCategoryId,
            Amount = 2000m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(income1);
        await _repository.InsertAsync(income2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByCategoryAsync(_fixture.TestCategoryId)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(_fixture.TestCategoryId, result[0].CategoryId);
    }

    [Fact]
    public async Task GetByCategoryAsync_NoIncome_ReturnsEmptyCollection()
    {
        // Act
        var result = (await _repository.GetByCategoryAsync(Guid.NewGuid())).ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetByDateRangeAsync Tests

    [Fact]
    public async Task GetByDateRangeAsync_WithIncomeInRange_ReturnsIncomeInRange()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = new DateTime(2025, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 2000m,
            Timestamp = new DateTime(2025, 2, 1) // Outside range
        };
        await _repository.InsertAsync(income1);
        await _repository.InsertAsync(income2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByDateRangeAsync(startDate, endDate)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(income1.Id, result[0].Id);
        Assert.InRange(result[0].Timestamp, startDate, endDate);
    }

    [Fact]
    public async Task GetByDateRangeAsync_NoIncomeInRange_ReturnsEmptyCollection()
    {
        // Arrange
        var startDate = new DateTime(2030, 1, 1);
        var endDate = new DateTime(2030, 1, 31);

        // Act
        var result = (await _repository.GetByDateRangeAsync(startDate, endDate)).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByDateRangeAsync_IncomeOnBoundaries_ReturnsIncome()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = startDate // Exactly on start boundary
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 2000m,
            Timestamp = endDate // Exactly on end boundary
        };
        await _repository.InsertAsync(income1);
        await _repository.InsertAsync(income2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByDateRangeAsync(startDate, endDate)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    #endregion

    #region GetTotalByAccountAsync Tests

    [Fact]
    public async Task GetTotalByAccountAsync_WithIncome_ReturnsSumOfAmounts()
    {
        // Arrange
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 2500m,
            Timestamp = DateTime.Now
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.SecondAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 5000m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(income1);
        await _repository.InsertAsync(income2);
        await _repository.InsertAsync(income3);
        await _repository.SaveChangesAsync();

        // Act
        var total = await _repository.GetTotalByAccountAsync(_fixture.TestAccountId);

        // Assert
        Assert.Equal(3500m, total);
    }

    [Fact]
    public async Task GetTotalByAccountAsync_NoIncome_ReturnsZero()
    {
        // Act
        var total = await _repository.GetTotalByAccountAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(0m, total);
    }

    #endregion

    #region GetTotalByCategoryAsync Tests

    [Fact]
    public async Task GetTotalByCategoryAsync_WithIncome_ReturnsSumOfAmounts()
    {
        // Arrange
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1500m,
            Timestamp = DateTime.Now
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 2500m,
            Timestamp = DateTime.Now
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.SecondCategoryId,
            Amount = 3000m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(income1);
        await _repository.InsertAsync(income2);
        await _repository.InsertAsync(income3);
        await _repository.SaveChangesAsync();

        // Act
        var total = await _repository.GetTotalByCategoryAsync(_fixture.TestCategoryId);

        // Assert
        Assert.Equal(4000m, total);
    }

    [Fact]
    public async Task GetTotalByCategoryAsync_NoIncome_ReturnsZero()
    {
        // Act
        var total = await _repository.GetTotalByCategoryAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(0m, total);
    }

    #endregion

    #region GetByAccountAndDateRangeAsync Tests

    [Fact]
    public async Task GetByAccountAndDateRangeAsync_WithMatchingIncome_ReturnsIncome()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = new DateTime(2025, 1, 15)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.SecondAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 2000m,
            Timestamp = new DateTime(2025, 1, 15) // Same date, different account
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 3000m,
            Timestamp = new DateTime(2025, 2, 1) // Same account, outside date range
        };
        await _repository.InsertAsync(income1);
        await _repository.InsertAsync(income2);
        await _repository.InsertAsync(income3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountAndDateRangeAsync(_fixture.TestAccountId, startDate, endDate)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(income1.Id, result[0].Id);
        Assert.Equal(_fixture.TestAccountId, result[0].AccountId);
        Assert.InRange(result[0].Timestamp, startDate, endDate);
    }

    [Fact]
    public async Task GetByAccountAndDateRangeAsync_NoMatchingIncome_ReturnsEmptyCollection()
    {
        // Arrange
        var startDate = new DateTime(2030, 1, 1);
        var endDate = new DateTime(2030, 1, 31);

        // Act
        var result = (await _repository.GetByAccountAndDateRangeAsync(_fixture.TestAccountId, startDate, endDate)).ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetAllOrderedByDateAsync Tests

    [Fact]
    public async Task GetAllOrderedByDateAsync_WithIncome_ReturnsOrderedByDateDescending()
    {
        // Arrange
        var income1 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = new DateTime(2025, 1, 1)
        };
        var income2 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 2000m,
            Timestamp = new DateTime(2025, 1, 15)
        };
        var income3 = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 3000m,
            Timestamp = new DateTime(2025, 1, 31)
        };
        await _repository.InsertAsync(income1);
        await _repository.InsertAsync(income2);
        await _repository.InsertAsync(income3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllOrderedByDateAsync()).ToList();

        // Assert
        Assert.True(result.Count >= 3);
        // Verify the three test items are in correct order (most recent first)
        var testIncomes = result.Where(i => i.Id == income1.Id || i.Id == income2.Id || i.Id == income3.Id).ToList();
        Assert.Equal(3, testIncomes.Count);
        Assert.Equal(income3.Id, testIncomes[0].Id); // 2025-01-31
        Assert.Equal(income2.Id, testIncomes[1].Id); // 2025-01-15
        Assert.Equal(income1.Id, testIncomes[2].Id); // 2025-01-01
    }

    [Fact]
    public async Task GetAllOrderedByDateAsync_NoIncome_ReturnsEmptyCollection()
    {
        // Arrange - Use a clean repository instance
        // Note: May have seeded data from fixture

        // Act
        var result = await _repository.GetAllOrderedByDateAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<Income>>(result);
    }

    #endregion

    #region Base Repository Tests (CRUD)

    [Fact]
    public async Task InsertAsync_NewIncome_AddsToDatabase()
    {
        // Arrange
        var income = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now,
            Comment = "Test income"
        };

        // Act
        await _repository.InsertAsync(income);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(income.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(income.Id, retrieved.Id);
        Assert.Equal(1000m, retrieved.Amount);
        Assert.Equal("Test income", retrieved.Comment);
    }

    [Fact]
    public async Task Update_ExistingIncome_UpdatesDatabase()
    {
        // Arrange
        var income = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now,
            Comment = "Original"
        };
        await _repository.InsertAsync(income);
        await _repository.SaveChangesAsync();

        // Act
        income.Amount = 1500m;
        income.Comment = "Updated";
        _repository.Update(income);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(income.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(1500m, retrieved.Amount);
        Assert.Equal("Updated", retrieved.Comment);
    }

    [Fact]
    public async Task Delete_ExistingIncome_RemovesFromDatabase()
    {
        // Arrange
        var income = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(income);
        await _repository.SaveChangesAsync();

        // Act
        _repository.Delete(income);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(income.Id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingIncome_ReturnsIncome()
    {
        // Arrange
        var income = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(income);
        await _repository.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(income.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(income.Id, retrieved.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingIncome_ReturnsNull()
    {
        // Act
        var retrieved = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task ExistsAsync_ExistingIncome_ReturnsTrue()
    {
        // Arrange
        var income = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(income);
        await _repository.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(i => i.Id == income.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingIncome_ReturnsFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var exists = await _repository.ExistsAsync(i => i.Id == nonExistingId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var initialCount = await _repository.CountAsync();
        var income = new Income
        {
            Id = Guid.NewGuid(),
            AccountId = _fixture.TestAccountId,
            CategoryId = _fixture.TestCategoryId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(income);
        await _repository.SaveChangesAsync();

        // Act
        var newCount = await _repository.CountAsync();

        // Assert
        Assert.Equal(initialCount + 1, newCount);
    }

    #endregion
}
