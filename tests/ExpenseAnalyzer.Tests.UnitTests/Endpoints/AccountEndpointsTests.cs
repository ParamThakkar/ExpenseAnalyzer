using Moq;
using Microsoft.AspNetCore.Http.HttpResults;
using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;

namespace ExpenseAnalyzer.Tests.UnitTests.Endpoints;

[Trait("Category", "Unit")]
public class AccountEndpointsTests
{
    private readonly Mock<IAccountRepository> _mockRepo;

    public AccountEndpointsTests()
    {
        _mockRepo = new Mock<IAccountRepository>();
    }

    [Fact]
    public async Task GetAllTasksAsync_WithAccounts_CallsRepositoryGetAllAsync()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new() { Id = Guid.NewGuid(), Name = "Account1" },
            new() { Id = Guid.NewGuid(), Name = "Account2" },
            new() { Id = Guid.NewGuid(), Name = "Account3" }
        };
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        // Act
        await AccountEndpoints.GetAllTasksAsync(_mockRepo.Object, CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTasksAsync_NoAccounts_CallsRepositoryGetAllAsync()
    {
        // Arrange
        var emptyAccounts = new List<Account>();
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyAccounts);

        // Act
        await AccountEndpoints.GetAllTasksAsync(_mockRepo.Object, CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTasksAsync_CallsRepositoryOnce()
    {
        // Arrange
        var accounts = new List<Account>();
        _mockRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        // Act
        await AccountEndpoints.GetAllTasksAsync(_mockRepo.Object, CancellationToken.None);

        // Assert
        _mockRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
