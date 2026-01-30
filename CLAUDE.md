# CLAUDE.md

This file provides high-level guidance to Claude Code (claude.ai/code) when working with code in this repository.

For specialized agent instructions, see:
- **Feature Development**: `.claude/agents/feature-builder.md`
- **Test Generation**: `.claude/agents/test-generator.md`

## Project Overview

ExpenseAnalyzer is a .NET 10.0 solution for tracking and analyzing personal expenses with import capabilities and comprehensive reporting.

### Solution Structure

```
ExpenseAnalyzer/
├── src/
│   ├── ExpenseAnalyzer.Domain/        # Core domain entities and EF Core DbContext
│   └── ExpenseAnalyzer.Import/        # Worker service for data import
└── tests/
    ├── ExpenseAnalyzer.Tests.UnitTests/           # Unit tests (xUnit)
    └── ExpenseAnalyzer.Tests.IntegrationTests/    # Integration tests (xUnit)
```

### Technology Stack

- **.NET 10.0**: Target framework
- **Entity Framework Core 10.0**: ORM with SQL Server provider
- **xUnit**: Testing framework
- **Central Package Management**: Versions defined in `Directory.Packages.props`

## Quick Commands

```bash
# Build
dotnet build

# Run Import Worker
dotnet run --project src/ExpenseAnalyzer.Import/ExpenseAnalyzer.Import.csproj

# Test
dotnet test                                        # All tests
dotnet test --filter "Category=Unit"               # Unit tests only
dotnet test --collect:"XPlat Code Coverage"        # With coverage

# EF Migrations
dotnet ef migrations add <Name> --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Import
dotnet ef database update --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Import
```

## Architecture Principles

### Layered Architecture
- **Domain Layer**: Core business entities, EF Core context, no external dependencies
- **Import Layer**: Background service for data ingestion, depends on Domain
- **Future Layers**: API, Application services (planned)

### Domain Model
- **Entities**: Expense, Category, Account, Tag (all using Guid as primary key)
- **Relationships**: Expense has CategoryId and AccountId (required), Tags (many-to-many, future)
- **Constraints**: Unique names for lookup entities (Category, Account, Tag)

### Database
- **Provider**: SQL Server via EF Core
- **Configuration**: Connection strings in `appsettings.json` or User Secrets (ID: `dotnet-ExpenseAnalyzer.Import-2c565ea4-a907-434e-b896-2e94af2c6c23`)
- **Migrations**: Code-first approach using EF Core migrations

## Development Guidelines

### Package Management
This solution uses **Central Package Management (CPM)**:
- All package versions are defined in `Directory.Packages.props`
- Project files reference packages WITHOUT version numbers
- To add/update packages: modify `Directory.Packages.props` first

### Configuration Management
- Use `appsettings.Development.json` for local settings
- Use User Secrets for sensitive data (connection strings, API keys)
- Never commit secrets to source control

### Testing Strategy
- Unit tests: Fast, isolated, no external dependencies
- Integration tests: Database interactions, EF Core behavior
- Target: 80% code coverage minimum for unit tests

## Project Status

**Current State**: Foundation complete with Domain and Import worker scaffolding
**Next Steps**: Implement import logic, add API layer, build reporting features
