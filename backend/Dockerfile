# ---- Restore & build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first (and the .sln) so Docker can cache the restore layer as long as no
# project file changed, even if application source code changes on every build.
COPY AssignmentHub.sln .
COPY src/AssignmentHub.Domain/AssignmentHub.Domain.csproj src/AssignmentHub.Domain/
COPY src/AssignmentHub.Application/AssignmentHub.Application.csproj src/AssignmentHub.Application/
COPY src/AssignmentHub.Infrastructure/AssignmentHub.Infrastructure.csproj src/AssignmentHub.Infrastructure/
COPY src/AssignmentHub.Api/AssignmentHub.Api.csproj src/AssignmentHub.Api/
COPY tests/AssignmentHub.UnitTests/AssignmentHub.UnitTests.csproj tests/AssignmentHub.UnitTests/
COPY tests/AssignmentHub.IntegrationTests/AssignmentHub.IntegrationTests.csproj tests/AssignmentHub.IntegrationTests/
COPY tests/AssignmentHub.ArchitectureTests/AssignmentHub.ArchitectureTests.csproj tests/AssignmentHub.ArchitectureTests/
RUN dotnet restore AssignmentHub.sln

COPY . .
RUN dotnet publish src/AssignmentHub.Api/AssignmentHub.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# curl is needed for the container HEALTHCHECK to hit the app's own /health endpoint.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=10 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AssignmentHub.Api.dll"]
