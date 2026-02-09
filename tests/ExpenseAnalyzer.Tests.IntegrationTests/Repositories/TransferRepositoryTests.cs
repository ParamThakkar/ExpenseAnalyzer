using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;
using ExpenseAnalyzer.Tests.IntegrationTests.Fixtures;

namespace ExpenseAnalyzer.Tests.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for TransferRepository with domain-specific query methods
/// Tests real database interactions for transfer operations between accounts
/// </summary>
[Collection("Database")]
[Trait("Category", "Integration")]
public class TransferRepositoryTests
{
    private readonly DatabaseFixture _fixture;
    private readonly TransferRepository _repository;

    public TransferRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new TransferRepository(_fixture.Context);
    }

    #region GetByOutgoingAccountAsync Tests

    [Fact]
    public async Task GetByOutgoingAccountAsync_WithTransfers_ReturnsTransfersFromAccount()
    {
        // Arrange
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 500m,
            Timestamp = DateTime.Now
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.ThirdAccountId,
            Amount = 300m,
            Timestamp = DateTime.Now.AddDays(-1)
        };
        var transfer3 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.SecondAccountId,
            IncomingAccountId = _fixture.TestAccountId,
            Amount = 200m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.InsertAsync(transfer3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByOutgoingAccountAsync(_fixture.TestAccountId)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(_fixture.TestAccountId, t.OutgoingAccountId));
        // Verify ordering by timestamp descending
        Assert.True(result[0].Timestamp >= result[1].Timestamp);
    }

    [Fact]
    public async Task GetByOutgoingAccountAsync_NoTransfers_ReturnsEmptyCollection()
    {
        // Act
        var result = (await _repository.GetByOutgoingAccountAsync(Guid.NewGuid())).ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetByIncomingAccountAsync Tests

    [Fact]
    public async Task GetByIncomingAccountAsync_WithTransfers_ReturnsTransfersToAccount()
    {
        // Arrange
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.SecondAccountId,
            IncomingAccountId = _fixture.TestAccountId,
            Amount = 400m,
            Timestamp = DateTime.Now
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.ThirdAccountId,
            IncomingAccountId = _fixture.TestAccountId,
            Amount = 600m,
            Timestamp = DateTime.Now.AddDays(-2)
        };
        var transfer3 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 100m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.InsertAsync(transfer3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByIncomingAccountAsync(_fixture.TestAccountId)).ToList();

        // Assert
        // Filter to only our test transfers
        var ourTransfers = result.Where(t => t.Id == transfer1.Id || t.Id == transfer2.Id).ToList();
        Assert.Equal(2, ourTransfers.Count);
        Assert.All(ourTransfers, t => Assert.Equal(_fixture.TestAccountId, t.IncomingAccountId));
        // Verify ordering by timestamp descending
        Assert.True(ourTransfers[0].Timestamp >= ourTransfers[1].Timestamp);
    }

    [Fact]
    public async Task GetByIncomingAccountAsync_NoTransfers_ReturnsEmptyCollection()
    {
        // Act
        var result = (await _repository.GetByIncomingAccountAsync(Guid.NewGuid())).ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetByAccountAsync Tests

    [Fact]
    public async Task GetByAccountAsync_WithTransfers_ReturnsAllTransfersInvolvingAccount()
    {
        // Arrange
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 250m,
            Timestamp = DateTime.Now
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.ThirdAccountId,
            IncomingAccountId = _fixture.TestAccountId,
            Amount = 150m,
            Timestamp = DateTime.Now.AddDays(-1)
        };
        var transfer3 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.SecondAccountId,
            IncomingAccountId = _fixture.ThirdAccountId,
            Amount = 500m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.InsertAsync(transfer3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountAsync(_fixture.TestAccountId)).ToList();

        // Assert
        // Filter to only our test transfers
        var ourTransfers = result.Where(t => t.Id == transfer1.Id || t.Id == transfer2.Id).ToList();
        Assert.Equal(2, ourTransfers.Count);
        Assert.All(ourTransfers, t =>
            Assert.True(t.OutgoingAccountId == _fixture.TestAccountId ||
                       t.IncomingAccountId == _fixture.TestAccountId));
        // Verify ordering by timestamp descending
        Assert.True(ourTransfers[0].Timestamp >= ourTransfers[1].Timestamp);
    }

    [Fact]
    public async Task GetByAccountAsync_NoTransfers_ReturnsEmptyCollection()
    {
        // Act
        var result = (await _repository.GetByAccountAsync(Guid.NewGuid())).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByAccountAsync_AccountAsOutgoingAndIncoming_ReturnsBothTransfers()
    {
        // Arrange
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 100m,
            Timestamp = DateTime.Now
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.SecondAccountId,
            IncomingAccountId = _fixture.TestAccountId,
            Amount = 200m,
            Timestamp = DateTime.Now.AddHours(-1)
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountAsync(_fixture.TestAccountId)).ToList();

        // Assert
        // Filter to only our test transfers
        var ourTransfers = result.Where(t => t.Id == transfer1.Id || t.Id == transfer2.Id).ToList();
        Assert.Equal(2, ourTransfers.Count);
        var outgoing = ourTransfers.FirstOrDefault(t => t.OutgoingAccountId == _fixture.TestAccountId && t.Id == transfer1.Id);
        var incoming = ourTransfers.FirstOrDefault(t => t.IncomingAccountId == _fixture.TestAccountId && t.Id == transfer2.Id);
        Assert.NotNull(outgoing);
        Assert.NotNull(incoming);
    }

    #endregion

    #region GetByDateRangeAsync Tests

    [Fact]
    public async Task GetByDateRangeAsync_WithTransfersInRange_ReturnsTransfersInRange()
    {
        // Arrange
        var startDate = new DateTime(2025, 2, 1);
        var endDate = new DateTime(2025, 2, 28);
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 350m,
            Timestamp = new DateTime(2025, 2, 15)
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 450m,
            Timestamp = new DateTime(2025, 3, 1) // Outside range
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByDateRangeAsync(startDate, endDate)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(transfer1.Id, result[0].Id);
        Assert.InRange(result[0].Timestamp, startDate, endDate);
    }

    [Fact]
    public async Task GetByDateRangeAsync_NoTransfersInRange_ReturnsEmptyCollection()
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
    public async Task GetByDateRangeAsync_TransfersOnBoundaries_ReturnsTransfers()
    {
        // Arrange
        var startDate = new DateTime(2025, 4, 1);
        var endDate = new DateTime(2025, 4, 30);
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 200m,
            Timestamp = startDate // Exactly on start boundary
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 300m,
            Timestamp = endDate // Exactly on end boundary
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByDateRangeAsync(startDate, endDate)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByDateRangeAsync_MultipleTransfers_ReturnsOrderedByTimestampDescending()
    {
        // Arrange
        var startDate = new DateTime(2025, 5, 1);
        var endDate = new DateTime(2025, 5, 31);
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 100m,
            Timestamp = new DateTime(2025, 5, 5)
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 200m,
            Timestamp = new DateTime(2025, 5, 20)
        };
        var transfer3 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 300m,
            Timestamp = new DateTime(2025, 5, 10)
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.InsertAsync(transfer3);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByDateRangeAsync(startDate, endDate)).ToList();

        // Assert
        Assert.True(result.Count >= 3);
        var testTransfers = result.Where(t => t.Id == transfer1.Id || t.Id == transfer2.Id || t.Id == transfer3.Id).ToList();
        Assert.Equal(3, testTransfers.Count);
        // Most recent first
        Assert.Equal(transfer2.Id, testTransfers[0].Id); // 2025-05-20
        Assert.Equal(transfer3.Id, testTransfers[1].Id); // 2025-05-10
        Assert.Equal(transfer1.Id, testTransfers[2].Id); // 2025-05-05
    }

    #endregion

    #region GetByAccountAndDateRangeAsync Tests

    [Fact]
    public async Task GetByAccountAndDateRangeAsync_WithMatchingTransfers_ReturnsTransfers()
    {
        // Arrange
        var startDate = new DateTime(2025, 6, 1);
        var endDate = new DateTime(2025, 6, 30);
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 175m,
            Timestamp = new DateTime(2025, 6, 15)
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.SecondAccountId,
            IncomingAccountId = _fixture.TestAccountId,
            Amount = 275m,
            Timestamp = new DateTime(2025, 6, 20)
        };
        var transfer3 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.ThirdAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 125m,
            Timestamp = new DateTime(2025, 6, 15) // Same date, different account
        };
        var transfer4 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 225m,
            Timestamp = new DateTime(2025, 7, 1) // Same account, outside date range
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.InsertAsync(transfer3);
        await _repository.InsertAsync(transfer4);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountAndDateRangeAsync(_fixture.TestAccountId, startDate, endDate)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t =>
            Assert.True(t.OutgoingAccountId == _fixture.TestAccountId ||
                       t.IncomingAccountId == _fixture.TestAccountId));
        Assert.All(result, t => Assert.InRange(t.Timestamp, startDate, endDate));
        // Verify ordering by timestamp descending
        Assert.True(result[0].Timestamp >= result[1].Timestamp);
    }

    [Fact]
    public async Task GetByAccountAndDateRangeAsync_NoMatchingTransfers_ReturnsEmptyCollection()
    {
        // Arrange
        var startDate = new DateTime(2030, 1, 1);
        var endDate = new DateTime(2030, 1, 31);

        // Act
        var result = (await _repository.GetByAccountAndDateRangeAsync(_fixture.TestAccountId, startDate, endDate)).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByAccountAndDateRangeAsync_OutgoingTransfersOnly_ReturnsTransfers()
    {
        // Arrange
        var startDate = new DateTime(2025, 7, 1);
        var endDate = new DateTime(2025, 7, 31);
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 400m,
            Timestamp = new DateTime(2025, 7, 10)
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.ThirdAccountId,
            Amount = 500m,
            Timestamp = new DateTime(2025, 7, 20)
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountAndDateRangeAsync(_fixture.TestAccountId, startDate, endDate)).ToList();

        // Assert
        // Filter to only our test transfers
        var ourTransfers = result.Where(t => t.Id == transfer1.Id || t.Id == transfer2.Id).ToList();
        Assert.Equal(2, ourTransfers.Count);
        Assert.All(ourTransfers, t => Assert.Equal(_fixture.TestAccountId, t.OutgoingAccountId));
    }

    [Fact]
    public async Task GetByAccountAndDateRangeAsync_IncomingTransfersOnly_ReturnsTransfers()
    {
        // Arrange
        var startDate = new DateTime(2025, 8, 1);
        var endDate = new DateTime(2025, 8, 31);
        var transfer1 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.SecondAccountId,
            IncomingAccountId = _fixture.TestAccountId,
            Amount = 350m,
            Timestamp = new DateTime(2025, 8, 5)
        };
        var transfer2 = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.ThirdAccountId,
            IncomingAccountId = _fixture.TestAccountId,
            Amount = 450m,
            Timestamp = new DateTime(2025, 8, 25)
        };
        await _repository.InsertAsync(transfer1);
        await _repository.InsertAsync(transfer2);
        await _repository.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByAccountAndDateRangeAsync(_fixture.TestAccountId, startDate, endDate)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(_fixture.TestAccountId, t.IncomingAccountId));
    }

    #endregion

    #region Base Repository Tests (CRUD)

    [Fact]
    public async Task InsertAsync_NewTransfer_AddsToDatabase()
    {
        // Arrange
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 750m,
            Timestamp = DateTime.Now,
            Comment = "Test transfer"
        };

        // Act
        await _repository.InsertAsync(transfer);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(transfer.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(transfer.Id, retrieved.Id);
        Assert.Equal(750m, retrieved.Amount);
        Assert.Equal("Test transfer", retrieved.Comment);
    }

    [Fact]
    public async Task Update_ExistingTransfer_UpdatesDatabase()
    {
        // Arrange
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 600m,
            Timestamp = DateTime.Now,
            Comment = "Original"
        };
        await _repository.InsertAsync(transfer);
        await _repository.SaveChangesAsync();

        // Act
        transfer.Amount = 800m;
        transfer.Comment = "Updated";
        _repository.Update(transfer);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(transfer.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(800m, retrieved.Amount);
        Assert.Equal("Updated", retrieved.Comment);
    }

    [Fact]
    public async Task Delete_ExistingTransfer_RemovesFromDatabase()
    {
        // Arrange
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 250m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(transfer);
        await _repository.SaveChangesAsync();

        // Act
        _repository.Delete(transfer);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(transfer.Id);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTransfer_ReturnsTransfer()
    {
        // Arrange
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 550m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(transfer);
        await _repository.SaveChangesAsync();

        // Act
        var retrieved = await _repository.GetByIdAsync(transfer.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(transfer.Id, retrieved.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTransfer_ReturnsNull()
    {
        // Act
        var retrieved = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task ExistsAsync_ExistingTransfer_ReturnsTrue()
    {
        // Arrange
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 650m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(transfer);
        await _repository.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(t => t.Id == transfer.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingTransfer_ReturnsFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var exists = await _repository.ExistsAsync(t => t.Id == nonExistingId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var initialCount = await _repository.CountAsync();
        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            OutgoingAccountId = _fixture.TestAccountId,
            IncomingAccountId = _fixture.SecondAccountId,
            Amount = 425m,
            Timestamp = DateTime.Now
        };
        await _repository.InsertAsync(transfer);
        await _repository.SaveChangesAsync();

        // Act
        var newCount = await _repository.CountAsync();

        // Assert
        Assert.Equal(initialCount + 1, newCount);
    }

    #endregion
}
