#!/usr/bin/env bash
# Usage: 
# ./scripts/Update-SqlServer-Database.sh --connection "..." --migration "..." --environment "..."

set -euo pipefail

project="./SqlServerMigrations/SqlServerMigrations.csproj"
startup="./PrismaApi.Api/PrismaApi.Api.csproj"
connection=""
migration=""
environment="${ASPNETCORE_ENVIRONMENT:-Local}"

while [[ $# -gt 0 ]]; do
    [[ "$1" == "--connection" ]] && connection="$2" && shift 2 && continue
    [[ "$1" == "--migration" ]] && migration="$2" && shift 2 && continue
    [[ "$1" == "--environment" ]] && environment="$2" && shift 2 && continue
    shift
done

cmd=(dotnet ef database update)
[[ -n "$migration" ]] && cmd+=("$migration")
cmd+=(--project "$project" --startup-project "$startup")
[[ -n "$connection" ]] && cmd+=(--connection "$connection")
cmd+=(-- --provider=sqlserver)

ASPNETCORE_ENVIRONMENT="$environment" "${cmd[@]}"
