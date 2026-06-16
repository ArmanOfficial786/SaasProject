# Entity Framework Migrations

This directory contains Entity Framework Core migrations for the SaaS multi-tenant application.

## Migration Structure

### HRM Product
- **Location**: `HrmMigration/`
- **DbContext**: `HrmDbContext`
- **Database**: `saas_hrm_db`
- **Connection String**: Configured in `Src/Product/Hrm.Api/appsettings.json` as `HrmConnection`

### School Product
- **Location**: `SchoolMigration/`
- **DbContext**: `SchoolDbContext`
- **Database**: `saas_school_db`
- **Connection String**: Configured in `Src/Product/School.Api/appsettings.json` as `SchoolConnection`

## Creating New Migrations

### For HRM DbContext:
```bash
dotnet ef migrations add <MigrationName> --project Src/Shared/Shared.Infrastructure --context HrmDbContext --output-dir Migration/HrmMigration
```

### For School DbContext:
```bash
dotnet ef migrations add <MigrationName> --project Src/Shared/Shared.Infrastructure --context SchoolDbContext --output-dir Migration/SchoolMigration
```

## Updating Database

### For HRM Database:
```bash
dotnet ef database update --project Src/Shared/Shared.Infrastructure --context HrmDbContext
```

### For School Database:
```bash
dotnet ef database update --project Src/Shared/Shared.Infrastructure --context SchoolDbContext
```

## Architecture Notes

- Each product has its own isolated database with separate DbContext
- The `Shared.Infrastructure` project contains all DbContext implementations and migrations
- Shared entities and configurations are in `Shared.Domain` and `Shared.Application`
- Product-specific entities should be defined in their respective Domain projects
- Entity configurations should be added to the product's DbContext `OnModelCreating` method

## Next Steps

1. Define your domain entities for each product (e.g., Employee, Department for HRM; Student, Class for School)
2. Add the entities to the respective DbContext
3. Configure entity relationships and constraints in `OnModelCreating`
4. Run the migration commands above to generate and apply migrations
