using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quarzt.Infrastructure;
using Quarzt.Infrastructure.Consumers;
using Quarzt.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>("sql");

//builder.Services.Configure<RabbitMqTransportOptions>(builder.Configuration.GetSection("RabbitMqTransport"));

var connectionString = builder.Configuration.GetConnectionString("quartz");

builder.Services.AddQuartz(q =>
{
    q.SchedulerName = "MassTransit-Scheduler";
    q.SchedulerId = "AUTO";

    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 10;
    });

    q.UseTimeZoneConverter();

    q.UsePersistentStore(s =>
    {
        s.UseProperties = true;
        s.RetryInterval = TimeSpan.FromSeconds(15);

        s.UseSqlServer(connectionString ?? "");

        s.UseJsonSerializer();

        s.UseClustering(c =>
        {
            c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
            c.CheckinInterval = TimeSpan.FromSeconds(10);
        });
    });
});

builder.Services.AddMassTransit(x =>
{
    x.AddPublishMessageScheduler();

    x.AddQuartzConsumers(options =>
    {
        options.PrefetchCount = 1;
        options.QueueName = "quartz";
    });

    x.AddConsumer<SampleConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UsePublishMessageScheduler();

        //cfg.Host(new Uri("amqp://localhost"));
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.Configure<MassTransitHostOptions>(options =>
{
    options.WaitUntilStarted = true;
});

builder.Services.AddQuartzHostedService(options =>
{
    options.StartDelay = TimeSpan.FromSeconds(5);
    options.WaitForJobsToComplete = true;
});

builder.Services.AddHostedService<SampleJob>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/", async (AppDbContext context, CancellationToken cancellationToken) =>
{
    return Results.Ok(await context.Products.ToListAsync(cancellationToken));
});

app.Run();
