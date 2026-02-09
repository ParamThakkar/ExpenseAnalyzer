# Test Generator Agent Memory

## Test Project Structure

```
tests/
├── UnitTests/
│   ├── Domain/          # Entity tests (navigation, constraints)
│   ├── Infra/Repos/     # Repository tests (mocked DbContext)
│   └── Api/Endpoints/   # Endpoint logic tests (mocked dependencies)
└── IntegrationTests/
    ├── Infra/Repos/     # Repository tests (real DbContext)
    └── Api/Endpoints/   # API endpoint tests (WebApplicationFactory)
```

## xUnit Conventions

### Test Naming
- **Pattern**: `<Method>_<Scenario>_<ExpectedResult>`
- **Examples**:
  - `GetByIdAsync_ExistingId_ReturnsEntity`
  - `GetByIdAsync_NonExistingId_ReturnsNull`
  - `InsertAsync_ValidEntity_AddsToDatabase`
  - `CreateAccount_EmptyName_ReturnsBadRequest`

### Test Attributes
```csharp
[Fact] // Single test case
[Theory] // Parameterized test
[InlineData(value1, value2)] // Test data for theory
[Trait("Category", "Unit")] // For filtering (Unit/Integration)
```

### Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var entity = new Entity { /* setup */ };

    // Act
    var result = await repository.MethodAsync(entity);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(expected, result);
}
```

## Repository Testing Patterns

### Unit Tests (Mocked DbContext)
**Use when**: Testing repository logic in isolation without database
**Pattern**: Mock `DbSet<T>` and `ExpenseContext`

### Integration Tests (Real Database)
**Use when**: Testing actual database interactions, EF Core queries, constraints
**Pattern**: Use real `ExpenseContext` with in-memory or test database

```csharp
public class AccountRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly ExpenseContext _context;
    private readonly AccountRepository _repository;

    public async Task InitializeAsync()
    {
        // Setup: seed test data
    }

    public async Task DisposeAsync()
    {
        // Cleanup: reset database
    }
}
```

## API Endpoint Testing Patterns

### Integration Tests with WebApplicationFactory
```csharp
public class AccountEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    [Fact]
    public async Task GetAll_ReturnsOkWithAccounts()
    {
        // Arrange
        // (seed data if needed)

        // Act
        var response = await _client.GetAsync("/api/v1/accounts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<List<Account>>();
        Assert.NotNull(accounts);
    }
}
```

### Common Test Scenarios
- ✅ **GET all**: Returns 200 OK with collection (empty or populated)
- ✅ **GET by ID**: Returns 200 OK for existing, 404 NotFound for non-existing
- ✅ **POST**: Returns 201 Created with location header for valid, 400 BadRequest for invalid
- ✅ **PUT**: Returns 204 NoContent for successful update, 404 for non-existing, 400 for invalid
- ✅ **DELETE**: Returns 204 NoContent for successful delete, 404 for non-existing

## Test Data Strategies

### Use Meaningful Test Data
❌ **Avoid**: `var id = Guid.NewGuid();` (non-deterministic, hard to debug)
✅ **Prefer**: `var id = new Guid("11111111-1111-1111-1111-111111111111");` (deterministic, readable)

### Common Test Data Patterns
```csharp
// Descriptive entity creation
var groceryCategory = new Category
{
    Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
    Name = "Groceries"
};

var checkingAccount = new Account
{
    Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
    Name = "Checking"
};
```

## Domain-Specific Test Cases

### Decimal Precision Tests
- Test amounts with 2 decimal places (18,2 precision)
- Verify database stores and retrieves exact decimal values
- Test edge cases: 0.01, 999999999999999999.99

### DateTime/Timestamp Tests
- Test date range queries (start/end boundaries)
- Test ordering by timestamp (ascending/descending)
- Test filtering by specific dates

### FK Constraint Tests
- Test cascade behavior (should be Restrict)
- Test required FKs (cannot be Guid.Empty)
- Test navigation property lazy loading

### Business Logic Tests
- Test domain-specific validations (e.g., Transfer accounts must differ)
- Test aggregations (totals, sums)
- Test filtering combinations (account + date range)

## Common Assertions

```csharp
// Null checks
Assert.NotNull(result);
Assert.Null(result);

// Equality
Assert.Equal(expected, actual);
Assert.NotEqual(expected, actual);

// Collections
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Single(collection);
Assert.Equal(expectedCount, collection.Count);

// Boolean
Assert.True(condition);
Assert.False(condition);

// Exceptions
await Assert.ThrowsAsync<InvalidOperationException>(() => methodCall);

// Type checks
Assert.IsType<Account>(result);
Assert.IsAssignableFrom<IEnumerable<Account>>(result);
```

## Integration Test Setup Patterns

### Database Initialization
```csharp
protected async Task SeedDatabaseAsync()
{
    var category = new Category { Id = Guid.NewGuid(), Name = "Test" };
    await _context.Categories.AddAsync(category);
    await _context.SaveChangesAsync();
}

protected async Task ClearDatabaseAsync()
{
    _context.Accounts.RemoveRange(_context.Accounts);
    _context.Categories.RemoveRange(_context.Categories);
    await _context.SaveChangesAsync();
}
```

### Using IAsyncLifetime for Setup/Teardown
```csharp
public class RepositoryTests : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Runs before each test
        await SeedDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        // Runs after each test
        await ClearDatabaseAsync();
    }
}
```

## Coverage Guidelines

### Target Coverage
- **Overall**: 80%+ code coverage
- **Critical paths**: 100% (repositories, business logic)
- **Edge cases**: Cover null, empty, boundary values

### Test Categories (Use Traits)
```csharp
[Trait("Category", "Unit")]      // Fast, isolated, no I/O
[Trait("Category", "Integration")] // Database/API interactions
```

### Run Specific Tests
```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --collect:"XPlat Code Coverage"
```

## Lessons Learned

### ✅ Do This
- Use descriptive test names that explain scenario and outcome
- Test both happy paths and error conditions
- Use async/await consistently with `Task` return types
- Include XML comments on test classes explaining their purpose
- Group related tests in nested classes
- Use constants for repeated values (URLs, test data)

### ❌ Avoid This
- Random test data (use deterministic Guids)
- Testing implementation details (test behavior, not internals)
- Large test methods (split into focused tests)
- Missing cleanup in integration tests (causes test interdependence)
- Ignoring edge cases (null, empty, boundaries)

## Next Steps

As tests are generated and patterns emerge, update this memory with:
- New test fixture patterns discovered
- Effective mocking strategies for specific scenarios
- Reusable test data builders or factories
- Integration test database strategies
- Domain-specific edge cases encountered
- Framework quirks or gotchas
