using Moq;
using Microsoft.AspNetCore.Http.HttpResults;
using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;
using ExpenseAnalyzer.Api.Endpoints;

namespace ExpenseAnalyzer.Tests.UnitTests.Endpoints;

/// <summary>
/// Unit tests for TransferEndpoints with mocked dependencies
/// Tests all API endpoint handlers for transfer operations
/// </summary>
[Trait("Category", "Unit")]
public class TransferEndpointsTests
{
    private readonly Mock<ITransferRepository> _mockRepo;
    private readonly Guid _testOutgoingAccountId = Guid.NewGuid();
    private readonly Guid _testIncomingAccountId = Guid.NewGuid();

    public TransferEndpointsTests()
    {
        _mockRepo = new Mock<ITransferRepository>();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithTransfers_ReturnsOkWithTransfers()
    {
        // Arrange
        var transfers = new List<Transfer>
        {
            new() { Id = Guid.NewGuid(), OutgoingAccountId = _testOutgoingAccountId, IncomingAccountId = _testIncomingAccountId, Amount = 500m, Timestamp = DateTime.Now },
            new() { Id = Guid.NewGuid(), OutgoingAccountId = _testOutgoingAccountId, IncomingAccountId = _testIncomingAccountId, Amount = 750m, Timestamp = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfers);

        // Act
        var result = await TransferEndpoints.GetAllAsync(_mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Equal(2, okResult.Value!.Count());
        _mockRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_NoTransfers_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var emptyTransfers = new List<Transfer>();
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyTransfers);

        // Act
        var result = await TransferEndpoints.GetAllAsync(_mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Empty(okResult.Value!);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingTransfer_ReturnsOkWithTransfer()
    {
        // Arrange
        var id = Guid.NewGuid();
        var transfer = new Transfer { Id = id, OutgoingAccountId = _testOutgoingAccountId, IncomingAccountId = _testIncomingAccountId, Amount = 600m, Timestamp = DateTime.Now };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);

        // Act
        var result = await TransferEndpoints.GetByIdAsync(id, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<Transfer>>(result);
        Assert.Equal(id, okResult.Value!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTransfer_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transfer?)null);

        // Act
        var result = await TransferEndpoints.GetByIdAsync(id, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    #endregion

    #region GetByAccountAsync Tests

    [Fact]
    public async Task GetByAccountAsync_WithTransfers_ReturnsOkWithTransfers()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transfers = new List<Transfer>
        {
            new() { Id = Guid.NewGuid(), OutgoingAccountId = accountId, IncomingAccountId = _testIncomingAccountId, Amount = 300m, Timestamp = DateTime.Now },
            new() { Id = Guid.NewGuid(), OutgoingAccountId = _testOutgoingAccountId, IncomingAccountId = accountId, Amount = 400m, Timestamp = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetByAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfers);

        // Act
        var result = await TransferEndpoints.GetByAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Equal(2, okResult.Value!.Count());
    }

    [Fact]
    public async Task GetByAccountAsync_NoTransfers_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var emptyTransfers = new List<Transfer>();
        _mockRepo.Setup(r => r.GetByAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyTransfers);

        // Act
        var result = await TransferEndpoints.GetByAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Empty(okResult.Value!);
    }

    #endregion

    #region GetByOutgoingAccountAsync Tests

    [Fact]
    public async Task GetByOutgoingAccountAsync_WithTransfers_ReturnsOkWithTransfers()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transfers = new List<Transfer>
        {
            new() { Id = Guid.NewGuid(), OutgoingAccountId = accountId, IncomingAccountId = _testIncomingAccountId, Amount = 250m, Timestamp = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetByOutgoingAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfers);

        // Act
        var result = await TransferEndpoints.GetByOutgoingAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Single(okResult.Value!);
    }

    [Fact]
    public async Task GetByOutgoingAccountAsync_NoTransfers_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var emptyTransfers = new List<Transfer>();
        _mockRepo.Setup(r => r.GetByOutgoingAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyTransfers);

        // Act
        var result = await TransferEndpoints.GetByOutgoingAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Empty(okResult.Value!);
    }

    #endregion

    #region GetByIncomingAccountAsync Tests

    [Fact]
    public async Task GetByIncomingAccountAsync_WithTransfers_ReturnsOkWithTransfers()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transfers = new List<Transfer>
        {
            new() { Id = Guid.NewGuid(), OutgoingAccountId = _testOutgoingAccountId, IncomingAccountId = accountId, Amount = 350m, Timestamp = DateTime.Now }
        };
        _mockRepo.Setup(r => r.GetByIncomingAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfers);

        // Act
        var result = await TransferEndpoints.GetByIncomingAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Single(okResult.Value!);
    }

    [Fact]
    public async Task GetByIncomingAccountAsync_NoTransfers_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var emptyTransfers = new List<Transfer>();
        _mockRepo.Setup(r => r.GetByIncomingAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyTransfers);

        // Act
        var result = await TransferEndpoints.GetByIncomingAccountAsync(accountId, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Empty(okResult.Value!);
    }

    #endregion

    #region GetByDateRangeAsync Tests

    [Fact]
    public async Task GetByDateRangeAsync_ValidRange_ReturnsOkWithTransfers()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 31);
        var transfers = new List<Transfer>
        {
            new() { Id = Guid.NewGuid(), OutgoingAccountId = _testOutgoingAccountId, IncomingAccountId = _testIncomingAccountId, Amount = 450m, Timestamp = new DateTime(2025, 1, 15) }
        };
        _mockRepo.Setup(r => r.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfers);

        // Act
        var result = await TransferEndpoints.GetByDateRangeAsync(startDate, endDate, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Single(okResult.Value!);
    }

    [Fact]
    public async Task GetByDateRangeAsync_InvalidRange_ReturnsBadRequest()
    {
        // Arrange
        var startDate = new DateTime(2025, 1, 31);
        var endDate = new DateTime(2025, 1, 1);

        // Act
        var result = await TransferEndpoints.GetByDateRangeAsync(startDate, endDate, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Start date must be before end date", badRequest.Value);
        _mockRepo.Verify(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByDateRangeAsync_NoTransfersInRange_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var startDate = new DateTime(2025, 2, 1);
        var endDate = new DateTime(2025, 2, 28);
        var emptyTransfers = new List<Transfer>();
        _mockRepo.Setup(r => r.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyTransfers);

        // Act
        var result = await TransferEndpoints.GetByDateRangeAsync(startDate, endDate, _mockRepo.Object, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<Transfer>>>(result);
        Assert.Empty(okResult.Value!);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidTransfer_ReturnsCreatedWithTransfer()
    {
        // Arrange
        var transfer = new Transfer
        {
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 550m,
            Timestamp = DateTime.Now,
            Comment = "Test transfer"
        };
        _mockRepo.Setup(r => r.InsertAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await TransferEndpoints.CreateAsync(transfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<Created<Transfer>>(result);
        Assert.NotEqual(Guid.Empty, createdResult.Value!.Id);
        Assert.Contains("/api/v1/transfers/", createdResult.Location);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyOutgoingAccountId_ReturnsBadRequest()
    {
        // Arrange
        var transfer = new Transfer
        {
            OutgoingAccountId = Guid.Empty,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 500m,
            Timestamp = DateTime.Now
        };

        // Act
        var result = await TransferEndpoints.CreateAsync(transfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("OutgoingAccountId is required", badRequest.Value);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_EmptyIncomingAccountId_ReturnsBadRequest()
    {
        // Arrange
        var transfer = new Transfer
        {
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = Guid.Empty,
            Amount = 500m,
            Timestamp = DateTime.Now
        };

        // Act
        var result = await TransferEndpoints.CreateAsync(transfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("IncomingAccountId is required", badRequest.Value);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SameOutgoingAndIncomingAccountId_ReturnsBadRequest()
    {
        // Arrange
        var sameAccountId = Guid.NewGuid();
        var transfer = new Transfer
        {
            OutgoingAccountId = sameAccountId,
            IncomingAccountId = sameAccountId,
            Amount = 500m,
            Timestamp = DateTime.Now
        };

        // Act
        var result = await TransferEndpoints.CreateAsync(transfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("OutgoingAccountId and IncomingAccountId must be different", badRequest.Value);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-250.75)]
    public async Task CreateAsync_InvalidAmount_ReturnsBadRequest(decimal amount)
    {
        // Arrange
        var transfer = new Transfer
        {
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = amount,
            Timestamp = DateTime.Now
        };

        // Act
        var result = await TransferEndpoints.CreateAsync(transfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Amount must be greater than zero", badRequest.Value);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_MultipleValidationErrors_ReturnsFirstError()
    {
        // Arrange
        var transfer = new Transfer
        {
            OutgoingAccountId = Guid.Empty, // Invalid
            IncomingAccountId = Guid.Empty, // Invalid
            Amount = -100m, // Invalid
            Timestamp = DateTime.Now
        };

        // Act
        var result = await TransferEndpoints.CreateAsync(transfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        // Should return OutgoingAccountId error first (order of validation)
        Assert.Contains("OutgoingAccountId is required", badRequest.Value);
        _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Transfer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ExistingTransfer_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingTransfer = new Transfer
        {
            Id = id,
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 400m,
            Timestamp = DateTime.Now
        };
        var updatedTransfer = new Transfer
        {
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 650m,
            Timestamp = DateTime.Now.AddDays(1),
            Comment = "Updated transfer"
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransfer);
        _mockRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await TransferEndpoints.UpdateAsync(id, updatedTransfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NoContent>(result);
        _mockRepo.Verify(r => r.Update(existingTransfer), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(650m, existingTransfer.Amount);
        Assert.Equal("Updated transfer", existingTransfer.Comment);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingTransfer_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var transfer = new Transfer
        {
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 500m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transfer?)null);

        // Act
        var result = await TransferEndpoints.UpdateAsync(id, transfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NotFound>(result);
        _mockRepo.Verify(r => r.Update(It.IsAny<Transfer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmptyOutgoingAccountId_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingTransfer = new Transfer
        {
            Id = id,
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 400m,
            Timestamp = DateTime.Now
        };
        var updatedTransfer = new Transfer
        {
            OutgoingAccountId = Guid.Empty,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 500m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransfer);

        // Act
        var result = await TransferEndpoints.UpdateAsync(id, updatedTransfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("OutgoingAccountId is required", badRequest.Value);
        _mockRepo.Verify(r => r.Update(It.IsAny<Transfer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_EmptyIncomingAccountId_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingTransfer = new Transfer
        {
            Id = id,
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 400m,
            Timestamp = DateTime.Now
        };
        var updatedTransfer = new Transfer
        {
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = Guid.Empty,
            Amount = 500m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransfer);

        // Act
        var result = await TransferEndpoints.UpdateAsync(id, updatedTransfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("IncomingAccountId is required", badRequest.Value);
        _mockRepo.Verify(r => r.Update(It.IsAny<Transfer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SameOutgoingAndIncomingAccountId_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sameAccountId = Guid.NewGuid();
        var existingTransfer = new Transfer
        {
            Id = id,
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 400m,
            Timestamp = DateTime.Now
        };
        var updatedTransfer = new Transfer
        {
            OutgoingAccountId = sameAccountId,
            IncomingAccountId = sameAccountId,
            Amount = 500m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransfer);

        // Act
        var result = await TransferEndpoints.UpdateAsync(id, updatedTransfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("OutgoingAccountId and IncomingAccountId must be different", badRequest.Value);
        _mockRepo.Verify(r => r.Update(It.IsAny<Transfer>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-350.50)]
    public async Task UpdateAsync_InvalidAmount_ReturnsBadRequest(decimal amount)
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingTransfer = new Transfer
        {
            Id = id,
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 400m,
            Timestamp = DateTime.Now
        };
        var updatedTransfer = new Transfer
        {
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = amount,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransfer);

        // Act
        var result = await TransferEndpoints.UpdateAsync(id, updatedTransfer, _mockRepo.Object, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Amount must be greater than zero", badRequest.Value);
        _mockRepo.Verify(r => r.Update(It.IsAny<Transfer>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingTransfer_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var transfer = new Transfer
        {
            Id = id,
            OutgoingAccountId = _testOutgoingAccountId,
            IncomingAccountId = _testIncomingAccountId,
            Amount = 450m,
            Timestamp = DateTime.Now
        };
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transfer);
        _mockRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await TransferEndpoints.DeleteAsync(id, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NoContent>(result);
        _mockRepo.Verify(r => r.Delete(transfer), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingTransfer_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transfer?)null);

        // Act
        var result = await TransferEndpoints.DeleteAsync(id, _mockRepo.Object, CancellationToken.None);

        // Assert
        Assert.IsType<NotFound>(result);
        _mockRepo.Verify(r => r.Delete(It.IsAny<Transfer>()), Times.Never);
    }

    #endregion
}
