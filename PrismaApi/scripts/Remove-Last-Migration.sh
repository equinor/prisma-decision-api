#!/usr/bin/env bash
# Run from the PrismaApi directory: ./scripts/Remove-Last-Migration.sh
# Example: 
# ./scripts/Remove-Last-Migration.sh

set -euo pipefail

startupProject="./PrismaApi.Api/PrismaApi.Api.csproj"
sqliteProject="./SqliteMigrations/SqliteMigrations.csproj"
sqlServerProject="./SqlServerMigrations/SqlServerMigrations.csproj"

dotnet ef migrations remove \
    --project "$sqliteProject" \
    --startup-project "$startupProject" \
    -- --provider=sqlite

dotnet ef migrations remove \
    --project "$sqlServerProject" \
    --startup-project "$startupProject" \
    -- --provider=sqlserver
