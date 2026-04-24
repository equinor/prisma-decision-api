#!/usr/bin/env bash
# Usage: 
# ./scripts/Update-Sqlite-Database.sh --connection "..." --migration "..." --environment "..."

set -euo pipefail

project="./SqliteMigrations/SqliteMigrations.csproj"
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

echo "Current database migrations:"
ASPNETCORE_ENVIRONMENT="$environment" dotnet ef migrations list --project "$project" --startup-project "$startup" -- --provider=sqlite
echo ""

cmd=(dotnet ef database update)
[[ -n "$migration" ]] && cmd+=("$migration")
cmd+=(--project "$project" --startup-project "$startup")
[[ -n "$connection" ]] && cmd+=(--connection "$connection")
cmd+=(-- --provider=sqlite)

ASPNETCORE_ENVIRONMENT="$environment" "${cmd[@]}"
