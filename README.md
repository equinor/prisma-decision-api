# Prisma Decisions API

Two APIs working together:
- **.NET API** — handles the database and delegates to the Python API
- **Python (FastAPI)** — interfaces with Python packages such as [pyagrum](https://pyagrum.readthedocs.io/)

## Quick start

### Python API ([Python 3.12+](https://www.python.org/downloads/))

```bash
pip install poetry
poetry install
uvicorn src.main:app --port 8080
```

### .NET API ([.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))

```bash
cd PrismaApi
dotnet build
dotnet run
```

## Database Migrations

Migrations use Entity Framework Core with separate projects for SQLite and SQL Server ([docs](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects?tabs=dotnet-core-cli)).

**Using scripts** (recommended — syncs both providers at once):

| Script | Purpose |
|---|---|
| `scripts/Add-Migrations.sh` | Add migration to SQLite and SQL Server |
| `scripts/Remove-Last-Migration.sh` | Remove the last migration |
| `scripts/Update-Sqlite-Database.sh` | Apply migrations to SQLite |
| `scripts/Update-SqlServer-Database.sh` | Apply migrations to SQL Server |

Usage instructions are at the top of each script file.

**Manually** (from `PrismaApi/`):

```bash
dotnet ef migrations add YourMigrationName --project ./SqliteMigrations/SqliteMigrations.csproj --startup-project ./PrismaApi.Api/PrismaApi.Api.csproj -- --provider=sqlite
dotnet ef migrations add YourMigrationName --project ./SqlServerMigrations/SqlServerMigrations.csproj --startup-project ./PrismaApi.Api/PrismaApi.Api.csproj -- --provider=sqlserver
```