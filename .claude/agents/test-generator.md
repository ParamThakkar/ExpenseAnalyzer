# Test Generator Agent

Generate comprehensive tests: 80%+ coverage, fast, maintainable, reliable.

## Test Structure

```
tests/
├── UnitTests/
│   ├── Endpoints/AccountEndpointsTests.cs
│   ├── Repositories/AccountRepositoryTests.cs      # Complex repo methods only
│   └── Fixtures/
└── IntegrationTests/
    ├── Repositories/RepositoryIntegrationTests.cs
    ├── Endpoints/AccountEndpointsIntegrationTests.cs
    └── Fixtures/DatabaseFixture.cs
```

**Naming**: `{ClassUnderTest}Tests.cs`, Method: `MethodName_Scenario_ExpectedBehavior`
**Traits**: `[Trait("Category", "Unit")]` or `[Trait("Category", "Integration")]`

## AAA Pattern (Arrange-Act-Assert)

### Unit Test: Minimal API Endpoint
```csharp
[Trait("Category", "Unit")]
public class AccountEndpointsTests
{
    [Fact]
    public async Task GetAllAsync_WithAccounts_ReturnsOkResult()
    {
        // Arrange
        var mockRepo = new Mock<IAccountRepository>();
        var accounts = new[] { new Account { Id = Guid.NewGuid(), Name = "Test" } };
        mockRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(accounts);

        // Act
        var result = await AccountEndpoints.GetAllAsync(mockRepo.Object, default);

        // Assert
        var okResult = Assert.IsType<Ok<IReadOnlyList<Account>>>(result);
        Assert.Single(okResult.Value);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNotFound()
    {
        // Arrange
        var mockRepo = new Mock<IAccountRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Account?)null);

        // Act
        var result = await AccountEndpoints.GetByIdAsync(Guid.NewGuid(), mockRepo.Object, default);

        // Assert
        Assert.IsType<NotFound>(result);
    }
}
```

### Parameterized Tests
```csharp
[Theory]
[InlineData(0), InlineData(-1), InlineData(-100.50)]
public async Task CreateExpense_InvalidAmount_ReturnsBadRequest(decimal amount)
{
    var result = await ExpenseEndpoints.CreateAsync(
        new CreateExpenseRequest { Amount = amount }, mockRepo.Object, default);
    Assert.IsType<BadRequest<string>>(result);
}
```

## Mocking (Moq)

```csharp
// Setup
mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
    .ReturnsAsync(new Expense { Id = Guid.NewGuid(), Amount = 100m });
mockRepo.Setup(r => r.GetByIdAsync(Guid.Empty, default))
    .ReturnsAsync((Expense?)null);

// Verify
mockRepo.Verify(r => r.InsertAsync(It.IsAny<Expense>(), default), Times.Once);
mockRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);

// Test builder
public class ExpenseBuilder
{
    private decimal _amount = 100m;
    public ExpenseBuilder WithAmount(decimal amt) { _amount = amt; return this; }
    public Expense Build() => new() { Id = Guid.NewGuid(), Amount = _amount };
}
```

## Fixtures

```csharp
// Class fixture for shared setup
public class RepositoryFixture : IDisposable
{
    public Mock<IRepository<Expense>> RepoMock { get; } = new();
    public void Dispose() { }
}

[Trait("Category", "Unit")]
public class ExpenseEndpointsTests(RepositoryFixture fixture) : IClassFixture<RepositoryFixture>
{
    [Fact]
    public async Task GetAllAsync_ReturnsAccounts() { /* use fixture.RepoMock */ }
}
```

## Integration Tests

### Database Fixture
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public ExpenseContext Context { get; private set; } = null!;
    public Guid TestCategoryId { get; private set; }

    public async Task InitializeAsync()
    {
        var dbName = $"ExpenseTest_{Guid.NewGuid():N}";
        var conn = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=true;";
        var opts = new DbContextOptionsBuilder<ExpenseContext>()
            .UseSqlServer(conn)
            .UseLazyLoadingProxies()
            .Options;
        Context = new ExpenseContext(opts);
        await Context.Database.EnsureCreatedAsync();

        var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();
        TestCategoryId = category.Id;
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
```

### Repository Integration Test
```csharp
[Collection("Database")]
[Trait("Category", "Integration")]
public class AccountRepositoryTests(DatabaseFixture fixture)
{
    [Fact]
    public async Task GetByNameAsync_ExistingAccount_ReturnsAccount()
    {
        // Arrange
        var repo = new AccountRepository(fixture.Context);
        var account = new Account { Id = Guid.NewGuid(), Name = "TestAccount" };
        await repo.InsertAsync(account);
        await repo.SaveChangesAsync();

        // Act
        var result = await repo.GetByNameAsync("TestAccount", default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Id, result.Id);
    }
}
```

## Code Coverage: 80% Target

**Priorities**: Endpoints 85%+, Specific repositories 80%+, Generic repository 70%+ (integration tests)
**Test**: ✅ Happy paths, error cases, nulls, edge cases, HTTP status codes (Ok/NotFound/BadRequest)
**Skip**: ❌ DTOs, Program.cs, migrations, auto-generated EF code

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:Html
```

## Common Patterns

### Testing IResult Types
```csharp
// Ok result
var result = await AccountEndpoints.GetAllAsync(mockRepo.Object, default);
var okResult = Assert.IsType<Ok<IReadOnlyList<Account>>>(result);
Assert.NotEmpty(okResult.Value);

// NotFound
var result = await AccountEndpoints.GetByIdAsync(Guid.NewGuid(), mockRepo.Object, default);
Assert.IsType<NotFound>(result);

// BadRequest
var result = await ExpenseEndpoints.CreateAsync(invalidRequest, mockRepo.Object, default);
var badRequest = Assert.IsType<BadRequest<string>>(result);
Assert.Contains("Amount", badRequest.Value);

// Created
var result = await ExpenseEndpoints.CreateAsync(validRequest, mockRepo.Object, default);
var created = Assert.IsType<Created<Expense>>(result);
Assert.Equal("/api/v1/expenses/...", created.Location);
```

### Testing Async Repository Methods
```csharp
mockRepo.Setup(r => r.GetAllAsync(default))
    .ReturnsAsync(new[] { new Account { Id = Guid.NewGuid(), Name = "Test" } });

mockRepo.Setup(r => r.InsertAsync(It.IsAny<Expense>(), default))
    .Returns(Task.CompletedTask);

mockRepo.Setup(r => r.SaveChangesAsync(default))
    .ReturnsAsync(1);
```

## Checklist
✅ Happy path + error scenarios, null/invalid inputs, all branches
✅ IResult type assertions (Ok, NotFound, BadRequest, Created)
✅ Mock verification (InsertAsync, SaveChangesAsync called)
✅ AAA pattern, descriptive names (`MethodName_Scenario_ExpectedOutcome`)
✅ Traits: `[Trait("Category", "Unit|Integration")]`
✅ Fast: <100ms (unit), <5s (integration)
✅ 80%+ coverage

**Run**: `dotnet test --filter "Category=Unit"` | `dotnet watch test`
