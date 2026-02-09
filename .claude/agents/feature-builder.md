# Feature Builder Agent

Build features efficiently with modern C#, SOLID principles, and project-specific patterns.

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

## End-to-End Entity Implementation Guide

Follow this 8-step process to add a new entity with full CRUD operations (~10-15 minutes).

### Step 1: Create Entity (Domain Layer)

**File**: `src/ExpenseAnalyzer.Domain/Entities/<Entity>.cs`

```csharp
namespace ExpenseAnalyzer.Domain;

public class Income  // Replace with your entity name
{
    public Guid Id { get; set; }

    // Foreign Keys (required relationships)
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public Guid AccountId { get; set; }
    public virtual Account Account { get; set; } = null!;

    // Value properties
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Comment { get; set; }  // Nullable for optional fields
}
```

**Key Points**:
- Use `Guid` for PKs
- Use `virtual` for navigation properties (enables lazy loading)
- Use `string?` for optional fields
- Use `DateTime` for temporal fields

---

### Step 2: Update ExpenseContext (Domain Layer)

**File**: `src/ExpenseAnalyzer.Domain/ExpenseContext.cs`

**A. Add DbSet** (after existing DbSets):
```csharp
public DbSet<Income> Income { get; set; }
```

**B. Configure in OnModelCreating** (at end of method, before closing brace):
```csharp
// Income: Primary Key
modelBuilder.Entity<Income>()
    .HasKey(x => x.Id);

// Income: Explicit decimal precision (standard currency format)
modelBuilder.Entity<Income>()
    .Property(i => i.Amount)
    .HasColumnType("decimal(18,2)");

// Income → Category: FK relationship with Restrict delete
modelBuilder.Entity<Income>()
    .HasOne(i => i.Category)
    .WithMany()
    .HasForeignKey(i => i.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);

// Income → Account: FK relationship with Restrict delete
modelBuilder.Entity<Income>()
    .HasOne(i => i.Account)
    .WithMany()
    .HasForeignKey(i => i.AccountId)
    .OnDelete(DeleteBehavior.Restrict);

// Income: Indexes for query performance
modelBuilder.Entity<Income>()
    .HasIndex(i => i.Timestamp);

modelBuilder.Entity<Income>()
    .HasIndex(i => i.AccountId);

modelBuilder.Entity<Income>()
    .HasIndex(i => i.CategoryId);
```

**Configuration Pattern**: PK → Precision → FKs (Restrict) → Indexes

---

### Step 3: Create & Apply Migration

```bash
# Generate migration
dotnet ef migrations add Add<Entity>Entity --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Api

# Review generated migration file, then apply
dotnet ef database update --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Api
```

---

### Step 4: Create Repository Interface (Infra Layer)

**File**: `src/ExpenseAnalyzer.Infra/Repos/I<Entity>Repository.cs`

```csharp
using ExpenseAnalyzer.Domain;

namespace ExpenseAnalyzer.Infra.Repos;

public interface IIncomeRepository : IRepository<Income>
{
    // Filtering methods
    Task<IEnumerable<Income>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Income>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Income>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    // Aggregation methods
    Task<decimal> GetTotalByAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    // Combined queries
    Task<IEnumerable<Income>> GetByAccountAndDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    // Ordering
    Task<IEnumerable<Income>> GetAllOrderedByDateAsync(CancellationToken cancellationToken = default);
}
```

---

### Step 5: Implement Repository (Infra Layer)

**File**: `src/ExpenseAnalyzer.Infra/Repos/<Entity>Repository.cs`

```csharp
using ExpenseAnalyzer.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExpenseAnalyzer.Infra.Repos;

public class IncomeRepository : Repository<Income>, IIncomeRepository
{
    private readonly ExpenseContext _context;

    public IncomeRepository(ExpenseContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Income>> GetByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.AccountId == accountId)
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Income>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.CategoryId == categoryId)
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Income>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.Timestamp >= startDate && i.Timestamp <= endDate)
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalByAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.AccountId == accountId)
            .SumAsync(i => i.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.CategoryId == categoryId)
            .SumAsync(i => i.Amount, cancellationToken);
    }

    public async Task<IEnumerable<Income>> GetByAccountAndDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .Where(i => i.AccountId == accountId && i.Timestamp >= startDate && i.Timestamp <= endDate)
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Income>> GetAllOrderedByDateAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Income
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
```

---

### Step 6: Register Repository (Infra Layer)

**File**: `src/ExpenseAnalyzer.Infra/DependencyRegistrar.cs`

Add after existing repository registrations:
```csharp
services.AddScoped<IIncomeRepository, IncomeRepository>();
```

**CRITICAL**: Never register repositories in Program.cs - only in DependencyRegistrar.cs

---

### Step 7: Create API Endpoints (API Layer)

**File**: `src/ExpenseAnalyzer.Api/Endpoints/<Entity>Endpoints.cs`

```csharp
using Asp.Versioning;
using Asp.Versioning.Builder;
using ExpenseAnalyzer.Domain;
using ExpenseAnalyzer.Infra.Repos;

namespace ExpenseAnalyzer.Api.Endpoints;

public static class IncomeEndpoints
{
    public static void MapIncomeEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/income")
            .WithTags("Income")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0));

        // GET endpoints
        group.MapGet("/", GetAllAsync)
            .WithName("GetAllIncome")
            .Produces<IEnumerable<Income>>(200);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetIncomeById")
            .Produces<Income>(200)
            .Produces(404);

        group.MapGet("/account/{accountId:guid}", GetByAccountAsync)
            .WithName("GetIncomeByAccount")
            .Produces<IEnumerable<Income>>(200);

        group.MapGet("/category/{categoryId:guid}", GetByCategoryAsync)
            .WithName("GetIncomeByCategory")
            .Produces<IEnumerable<Income>>(200);

        group.MapGet("/daterange", GetByDateRangeAsync)
            .WithName("GetIncomeByDateRange")
            .Produces<IEnumerable<Income>>(200);

        group.MapGet("/account/{accountId:guid}/total", GetTotalByAccountAsync)
            .WithName("GetTotalIncomeByAccount")
            .Produces<decimal>(200);

        // POST endpoint
        group.MapPost("/", CreateAsync)
            .WithName("CreateIncome")
            .Produces<Income>(201)
            .Produces(400);

        // PUT endpoint
        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateIncome")
            .Produces(204)
            .Produces(400)
            .Produces(404);

        // DELETE endpoint
        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteIncome")
            .Produces(204)
            .Produces(404);
    }

    static async Task<IResult> GetAllAsync(IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetAllOrderedByDateAsync(ct);
        return Results.Ok(income);
    }

    static async Task<IResult> GetByIdAsync(Guid id, IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetByIdAsync(id, ct);
        return income is null ? Results.NotFound() : Results.Ok(income);
    }

    static async Task<IResult> GetByAccountAsync(Guid accountId, IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetByAccountAsync(accountId, ct);
        return Results.Ok(income);
    }

    static async Task<IResult> GetByCategoryAsync(Guid categoryId, IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetByCategoryAsync(categoryId, ct);
        return Results.Ok(income);
    }

    static async Task<IResult> GetByDateRangeAsync(DateTime startDate, DateTime endDate, IIncomeRepository repo, CancellationToken ct)
    {
        if (startDate > endDate)
            return Results.BadRequest("Start date must be before end date");

        var income = await repo.GetByDateRangeAsync(startDate, endDate, ct);
        return Results.Ok(income);
    }

    static async Task<IResult> GetTotalByAccountAsync(Guid accountId, IIncomeRepository repo, CancellationToken ct)
    {
        var total = await repo.GetTotalByAccountAsync(accountId, ct);
        return Results.Ok(total);
    }

    static async Task<IResult> CreateAsync(Income income, IIncomeRepository repo, CancellationToken ct)
    {
        // Validation
        if (income.CategoryId == Guid.Empty)
            return Results.BadRequest("CategoryId is required");
        if (income.AccountId == Guid.Empty)
            return Results.BadRequest("AccountId is required");
        if (income.Amount <= 0)
            return Results.BadRequest("Amount must be greater than zero");

        // Create
        income.Id = Guid.NewGuid();
        await repo.InsertAsync(income, ct);
        await repo.SaveChangesAsync(ct);

        return Results.Created($"/api/v1/income/{income.Id}", income);
    }

    static async Task<IResult> UpdateAsync(Guid id, Income income, IIncomeRepository repo, CancellationToken ct)
    {
        var existing = await repo.GetByIdAsync(id, ct);
        if (existing is null)
            return Results.NotFound();

        // Validation
        if (income.CategoryId == Guid.Empty)
            return Results.BadRequest("CategoryId is required");
        if (income.AccountId == Guid.Empty)
            return Results.BadRequest("AccountId is required");
        if (income.Amount <= 0)
            return Results.BadRequest("Amount must be greater than zero");

        // Update
        existing.CategoryId = income.CategoryId;
        existing.AccountId = income.AccountId;
        existing.Amount = income.Amount;
        existing.Timestamp = income.Timestamp;
        existing.Comment = income.Comment;

        repo.Update(existing);
        await repo.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    static async Task<IResult> DeleteAsync(Guid id, IIncomeRepository repo, CancellationToken ct)
    {
        var income = await repo.GetByIdAsync(id, ct);
        if (income is null)
            return Results.NotFound();

        repo.Delete(income);
        await repo.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
```

---

### Step 8: Register Endpoints (API Layer)

**File**: `src/ExpenseAnalyzer.Api/Program.cs`

**A. Add using statement** (at top):
```csharp
using ExpenseAnalyzer.Api.Endpoints;
```

**B. Register endpoints** (after existing endpoint registrations, before `app.Run()`):
```csharp
app.MapIncomeEndpoints(apiVersionSet);
```

---

### Verification Checklist

After implementation:
- [ ] `dotnet build` - No compilation errors
- [ ] Migration file created with correct schema
- [ ] `dotnet ef database update` - Migration applied successfully
- [ ] `dotnet run --project src/ExpenseAnalyzer.Api` - API starts
- [ ] Navigate to Swagger UI - New endpoints visible under entity tag
- [ ] Test POST endpoint - Create record
- [ ] Test GET endpoints - Retrieve records
- [ ] Query database - Verify data persisted

---

## Project-Specific Patterns

### Minimal API Response Patterns

**Standard responses**:
- `Results.Ok(data)` - 200 with data
- `Results.Created(uri, data)` - 201 with location header
- `Results.NoContent()` - 204 for successful PUT/DELETE
- `Results.NotFound()` - 404 when entity doesn't exist
- `Results.BadRequest(message)` - 400 with validation error

**Pattern for GET single**:
```csharp
var entity = await repo.GetByIdAsync(id, ct);
return entity is null ? Results.NotFound() : Results.Ok(entity);
```

**Pattern for POST**:
```csharp
entity.Id = Guid.NewGuid();
await repo.InsertAsync(entity, ct);
await repo.SaveChangesAsync(ct);
return Results.Created($"/api/v1/<entity>/{entity.Id}", entity);
```

### Repository Domain Method Patterns

Use specific repositories for domain queries beyond basic CRUD:

**Filtering by property**:
```csharp
Task<IEnumerable<T>> GetBy<Property>Async(Guid id, CancellationToken ct);
```

**Date range queries**:
```csharp
Task<IEnumerable<T>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken ct);
```

**Aggregations**:
```csharp
Task<decimal> GetTotalBy<Property>Async(Guid id, CancellationToken ct);
```

**Ordering defaults**:
```csharp
Task<IEnumerable<T>> GetAllOrderedBy<Property>Async(CancellationToken ct);
```

See Step 4-5 in End-to-End Guide above for complete examples.

## EF Core Configuration Patterns

### Standard Configuration Order
1. **Primary Key**: `HasKey(x => x.Id)`
2. **Precision**: `HasColumnType("decimal(18,2)")` for money
3. **Foreign Keys**: `HasOne().WithMany().HasForeignKey().OnDelete(DeleteBehavior.Restrict)`
4. **Indexes**: On FKs, date fields, filter fields

**Example** (see Step 2 in End-to-End Guide for complete template):
```csharp
modelBuilder.Entity<Income>()
    .HasKey(x => x.Id);

modelBuilder.Entity<Income>()
    .Property(i => i.Amount)
    .HasColumnType("decimal(18,2)");

modelBuilder.Entity<Income>()
    .HasOne(i => i.Category)
    .WithMany()
    .HasForeignKey(i => i.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);
```

### Query Best Practices
✅ `AsNoTracking()` for reads, project to DTOs, indexes, eager loading with `Include`, pagination
❌ Tracking read-only queries, over-fetching, N+1 queries

```csharp
// Optimized read query
await repo.GetQueryable().AsNoTracking()
    .Where(e => e.Date >= start && e.Date <= end)
    .Select(e => new ExpenseSummaryDto(e.Id, e.Amount, e.Category.Name))
    .ToListAsync(ct);
```

## Performance

**Async/Await**: Always for I/O, pass `CancellationToken`, avoid `async void`, never `.Result/.Wait()`
**Memory**: `using` for IDisposable, stream large data, `Span<T>` for high-perf scenarios
**Caching**: Use `IMemoryCache` for lookup data (categories, accounts, tags)

```csharp
static async Task<IResult> GetCategoriesAsync(IMemoryCache cache, IRepository<Category> repo, CancellationToken ct)
{
    if (cache.TryGetValue("categories", out IReadOnlyList<CategoryDto>? cached))
        return Results.Ok(cached);

    var categories = await repo.GetQueryable().AsNoTracking()
        .Select(c => new CategoryDto(c.Id, c.Name))
        .ToListAsync(ct);

    cache.Set("categories", categories, TimeSpan.FromMinutes(30));
    return Results.Ok(categories);
}
```

## Error Handling & Validation

**Minimal API Error Handling**:
```csharp
static async Task<IResult> CreateAsync(CreateExpenseRequest req, IRepository<Expense> repo, CancellationToken ct)
{
    if (req.Amount <= 0) return Results.BadRequest("Amount must be positive");
    if (req.CategoryId == Guid.Empty) return Results.BadRequest("CategoryId required");

    var expense = new Expense { Id = Guid.NewGuid(), Amount = req.Amount, CategoryId = req.CategoryId };
    await repo.InsertAsync(expense, ct);
    await repo.SaveChangesAsync(ct);
    return Results.Created($"/api/v1/expenses/{expense.Id}", expense);
}
```

**Custom Exceptions** (optional for domain logic):
```csharp
public class EntityNotFoundException(string entity, Guid id)
    : Exception($"{entity} {id} not found");
```

## Logging & DI

**Structured Logging**:
```csharp
static async Task<IResult> CreateAsync(CreateExpenseRequest req, IRepository<Expense> repo, ILogger<Program> logger, CancellationToken ct)
{
    logger.LogInformation("Creating expense: Amount={Amount}, Category={CategoryId}", req.Amount, req.CategoryId);
    try { /* logic */ }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed creating expense: {Amount}", req.Amount);
        return Results.Problem("Failed to create expense");
    }
}
```

**DI Registration** (in `Infra/DependencyRegistrar.cs`):
```csharp
public static void AddInfra(this IServiceCollection services, IConfiguration config)
{
    services.AddDbContext<ExpenseContext>(opt =>
        opt.UseSqlServer(config.GetConnectionString("DefaultConnection"))
           .UseLazyLoadingProxies());
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    services.AddScoped<IAccountRepository, AccountRepository>();
}
```

**Lifetimes**: Transient (stateless logic), Scoped (DbContext, repos), Singleton (cache, config)

## Implementation Checklist

**For New Entities**: Follow the 8-step guide at top of this document

**Code Quality**:
- [ ] SOLID principles (Single Responsibility, Interface Segregation)
- [ ] Async/await + `CancellationToken` everywhere
- [ ] Validation: Required FKs, Amount > 0, Date ranges
- [ ] Error handling: `Results.BadRequest/NotFound/Problem` with clear messages
- [ ] Structured logging: named parameters (if adding logging)
- [ ] EF optimizations: `AsNoTracking()` for reads, projections, indexes on filters
- [ ] Nullable reference types handled (`string?` for optional)
- [ ] Modern C# 12: file-scoped namespaces, collection expressions `[]`

**Verification**:
- [ ] `dotnet build` passes
- [ ] Migration created and applied
- [ ] API runs and Swagger shows endpoints
- [ ] Test CRUD operations via Swagger
