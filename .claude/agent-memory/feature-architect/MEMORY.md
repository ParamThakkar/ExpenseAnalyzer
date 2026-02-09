# Feature Architect Agent Memory

## End-to-End Entity Implementation (8 Steps)

**Time Investment**: ~10-15 minutes for full entity with CRUD endpoints

### Checklist
1. ✅ Create entity in `Domain/Entities/<Entity>.cs` (Guid PK, virtual nav props for lazy loading)
2. ✅ Update `Domain/ExpenseContext.cs` (add DbSet, configure in OnModelCreating: PK, decimal precision, FKs, indexes)
3. ✅ Generate migration: `dotnet ef migrations add Add<Entity>Entity --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Api`
4. ✅ Apply migration: `dotnet ef database update --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Api`
5. ✅ Create `Infra/Repos/I<Entity>Repository.cs` (inherit IRepository<Entity>, add domain methods)
6. ✅ Create `Infra/Repos/<Entity>Repository.cs` (inherit Repository<Entity>, implement domain methods)
7. ✅ Register in `Infra/DependencyRegistrar.cs`: `services.AddScoped<I<Entity>Repository, <Entity>Repository>();`
8. ✅ Create `Api/Endpoints/<Entity>Endpoints.cs` (Map<Entity>Endpoints method, CRUD handlers)
9. ✅ Register in `Api/Program.cs`: Add using for Endpoints, call `app.Map<Entity>Endpoints(apiVersionSet);`

**CRITICAL**: Remove any duplicate DI registrations in Program.cs - all repos should ONLY be registered in DependencyRegistrar.cs

## Key Patterns & Conventions

### Entity Design
- **Guid PKs**: All entities use `Guid Id { get; set; }`
- **Virtual nav props**: Enable lazy loading (`public virtual Category Category { get; set; } = null!;`)
- **Nullable fields**: Use `string?` for optional fields (e.g., Comment)
- **Temporal fields**: Use `DateTime` for timestamps (not DateTimeOffset for simplicity)
- **XML comments**: Include `<summary>` tags for all public properties and classes

### EF Core Configuration
- **Decimal precision**: Always specify `HasColumnType("decimal(18,2)")` for money
- **FK behavior**: Use `DeleteBehavior.Restrict` to prevent cascading deletes
- **Indexes**: Add on FK columns, date/timestamp fields, and any filter fields
- **Pattern**: Group related config (PK → precision → FKs → indexes)
- **Multiple FKs to same entity**: Use separate navigation properties with explicit HasOne/WithMany configuration

### Repository Domain Methods
- **Filtering**: GetBy<Property>Async (e.g., GetByAccountAsync, GetByCategoryAsync)
- **Date ranges**: GetByDateRangeAsync(startDate, endDate)
- **Aggregation**: GetTotalBy<Property>Async (returns decimal)
- **Ordering**: GetAllOrderedBy<Property>Async (default sort for lists)
- **Combined**: GetBy<Property>AndDateRangeAsync (common query combos)
- **OR conditions**: Use `Where(x => x.Property1 == value || x.Property2 == value)` for inclusive queries

### API Endpoint Validation
```csharp
// Required FK validation
if (entity.CategoryId == Guid.Empty) return Results.BadRequest("CategoryId is required");

// Amount validation
if (entity.Amount <= 0) return Results.BadRequest("Amount must be greater than zero");

// Date validation
if (startDate > endDate) return Results.BadRequest("Start date must be before end date");

// Business logic validation (e.g., Transfer)
if (transfer.OutgoingAccountId == transfer.IncomingAccountId)
    return Results.BadRequest("OutgoingAccountId and IncomingAccountId must be different");
```

### Endpoint Response Patterns
- **GET single**: `NotFound()` if null, else `Ok(entity)`
- **GET collection**: Always `Ok(collection)` (empty array if none)
- **POST**: `Created($"/api/v1/<entity>/{id}", entity)` with new Guid
- **PUT**: `NotFound()` if not exists, `NoContent()` on success
- **DELETE**: `NotFound()` if not exists, `NoContent()` on success

## Project Architecture
- **Pattern**: Minimal APIs with versioned endpoints (Asp.Versioning 8.1.1)
- **Layers**: Domain (entities + DbContext) → Infra (repositories + DI) → Api (endpoints)
- **Repository**: Generic `IRepository<T>` + specific repos with domain methods
- **DI Registration**: Centralized in `Infra/DependencyRegistrar.cs` via `AddInfra()` extension

## Completed Implementations
- **Account**: Simple entity with name (no domain methods yet)
- **Income**: Full implementation with filtering (account/category/date), aggregation (totals), ordering
- **Transfer**: Full implementation with dual account FKs, business validation, OR queries for account filtering

## Complex Entity Patterns

### Multiple FKs to Same Entity (e.g., Transfer with OutgoingAccount and IncomingAccount)
```csharp
// Entity
public Guid OutgoingAccountId { get; set; }
public virtual Account OutgoingAccount { get; set; } = null!;

public Guid IncomingAccountId { get; set; }
public virtual Account IncomingAccount { get; set; } = null!;

// EF Configuration
modelBuilder.Entity<Transfer>()
    .HasOne(t => t.OutgoingAccount)
    .WithMany()
    .HasForeignKey(t => t.OutgoingAccountId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<Transfer>()
    .HasOne(t => t.IncomingAccount)
    .WithMany()
    .HasForeignKey(t => t.IncomingAccountId)
    .OnDelete(DeleteBehavior.Restrict);

// Repository OR Query
return await _context.Transfer
    .Where(t => t.OutgoingAccountId == accountId || t.IncomingAccountId == accountId)
    .OrderByDescending(t => t.Timestamp)
    .ToListAsync(cancellationToken);
```

### Business Validation in Endpoints
- Validate all required fields (non-empty Guids)
- Validate business rules (e.g., accounts must be different)
- Validate data constraints (amount > 0, date ranges)
- Return specific error messages for each validation failure

## Common Gotchas
1. **Duplicate DI registrations**: Program.cs had duplicate repo registrations - removed in Income implementation
2. **Decimal precision**: Must explicitly specify `decimal(18,2)` in OnModelCreating
3. **Endpoint registration**: Must add `using ExpenseAnalyzer.Api.Endpoints;` in Program.cs
4. **Virtual properties**: Required for lazy loading proxies to work
5. **Multiple FKs**: Explicit navigation properties required when entity has multiple FKs to same entity
6. **Business validation**: Implement domain-specific validations in endpoints (not just data type checks)

## Documentation Strategy
- **CLAUDE.md**: Quick reference (architecture, commands, status) - keep under 130 lines
- **feature-architect.md**: Complete implementation guide with SOLID principles and 8-step checklist
- **MEMORY.md** (this file): Checklist, patterns, gotchas - lessons learned from implementations
- **test-generator.md**: Testing patterns for unit/integration tests