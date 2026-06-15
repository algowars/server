var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("algowars-postgres")
    .WithDataVolume()
    .AddDatabase("algowars-db");

var rabbitmq = builder.AddRabbitMQ("algowars-mq")
    .WithDataVolume()
    .WithManagementPlugin();

builder.AddProject<Projects.Algowars_Api>("algowars-api")
    .WithReference(postgres)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

builder.Build().Run();
