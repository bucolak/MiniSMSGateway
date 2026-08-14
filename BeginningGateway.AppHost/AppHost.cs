using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").AddDatabase("my-db");

builder.AddProject<Sms>("sms").WithReference(postgres);

builder.Build().Run();
