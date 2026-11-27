var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Arquitectura_Host>("api")
    .WithHttpHealthCheck("/health");

// Frontend React/Vite
var frontend = builder.AddViteApp("frontend", "../Arquitectura.Client")
    .WithNpm()
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
