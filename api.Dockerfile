# Create build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /App

# Do not send telemetry to MS
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copy project file first (layer cache)
COPY ["PrismaApi/PrismaApi.Api/PrismaApi.Api.csproj", "PrismaApi/PrismaApi.Api/"]

# Copy full source, then publish
COPY . .

RUN dotnet restore "PrismaApi/PrismaApi.Api/PrismaApi.Api.csproj"

RUN dotnet publish "PrismaApi/PrismaApi.Api/PrismaApi.Api.csproj" --no-restore -c Release -o /App/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /App
COPY --from=build /App/out .

# Add globalization support
RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Run as non-root
RUN addgroup -S -g 2001 ep-sb-non-root-group && \
    adduser -S -u 2001 -G ep-sb-non-root-group ep-sb-non-root-user
USER 2001

ENV ASPNETCORE_URLS="http://+:7075"
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 7075
ENTRYPOINT ["dotnet", "PrismaApi.Api.dll"]
