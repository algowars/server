var builder = DistributedApplication.CreateBuilder(args);

// Resources wired in task 03
var api = builder.AddProject<Projects.Algowars_Api>("algowars-api");

builder.Build().Run();
