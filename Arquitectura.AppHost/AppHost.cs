var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddDotnetProject("api", "../Arquitectura.Host/Arquitectura.Host.csproj")
    .WithHttpHealthCheck("/health");

// Frontend React/Vite
var frontend = builder.AddViteApp("frontend", "../Arquitectura.Client")
    .WithNpm()
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
