# Feature Builder Agent

Build features with performance, coding standards, and SOLID principles in mind.

## Naming Conventions
- **Classes/Interfaces**: PascalCase (`ExpenseRepository`, `IExpenseService`)
- **Methods/Properties**: PascalCase (`GetExpenseById`, `TotalAmount`)
- **Private fields**: `_camelCase` (`_dbContext`, `_logger`)
- **Parameters/locals**: camelCase (`expenseId`, `categoryName`)
- **Interfaces**: Prefix with `I`

## Modern C# (C# 12/.NET 10)
- Use nullable reference types explicitly
- Prefer primary constructors for DI
- Use collection expressions `[]`, required properties, pattern matching
- File-scoped namespaces, record types, init-only setters
- Target-typed new expressions

## Architecture Patterns

### Repository Pattern
```csharp
public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken ct = default);
    Task<Expense> AddAsync(Expense expense, CancellationToken ct = default);
}

public class ExpenseRepository(ExpenseContext context) : IExpenseRepository
{
    public async Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.Expenses
            .Include(e => e.Category)
            .Include(e => e.Account)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
}
```

### Service Layer
```csharp
public class ExpenseService(
    IExpenseRepository repository,
    IValidator<CreateExpenseRequest> validator,
    ILogger<ExpenseService> logger) : IExpenseService
{
    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var expense = await repository.AddAsync(MapToEntity(request), ct);
        logger.LogInformation("Expense created: {Id}", expense.Id);
        return MapToDto(expense);
    }
}
```

### Options Pattern
```csharp
public class ImportOptions
{
    public const string SectionName = "Import";
    public required string SourcePath { get; init; }
    public int BatchSize { get; init; } = 100;
}

// Register: builder.Services.Configure<ImportOptions>(config.GetSection(ImportOptions.SectionName));
```

## Entity Framework Core

### Configuration
```csharp
public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.Comment).HasMaxLength(500);
        builder.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.Date);
    }
}

// In DbContext: modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExpenseContext).Assembly);
```

### Query Optimization
```csharp
// ✅ Good: AsNoTracking + Projection
public async Task<IReadOnlyList<ExpenseSummaryDto>> GetSummariesAsync(
    DateTime start, DateTime end, CancellationToken ct)
    => await _context.Expenses.AsNoTracking()
        .Where(e => e.Date >= start && e.Date <= end)
        .Select(e => new ExpenseSummaryDto
        {
            Id = e.Id,
            Amount = e.Amount,
            CategoryName = e.Category.Name
        })
        .ToListAsync(ct);

// ❌ Bad: Over-fetching, tracking
var expenses = await _context.Expenses
    .Include(e => e.Category).Include(e => e.Account)
    .ToListAsync(); // Loads and tracks everything
```

**Key Rules**:
- Use `AsNoTracking()` for read-only queries
- Project to DTOs directly in LINQ
- Add indexes for frequently queried columns
- Avoid N+1 queries with proper Include/eager loading
- Use pagination (Skip/Take) for large datasets

## Performance

### Async/Await
- Always use async/await for I/O (DB, file, network)
- Pass `CancellationToken` to all async methods
- Avoid `async void` (except event handlers)
- Never use `.Result` or `.Wait()` with async code

### Memory & Caching
- Use `using` statements for `IDisposable`/`IAsyncDisposable`
- Stream large collections, don't load all into memory
- Use `Span<T>` for high-performance scenarios
- Cache lookup data (categories, accounts) with `IMemoryCache`

```csharp
public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken ct)
{
    if (_cache.TryGetValue("categories", out IReadOnlyList<CategoryDto>? cached))
        return cached!;

    var categories = await _repository.GetAllAsync(ct);
    var dtos = categories.Select(c => new CategoryDto(c.Id, c.Name)).ToList();
    _cache.Set("categories", dtos, TimeSpan.FromMinutes(30));
    return dtos;
}
```

## Error Handling

### Domain Exceptions
```csharp
public class EntityNotFoundException(string entityName, Guid id)
    : Exception($"{entityName} with ID {id} not found");

public class ExpenseValidationException(string message) : Exception(message);
```

### Service Error Handling
```csharp
public async Task<ExpenseDto> GetAsync(Guid id, CancellationToken ct)
{
    var expense = await _repository.GetByIdAsync(id, ct);
    if (expense is null)
    {
        _logger.LogWarning("Expense {Id} not found", id);
        throw new EntityNotFoundException(nameof(Expense), id);
    }
    return MapToDto(expense);
}
```

### Validation
```csharp
public class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Date).LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Comment).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Comment));
    }
}
```

## Logging

Use structured logging with named parameters:
```csharp
_logger.LogInformation("Creating expense: Amount={Amount}, Category={CategoryId}",
    request.Amount, request.CategoryId);

_logger.LogError(ex, "Failed to create expense: {CategoryId}", request.CategoryId);
```

**Levels**: Trace (debugging) → Debug (diagnostic) → Information (flow) → Warning (unexpected) → Error (failures) → Critical (catastrophic)

## Dependency Injection

```csharp
public static IServiceCollection AddDomainServices(this IServiceCollection services)
{
    services.AddDbContext<ExpenseContext>((provider, options) =>
        options.UseSqlServer(
            provider.GetRequiredService<IConfiguration>().GetConnectionString("Default"),
            sql => sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

    services.AddScoped<IExpenseRepository, ExpenseRepository>();
    services.AddScoped<IExpenseService, ExpenseService>();
    services.AddScoped<IValidator<CreateExpenseRequest>, CreateExpenseRequestValidator>();
    return services;
}
```

**Lifetimes**:
- **Transient**: Stateless, lightweight services
- **Scoped**: DbContext, repositories (per request)
- **Singleton**: Configuration, caching services

## Code Review Checklist
- [ ] SOLID principles (especially Single Responsibility)
- [ ] Async/await + CancellationToken throughout
- [ ] Error handling and structured logging
- [ ] Input validation (FluentValidation/DataAnnotations)
- [ ] EF optimizations (AsNoTracking, projections, indexes)
- [ ] Caching for lookup data
- [ ] Nullable reference types handled
- [ ] Testable via DI with interfaces
- [ ] Modern C# features used appropriately
