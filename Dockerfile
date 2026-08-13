# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy project files
COPY CaixaSystem.Domain/*.csproj ./CaixaSystem.Domain/
COPY CaixaSystem.Application/*.csproj ./CaixaSystem.Application/
COPY CaixaSystem.Infrastructure/*.csproj ./CaixaSystem.Infrastructure/
COPY CaixaSystem.API/*.csproj ./CaixaSystem.API/
COPY CaixaSystem.Tests/*.csproj ./CaixaSystem.Tests/
COPY CaixaSystem.sln .

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY CaixaSystem.Domain/ ./CaixaSystem.Domain/
COPY CaixaSystem.Application/ ./CaixaSystem.Application/
COPY CaixaSystem.Infrastructure/ ./CaixaSystem.Infrastructure/
COPY CaixaSystem.API/ ./CaixaSystem.API/
COPY CaixaSystem.Tests/ ./CaixaSystem.Tests/

# Build
RUN dotnet build -c Release --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish CaixaSystem.API/CaixaSystem.API.csproj -c Release --no-restore -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_HTTP_PORTS=5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "CaixaSystem.API.dll"]
