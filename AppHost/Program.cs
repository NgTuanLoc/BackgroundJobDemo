var builder = DistributedApplication.CreateBuilder(args);

// External Services
var db = builder.AddContainer("db", "citusdata/citus", "12.1")
    .WithEnvironment("POSTGRES_PASSWORD", "P@ssw0rd@123AA!@#")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEndpoint(port: 15432, targetPort: 5432)
    .WithVolume("VolumeMount.postgres.data", "/var/lib/postgresql/data")
    .PublishAsContainer();

var redis = builder.AddRedis("redis")
    .PublishAsContainer()
    .WithRedisCommander();

// Services
builder.AddProject<Projects.BackgroundJobDemo>("background-job")
    .WithEnvironment("IsAspireRunning", "true")
    .WithReference(redis)
    .WithReference(db.GetEndpoint("db"));

builder.Build().Run();
