using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume("sms-volume").WithContainerName("my-postgres-db").AddDatabase("my-db");

builder.AddProject<Sms>("sms").WithReference(postgres).WaitFor(postgres);

builder.Build().Run();
