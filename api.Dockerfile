# Create build environment
# Find the latest here https://hub.docker.com/_/microsoft-dotnet-sdk
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
WORKDIR /App

COPY ["PrismaApi/PrismaApi.Api/PrismaApi.Api.csproj", "PrismaApi/"]

WORKDIR "/PrismaApi/"
# Do not send telemetry to MS
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Restore required packages, and do a build and publish of solution
RUN dotnet restore
RUN dotnet publish "PrismaApi.Api/PrismaApi.Api.csproj" --no-restore -c release -o out

# Build runtime image
# Find the latest here https://hub.docker.com/_/microsoft-dotnet-aspnet
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /App
COPY --from=build-env /App/out .

# Add globalization support
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Required for Time Zone database lookups
RUN apk add --no-cache tzdata

RUN apk upgrade --available

# https://public-site-radix-platform-qa.radix.equinor.com/docs/topic-docker/#running-as-non-root
# add new non-root group and add a new user to it
RUN addgroup -S -g 2001 ep-sb-non-root-group
RUN adduser -S -u 2001 -G ep-sb-non-root-group ep-sb-non-root-user
USER 2001

# expose port 5000 as our application will be listening on this port
ENV ASPNETCORE_URLS="http://+:7075"
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 7075
ENTRYPOINT ["dotnet", "PrismaApi.Api.dll"]