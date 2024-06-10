using BackgroundJobDemo.Infrastructure;
using BackgroundJobDemo.IntergrationEvents;
using demo_background_job.Job;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<DemoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });

        cfg.Host(new Uri(builder.Configuration.GetConnectionString("rabbitmq") ?? ""));

        cfg.ConfigureEndpoints(context);
    });
});


// Jobs
//builder.Services.AddScoped<IHostedService, TimeTriggerJob>();
builder.Services.AddHostedService<TimeTriggerJob>();

var app = builder.Build();

app.MapGet("/", async (AppDbContext context) =>
{
    return Results.Ok(await context.Products.ToListAsync());
});

app.MapGet("/reset", async (AppDbContext context) =>
{
    var products = await context.Products.FirstOrDefaultAsync();
    if (products != null)
    {
        products.Price = 0;
        products.Name = "Product";
        await context.SaveChangesAsync();
    }
    return Results.Ok("Result Succeed");
});

app.MapGet("/add-product", async (AppDbContext context) =>
{
    var products = await context.Products.FirstOrDefaultAsync();
    if (products is null)
    {
        context.Products.Add(new BackgroundJobDemo.Entities.Product()
        {
            Id = 0,
            Name = "Product",
            Price = 0,
        });
        await context.SaveChangesAsync();
    }
    return Results.Ok("Add Product Succeed");
});

app.Run();
