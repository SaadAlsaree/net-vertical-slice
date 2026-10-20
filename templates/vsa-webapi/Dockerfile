# syntax=docker/dockerfile:1.7

# Runtime base. Visual Studio Fast mode stops here and runs the app it built on the host.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_ENVIRONMENT=Production
WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["VsaTemplate.csproj", "."]
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "VsaTemplate.csproj" \
        -p:IncludeWolverineRuntimeCompilation=true

COPY . .

RUN --mount=type=cache,target=/root/.nuget/packages \
    ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_ENVIRONMENT=Development \
    dotnet run \
        --project "VsaTemplate.csproj" \
        -c "$BUILD_CONFIGURATION" \
        -p:IncludeWolverineRuntimeCompilation=true \
        --no-restore \
        -- codegen write

RUN --mount=type=cache,target=/root/.nuget/packages \
    if [ "$APP_ENVIRONMENT" = "Development" ]; then \
        INCLUDE_RUNTIME_COMPILATION=true; \
    else \
        INCLUDE_RUNTIME_COMPILATION=false; \
    fi; \
    dotnet publish "VsaTemplate.csproj" \
        -c "$BUILD_CONFIGURATION" \
        -o /app/publish \
        -p:IncludeWolverineRuntimeCompilation="$INCLUDE_RUNTIME_COMPILATION" \
        -p:IncludeEfDesignTimePackages=false \
        /p:UseAppHost=false

RUN mkdir -p /app/logs

FROM base AS final
ARG APP_UID=1654
ARG APP_ENVIRONMENT=Production

RUN apt-get update \
    && apt-get install --no-install-recommends -y curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_ENVIRONMENT=${APP_ENVIRONMENT} \
    DOTNET_ENVIRONMENT=${APP_ENVIRONMENT} \
    DOTNET_gcServer=1

COPY --from=build /app/publish .
COPY --from=build --chown=${APP_UID}:${APP_UID} /app/logs ./Logs
#if (DB_SQLITE)

# Mount point for the SQLite database volume, writable by the non-root app user.
RUN mkdir -p /app/data && chown ${APP_UID}:${APP_UID} /app/data
#endif

USER ${APP_UID}

HEALTHCHECK --interval=15s --timeout=3s --start-period=30s --retries=5 \
    CMD curl --fail --silent --show-error http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "VsaTemplate.dll"]
