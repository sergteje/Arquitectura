# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src

# Copy the whole tree so restore can see every referenced project.
COPY . .

# Restore and publish the host (the single entry point).
RUN dotnet restore "Arquitectura.Host/Arquitectura.Host.csproj"
RUN dotnet publish "Arquitectura.Host/Arquitectura.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS final
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

# Health check (the /health endpoint is mapped in Development, which docker-compose sets)
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Entry point
ENTRYPOINT ["dotnet", "Arquitectura.Host.dll"]
