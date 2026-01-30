# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ExpenseAnalyzer is a .NET 10.0 solution for tracking and analyzing expenses. The solution consists of two main projects:

- **ExpenseAnalyzer.Domain**: Contains the domain model, entities, and Entity Framework Core DbContext
- **ExpenseAnalyzer.Import**: A worker service (background service) for importing expense data

## Build and Run Commands

### Build the solution
```bash
dotnet build
```

### Run the Import worker service
```bash
dotnet run --project src/ExpenseAnalyzer.Import/ExpenseAnalyzer.Import.csproj
```

### Restore packages
```bash
dotnet restore
```

## Architecture

### Domain Layer (ExpenseAnalyzer.Domain)

The domain layer uses Entity Framework Core with SQL Server and defines:

- **ExpenseContext**: Main DbContext with DbSets for Category, Account, and Tag
- **Entity Configuration**: All lookup entities (Category, Account, Tag) have unique name constraints configured via Fluent API
- **Entities**:
  - `Expense`: Core expense entity with CategoryId, AccountId, Amount, and optional Comment
  - `Category`, `Account`, `Tag`: Simple lookup entities with Id (Guid) and Name (string)

### Import Layer (ExpenseAnalyzer.Import)

A .NET Worker Service (BackgroundService) that runs continuously. Currently contains a placeholder Worker implementation that logs periodically.

**User Secrets**: The Import project has User Secrets configured (ID: `dotnet-ExpenseAnalyzer.Import-2c565ea4-a907-434e-b896-2e94af2c6c23`). Use `dotnet user-secrets` commands to manage sensitive configuration.

### Package Management

This solution uses **Central Package Management** (CPM). Package versions are defined in `Directory.Packages.props` at the solution root, not in individual .csproj files. To add or update packages:

1. Add/update the version in `Directory.Packages.props`
2. Reference the package in the project file without specifying a version

Current packages:
- Microsoft.EntityFrameworkCore.SqlServer: 10.0.2
- Microsoft.Extensions.Hosting: 10.0.2

## Database

The solution uses SQL Server via Entity Framework Core. The connection string should be configured in:
- `appsettings.json` or `appsettings.Development.json` in the Import project
- Or via User Secrets for local development

### Entity Framework Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Import

# Update database
dotnet ef database update --project src/ExpenseAnalyzer.Domain --startup-project src/ExpenseAnalyzer.Import
```

Note: EF Core tools require a startup project with hosting configured. Use the Import project as the startup project for migrations.
