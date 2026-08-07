var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume("minismsgateway");

var database = postgres.AddDatabase("SmsDb");

var apiService = builder.AddProject<Projects.MiniSMSGateway_ApiService>("apiservice")
    .WithReference(database)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.MiniSMSGateway_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
