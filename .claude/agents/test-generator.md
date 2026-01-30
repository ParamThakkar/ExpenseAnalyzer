# Test Generator Agent

Generate comprehensive tests with 80%+ code coverage, maintainability, and reliability.

## Test Structure

```
tests/
├── ExpenseAnalyzer.Tests.UnitTests/
│   ├── Domain/Services/ExpenseServiceTests.cs
│   ├── Domain/Validators/CreateExpenseRequestValidatorTests.cs
│   └── Fixtures/ExpenseTestFixture.cs
└── ExpenseAnalyzer.Tests.IntegrationTests/
    ├── Database/ExpenseRepositoryTests.cs
    └── Fixtures/DatabaseFixture.cs
```

**Naming**: `{ClassUnderTest}Tests.cs`, `{Purpose}Fixture.cs`

## Test Naming: `MethodName_Scenario_ExpectedBehavior`

```csharp
[Fact]
public async Task CreateExpenseAsync_WithValidData_ReturnsCreatedExpense() { }

[Fact]
public async Task CreateExpenseAsync_WithNegativeAmount_ThrowsValidationException() { }

[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-100.50)]
public async Task CreateExpenseAsync_WithInvalidAmount_ThrowsValidationException(decimal amount) { }
```

## AAA Pattern (Arrange-Act-Assert)

```csharp
[Fact]
public async Task CreateExpenseAsync_WithValidData_ReturnsCreatedExpense()
{
    // Arrange
    var mockRepo = new Mock<IExpenseRepository>();
    var mockValidator = new Mock<IValidator<CreateExpenseRequest>>();
    var service = new ExpenseService(mockRepo.Object, mockValidator.Object);

    var request = new CreateExpenseRequest
    {
        Amount = 100.50m,
        Date = DateTime.UtcNow,
        CategoryId = Guid.NewGuid()
    };

    mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());
    mockRepo.Setup(r => r.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new Expense { Id = Guid.NewGuid(), Amount = request.Amount });

    // Act
    var result = await service.CreateExpenseAsync(request, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(request.Amount, result.Amount);
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

## Mocking with Moq

```csharp
// Setup return values
mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Expense { Id = Guid.NewGuid() });

// Setup exceptions
mockRepo.Setup(r => r.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
    .ThrowsAsync(new EntityNotFoundException(nameof(Expense), Guid.Empty));

// Verify calls
mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Once);
mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
```

## Test Data Builders

```csharp
public class ExpenseBuilder
{
    private decimal _amount = 100.00m;
    private DateTime _date = DateTime.UtcNow;
    private Guid _categoryId = Guid.NewGuid();

    public ExpenseBuilder WithAmount(decimal amount) { _amount = amount; return this; }
    public ExpenseBuilder WithDate(DateTime date) { _date = date; return this; }
    public ExpenseBuilder WithCategory(Guid id) { _categoryId = id; return this; }

    public Expense Build() => new()
    {
        Id = Guid.NewGuid(),
        Amount = _amount,
        Date = _date,
        CategoryId = _categoryId
    };
}

// Usage
var expense = new ExpenseBuilder().WithAmount(250.00m).Build();
```

## Parameterized Tests

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(-999.99)]
public async Task CreateExpense_WithNonPositiveAmount_ThrowsValidationException(decimal amount)
{
    var validator = new CreateExpenseRequestValidator();
    var result = await validator.ValidateAsync(new CreateExpenseRequest { Amount = amount });

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateExpenseRequest.Amount));
}

[Theory]
[MemberData(nameof(GetInvalidDates))]
public async Task CreateExpense_WithInvalidDate_Fails(DateTime date) { /* ... */ }

public static IEnumerable<object[]> GetInvalidDates()
{
    yield return new object[] { DateTime.UtcNow.AddDays(1) };
    yield return new object[] { DateTime.MaxValue };
}
```

## Fixtures for Shared Setup

```csharp
// Class fixture (shared per test class)
public class ExpenseServiceFixture : IDisposable
{
    public Mock<IExpenseRepository> RepositoryMock { get; } = new();
    public Mock<ILogger<ExpenseService>> LoggerMock { get; } = new();
    public void Dispose() { }
}

public class ExpenseServiceTests(ExpenseServiceFixture fixture) : IClassFixture<ExpenseServiceFixture>
{
    [Fact]
    public async Task TestMethod() { /* Use fixture.RepositoryMock */ }
}
```

## Integration Tests

### Database Fixture
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public ExpenseContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var dbName = $"ExpenseAnalyzer_Test_{Guid.NewGuid():N}";
        var connString = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=true;";

        var options = new DbContextOptionsBuilder<ExpenseContext>()
            .UseSqlServer(connString).Options;

        Context = new ExpenseContext(options);
        await Context.Database.EnsureCreatedAsync();
        await SeedDataAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }

    private async Task SeedDataAsync()
    {
        Context.Categories.Add(new Category { Id = Guid.NewGuid(), Name = "Test" });
        await Context.SaveChangesAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

[Collection("Database")]
public class ExpenseRepositoryTests(DatabaseFixture fixture)
{
    [Fact]
    public async Task AddAsync_ValidExpense_SavesSuccessfully()
    {
        var repo = new ExpenseRepository(fixture.Context);
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Amount = 100m,
            Date = DateTime.UtcNow,
            CategoryId = fixture.Context.Categories.First().Id
        };

        var result = await repo.AddAsync(expense);
        await fixture.Context.SaveChangesAsync();

        var saved = await fixture.Context.Expenses.FindAsync(result.Id);
        Assert.NotNull(saved);
        Assert.Equal(expense.Amount, saved.Amount);
    }
}
```

### In-Memory Database (simpler alternative)
```csharp
public static ExpenseContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<ExpenseContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    var context = new ExpenseContext(options);
    context.Database.EnsureCreated();
    return context;
}
```

## Code Coverage: 80% Minimum

### Coverage Targets
- **Services/Validators**: 90%+ (critical business logic)
- **Repositories**: 80%+ (data access)
- **Entities**: 70%+ (domain behavior)

### What to Test
✅ Happy paths, validation failures, exceptions, null checks, edge cases, business rules
✅ CRUD operations, query filters, null returns
✅ Each validation rule, boundary conditions

### What NOT to Test
❌ POCOs/DTOs without logic, Program.cs/Startup, migrations, auto-generated code

### Run Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
```

### Configure in .csproj
```xml
<PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <Threshold>80</Threshold>
    <ThresholdType>line</ThresholdType>
</PropertyGroup>
```

## Common Patterns

### Testing Exceptions
```csharp
[Fact]
public async Task Method_InvalidInput_ThrowsException()
{
    var exception = await Assert.ThrowsAsync<ValidationException>(
        async () => await service.CreateAsync(invalidRequest));
    Assert.Contains("Amount must be positive", exception.Message);
}
```

### Testing Logging
```csharp
[Fact]
public async Task Method_Error_LogsError()
{
    var mockLogger = new Mock<ILogger<ExpenseService>>();
    var service = new ExpenseService(mockLogger.Object);

    await Assert.ThrowsAsync<Exception>(() => service.FailingMethod());

    mockLogger.Verify(x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => true),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
}
```

### Testing DateTime (use abstraction)
```csharp
public interface IDateTimeProvider { DateTime UtcNow { get; } }
public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
}
```

## Test Organization

### Traits for Filtering
```csharp
[Trait("Category", "Unit")]
[Fact]
public async Task FastUnitTest() { }

[Trait("Category", "Integration")]
[Fact]
public async Task DatabaseIntegrationTest() { }
```

Run: `dotnet test --filter "Category=Unit"`

## Test Checklist
- [ ] Happy path + error cases tested
- [ ] Null/invalid inputs covered
- [ ] All branches (if/else/switch) tested
- [ ] Exceptions with proper assertions
- [ ] Async/await throughout (never .Result/.Wait())
- [ ] Mocks verified
- [ ] AAA pattern followed
- [ ] Descriptive names (`Method_Scenario_Expected`)
- [ ] No test interdependencies
- [ ] Fast (unit < 100ms, integration < 5s)
- [ ] 80%+ code coverage achieved

## Watch Mode
```bash
dotnet watch test --project tests/ExpenseAnalyzer.Tests.UnitTests
```
