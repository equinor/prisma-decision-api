#!/usr/bin/env bash
# Run from the PrismaApi directory: ./scripts/Add-Migrations.sh <MigrationName>
# Example: 
# ./scripts/Add-Migrations.sh MigrationName

set -euo pipefail

if [[ $# -lt 1 || -z "${1}" ]]; then
    echo "Usage: $(basename "$0") <MigrationName>"
    exit 1
fi

name="$1"
startupProject="./PrismaApi.Api/PrismaApi.Api.csproj"
sqliteProject="./SqliteMigrations/SqliteMigrations.csproj"
sqlServerProject="./SqlServerMigrations/SqlServerMigrations.csproj"

ASPNETCORE_ENVIRONMENT="Local" dotnet ef migrations add "$name" \
    --project "$sqliteProject" \
    --startup-project "$startupProject" \
    -- --provider=sqlite

ASPNETCORE_ENVIRONMENT="Local" dotnet ef migrations add "$name" \
    --project "$sqlServerProject" \
    --startup-project "$startupProject" \
    -- --provider=sqlserver
