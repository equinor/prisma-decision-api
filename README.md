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

## Docker Compose

Run both APIs together with Docker Compose from the repository root.

### Prerequisites

- Docker Desktop (or Docker Engine + Compose)
- `AZURE_AD_CLIENT_SECRET` set in your shell

```bash
export AZURE_AD_CLIENT_SECRET="your-secret"
```

### Start Services

```bash
docker compose up --build
```

### Stop Services

```bash
docker compose down
```


### Rebuild from scratch (clear cache)

```bash

docker compose build --no-cache
docker compose up
```


### Reset SQLite Data

The API stores SQLite at `/data/Prisma.db` in the `api-data` named volume.

Remove containers and volume (full DB reset):

```bash
docker compose down -v
```

### Notes

- The `api` service runs as root in Docker Compose for local development to avoid SQLite write-permission issues on the named volume.
- Normal `docker compose down` keeps your SQLite data. Use `-v` only when you want to reset it.

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
