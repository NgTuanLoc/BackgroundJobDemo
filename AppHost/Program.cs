using Aspire.Hosting;

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

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithAnnotation(new ContainerImageAnnotation { Image = "rabbitmq:3-management" })
    .WithEndpoint(name: "management", scheme: "http", targetPort: 15672)
    .PublishAsContainer();

//var sqlServer = builder.AddContainer("sqlServer", "mcr.microsoft.com/mssql/server", "2019-latest")
//    .WithEnvironment("SA_PASSWORD", "Strong!Passw0rd")
//    .WithEnvironment("ACCEPT_EULA", "Y")
//    .WithEndpoint(port: 1433, targetPort: 1433)
//    .WithVolume("VolumeMount.sqlserver.data", "/var/opt/mssql")
//    .PublishAsContainer();

// Services
builder.AddProject<Projects.BackgroundJobDemo>("background-job")
    .WithEnvironment("IsAspireRunning", "true")
    .WithReplicas(5)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithReference(db.GetEndpoint("db"));

//builder.AddProject<Projects.Quarzt>("quarzt")
//    .WithEnvironment("IsAspireRunning", "true")
//    .WithReference(redis)
//    .WithReference(rabbitmq)
//    .WithReference(sqlServer.GetEndpoint("sqlServer"))
//    .WithReference(db.GetEndpoint("db"));

//builder.AddProject<Projects.QuarztDemo>("quarztdemo")
//    .WithReplicas(5);

// Services
//builder.AddProject<Projects.BackgroundJobDemo>("background-job")
//    .WithEnvironment("IsAspireRunning", "true")
//    .WithReplicas(5)
//    .WithReference(redis)
//    .WithReference(db.GetEndpoint("db"));

//builder.AddProject<Projects.Quarzt>("quarzt")
//    .WithEnvironment("IsAspireRunning", "true")
//    .WithReference(redis)
//    .WithReference(rabbitmq)
//    .WithReference(sqlServer.GetEndpoint("sqlServer"))
//    .WithReference(db.GetEndpoint("db"));

builder.Build().Run();
