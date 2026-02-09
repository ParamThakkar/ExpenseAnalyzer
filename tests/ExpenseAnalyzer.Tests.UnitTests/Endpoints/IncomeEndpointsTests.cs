using Moq;
using Microsoft.AspNetCore.Http.HttpResults;
using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;
using ExpenseAnalyzer.Api.Endpoints;

namespace ExpenseAnalyzer.Tests.UnitTests.Endpoints;

[Trait("Category", "Unit")]
public class IncomeEndpointsTests
{
    private readonly Mock<IIncomeRepository> _mockRepo;
    private readonly Guid _testCategoryId = Guid.NewGuid();
    private readonly Guid _testAccountId = Guid.NewGuid();

    public IncomeEndpointsTests()
    {
        _mockRepo = new Mock<IIncomeRepository>();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithIncome_ReturnsOkWithIncome()
    {
        // Arrange
        var income = new List<Income>
        {
            new() { Id = Guid.NewGuid(), Amount = 1000m, CategoryId = _testCategoryId, AccountId = _testAccountId, Timestamp = DateTime.Now },
            new() { Id = Guid.NewGuid(), Amount = 2000m, CategoryId = _testCategoryId, AccountId = _testAccountId, Timestamp = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetAllOrderedByDateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        // Act
        var result = await IncomeEndpoints.GetAllAsync(_mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Income>>>(result);
        Assert.Equal(2, okResult.Value!.Count());
        _mockRepo.Verify(r => r.GetAllOrderedByDateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_NoIncome_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var emptyIncome = new List<Income>();
        _mockRepo.Setup(r => r.GetAllOrderedByDateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyIncome);

        // Act
        var result = await IncomeEndpoints.GetAllAsync(_mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Income>>>(result);
        Assert.Empty(okResult.Value!);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingIncome_ReturnsOkWithIncome()
    {
        // Arrange
        var id = Guid.NewGuid();
        var income = new Income { Id = id, Amount = 1000m, CategoryId = _testCategoryId, AccountId = _testAccountId, Timestamp = DateTime.Now };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        // Act
        var result = await IncomeEndpoints.GetByIdAsync(id, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<Income>>(result);
        Assert.Equal(id, okResult.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingIncome_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Income?)null);

        // Act
        var result = await IncomeEndpoints.GetByIdAsync(id, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    #endregion

    #region GetByAccountAsync Tests

    [Fact]
    public async Task GetByAccountAsync_WithIncome_ReturnsOkWithIncome()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var income = new List<Income>
        {
            new() { Id = Guid.NewGuid(), Amount = 1000m, CategoryId = _testCategoryId, AccountId = accountId, Timestamp = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetByAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        // Act
        var result = await IncomeEndpoints.GetByAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Income>>>(result);
        Assert.Single(okResult.Value!);
    }

    #endregion

    #region GetByCategoryAsync Tests

    [Fact]
    public async Task GetByCategoryAsync_WithIncome_ReturnsOkWithIncome()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var income = new List<Income>
        {
            new() { Id = Guid.NewGuid(), Amount = 1000m, CategoryId = categoryId, AccountId = _testAccountId, Timestamp = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        // Act
        var result = await IncomeEndpoints.GetByCategoryAsync(categoryId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Income>>>(result);
        Assert.Single(okResult.Value!);
    }

    #endregion

    #region GetByDateRangeAsync Tests

    [Fact]
    public async Task GetByDateRangeAsync_ValidRange_ReturnsOkWithIncome()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        var income = new List<Income>
        {
            new() { Id = Guid.NewGuid(), Amount = 1000m, CategoryId = _testCategoryId, AccountId = _testAccountId, Timestamp = new DateTime(2025, 1, 15) }
        };
        _mockRepo.Setup(r => r.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);

        // Act
        var result = await IncomeEndpoints.GetByDateRangeAsync(startDate, endDate, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Income>>>(result);
        Assert.Single(okResult.Value!);
    }

    [Fact]
    public async Task GetByDateRangeAsync_InvalidRange_ReturnsBadRequest()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 31);
        var endDate = new DateTime(2025, 1, 1);

        // Act
        var result = await IncomeEndpoints.GetByDateRangeAsync(startDate, endDate, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Start date must be before end date", badRequest.Value);
        _mockRepo.Verify(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetTotalByAccountAsync Tests

    [Fact]
    public async Task GetTotalByAccountAsync_WithIncome_ReturnsOkWithTotal()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var total = 5000m;
        _mockRepo.Setup(r => r.GetTotalByAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(total);

        // Act
        var result = await IncomeEndpoints.GetTotalByAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<decimal>>(result);
        Assert.Equal(total, okResult.Value);
    }

    [Fact]
    public async Task GetTotalByAccountAsync_NoIncome_ReturnsOkWithZero()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetTotalByAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        // Act
        var result = await IncomeEndpoints.GetTotalByAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<decimal>>(result);
        Assert.Equal(0m, okResult.Value);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidIncome_ReturnsCreatedWithIncome()
    {
        // Arrange
        var income = new Income
        {
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = 1000m,
            Timestamp = DateTime.Now,
            Comment = "Test income"
        };
        _mockRepo.Setup(r => r.InsertAsync(It.IsAny<Income>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await IncomeEndpoints.CreateAsync(income, _mockRepo.Object, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<Created<Income>>(result);
        Assert.NotEqual(Guid.Empty, createdResult.Value!.Id);
        Assert.Contains("/api/v1/income/", createdResult.Location);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Income>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyCategoryId_ReturnsBadRequest()
    {
        // Arrange
        var income = new Income
        {
            CategoryId = Guid.Empty,
            AccountId = _testAccountId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };

        // Act
        var result = await IncomeEndpoints.CreateAsync(income, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("CategoryId is required", badRequest.Value);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Income>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_EmptyAccountId_ReturnsBadRequest()
    {
        // Arrange
        var income = new Income
        {
            CategoryId = _testCategoryId,
            AccountId = Guid.Empty,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };

        // Act
        var result = await IncomeEndpoints.CreateAsync(income, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("AccountId is required", badRequest.Value);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Income>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task CreateAsync_InvalidAmount_ReturnsBadRequest(decimal amount)
    {
        // Arrange
        var income = new Income
        {
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = amount,
            Timestamp = DateTime.Now
        };

        // Act
        var result = await IncomeEndpoints.CreateAsync(income, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Amount must be greater than zero", badRequest.Value);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Income>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingIncome_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = id,
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        var updatedIncome = new Income
        {
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = 1500m,
            Timestamp = DateTime.Now,
            Comment = "Updated"
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);
        _mockRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await IncomeEndpoints.UpdateAsync(id, updatedIncome, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NoContent>(result);
        _mockRepo.Verify(r => r.Update(existingIncome), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1500m, existingIncome.Amount);
        Assert.Equal("Updated", existingIncome.Comment);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingIncome_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var income = new Income
        {
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Income?)null);

        // Act
        var result = await IncomeEndpoints.UpdateAsync(id, income, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NotFound>(result);
        _mockRepo.Verify(r => r.Update(It.IsAny<Income>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmptyCategoryId_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = id,
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        var updatedIncome = new Income
        {
            CategoryId = Guid.Empty,
            AccountId = _testAccountId,
            Amount = 1500m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        // Act
        var result = await IncomeEndpoints.UpdateAsync(id, updatedIncome, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("CategoryId is required", badRequest.Value);
        _mockRepo.Verify(r => r.Update(It.IsAny<Income>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmptyAccountId_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = id,
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        var updatedIncome = new Income
        {
            CategoryId = _testCategoryId,
            AccountId = Guid.Empty,
            Amount = 1500m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        // Act
        var result = await IncomeEndpoints.UpdateAsync(id, updatedIncome, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("AccountId is required", badRequest.Value);
        _mockRepo.Verify(r => r.Update(It.IsAny<Income>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public async Task UpdateAsync_InvalidAmount_ReturnsBadRequest(decimal amount)
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingIncome = new Income
        {
            Id = id,
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        var updatedIncome = new Income
        {
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = amount,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingIncome);

        // Act
        var result = await IncomeEndpoints.UpdateAsync(id, updatedIncome, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Amount must be greater than zero", badRequest.Value);
        _mockRepo.Verify(r => r.Update(It.IsAny<Income>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingIncome_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var income = new Income
        {
            Id = id,
            CategoryId = _testCategoryId,
            AccountId = _testAccountId,
            Amount = 1000m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(income);
        _mockRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await IncomeEndpoints.DeleteAsync(id, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NoContent>(result);
        _mockRepo.Verify(r => r.Delete(income), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingIncome_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Income?)null);

        // Act
        var result = await IncomeEndpoints.DeleteAsync(id, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NotFound>(result);
        _mockRepo.Verify(r => r.Delete(It.IsAny<Income>()), Times.Never);
    }

    #endregion
}
