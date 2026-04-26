# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Arquitectura.slnx", "./"]
COPY ["Arquitectura.Api/Arquitectura.Api.csproj", "Arquitectura.Api/"]
COPY ["Arquitectura.Domain/Arquitectura.Domain.csproj", "Arquitectura.Domain/"]
COPY ["Arquitectura.Application/Arquitectura.Application.csproj", "Arquitectura.Application/"]
COPY ["Arquitectura.Infrastructure/Arquitectura.Infrastructure.csproj", "Arquitectura.Infrastructure/"]
COPY ["Arquitectura.Persistence/Arquitectura.Persistence.csproj", "Arquitectura.Persistence/"]
COPY ["Arquitectura.ServiceDefaults/Arquitectura.ServiceDefaults.csproj", "Arquitectura.ServiceDefaults/"]
COPY ["Arquitectura.Tests/Arquitectura.Tests.csproj", "Arquitectura.Tests/"]

# Restore dependencies
RUN dotnet restore "Arquitectura.Api/Arquitectura.Api.csproj"

# Copy all source code
COPY . .

# Build
WORKDIR "/src/Arquitectura.Api"
RUN dotnet build "Arquitectura.Api.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "Arquitectura.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r arquitectura && useradd -r -g arquitectura arquitectura

# Copy published files
COPY --from=build /app/publish .

# Set ownership
RUN chown -R arquitectura:arquitectura /app

# Switch to non-root user
USER arquitectura

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Entry point
ENTRYPOINT ["dotnet", "Arquitectura.Api.dll"]