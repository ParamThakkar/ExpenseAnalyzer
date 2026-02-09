# CLAUDE.md

Concise project guidance for Claude Code. Specialized instructions: `.claude/agents/feature-builder.md` (dev), `.claude/agents/test-generator.md` (testing).

## Project Overview

ExpenseAnalyzer: .NET 10 expense tracking with import/analysis capabilities.

### Structure

```
src/
├── Domain/         # Entities (Expense, Category, Account, Tag), ExpenseContext, Migrations
├── Infra/          # IRepository<T>, Repository<T>, specific repos (IAccountRepository)
├── Api/            # Minimal API, versioned endpoints, Swagger
└── Import/         # Worker service for data ingestion
tests/
├── UnitTests/           # xUnit unit tests
└── IntegrationTests/    # xUnit integration tests
```

### Stack

- **.NET 10**: Target framework
- **EF Core 10**: SQL Server, lazy loading proxies, retry on failure
- **xUnit**: Test framework (coverlet for coverage)
- **Minimal APIs**: Asp.Versioning (URL/header/query), Swashbuckle (OpenAPI)
- **Central Package Management**: `Directory.Packages.props`

## Quick Commands

```bash
# Build & Run
dotnet build
dotnet run --project src/ExpenseAnalyzer.Api        # API (https://localhost:5001/swagger)
dotnet run --project src/ExpenseAnalyzer.Import     # Import worker

# Test
dotnet test                                         # All
dotnet test --filter "Category=Unit"                # Unit only
dotnet test --collect:"XPlat Code Coverage"         # With coverage

# EF Migrations
dotnet ef migrations add <Name> --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Api
dotnet ef database update --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Api
```

## Architecture

### Layers
- **Domain**: Entities (Guid PKs), ExpenseContext, migrations. No external deps.
- **Infra**: Generic `IRepository<T>` + specific repos (e.g., `IAccountRepository`), DI registrar
- **Api**: Minimal API endpoints with versioning (v1, v2), Swagger, endpoint groups
- **Import**: Worker service for data ingestion

### Domain Model
- **Entities**: Expense, Income, Category, Account, Tag, ExpenseTag (Guid PKs)
- **Relationships**:
  - Expense → Category (required), Account (required), Tags (many-to-many via ExpenseTag)
  - Income → Category (required), Account (required)
- **Constraints**: Unique names on Category, Account, Tag
- **Repository**: `IRepository<T>` (GetByIdAsync, GetAllAsync, GetWithFilterAsync, GetQueryable, InsertAsync, Update, Delete, SaveChangesAsync, ExistsAsync, CountAsync)

### API Architecture
- **Pattern**: Minimal APIs with static methods in `<Entity>Endpoints` classes
- **Versioning**: URL segment (`/api/v1/...`), header (`X-Api-Version`), query string (`api-version`)
- **Grouping**: `MapGroup("/api/v{version:apiVersion}/<entity>")` with `WithApiVersionSet`
- **Swagger**: Multi-version docs at `/swagger` in Development mode

### Database
- **Provider**: SQL Server, lazy loading proxies, retry on failure (3 retries, 5s delay)
- **Config**: Connection string in `appsettings.json` or User Secrets (ID: `dotnet-ExpenseAnalyzer.Import-2c565ea4-a907-434e-b896-2e94af2c6c23`)
- **Migrations**: Code-first via EF Core

## Development Guidelines

### Package Management (CPM)
- All versions in `Directory.Packages.props`
- Projects reference WITHOUT versions
- Current packages: EF Core 10.0.2, xUnit 2.9.3, Asp.Versioning 8.1.1, Swashbuckle 7.1.0

### Configuration
- `appsettings.Development.json`: Local settings (not committed)
- User Secrets: Connection strings, API keys (ID: `dotnet-ExpenseAnalyzer.Import-2c565ea4-a907-434e-b896-2e94af2c6c23`)
- Never commit secrets

### Testing
- Unit tests: Fast, isolated, mock dependencies (target 80%+ coverage)
- Integration tests: Real DbContext, database interactions
- Use `[Trait("Category", "Unit|Integration")]` for filtering

### API Endpoint Pattern
```csharp
// In Endpoints/<Entity>Endpoints.cs
public static class AccountEndpoints
{
    public static void Map<Entity>Endpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/accounts")
            .WithTags("Account")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0));

        group.MapGet("/", GetAllAsync);
    }

    static Task<IReadOnlyList<Account>> GetAllAsync(IAccountRepository repo, CancellationToken ct)
        => repo.GetAllAsync(ct);
}
// In Program.cs: app.Map<Entity>Endpoints(apiVersionSet);
```

## Status

**Current**: Domain entities (Expense, Income, Category, Account, Tag), EF migrations, generic repositories, specific repos (Account, Income), API with versioning/Swagger, full CRUD endpoints for Account & Income
**Next**: Complete CRUD for Category/Tag/Expense, import logic, reporting, validation middleware

## Adding New Entities

See `.claude/agents/feature-builder.md` for complete end-to-end entity implementation guide with 8-step checklist and code templates.
