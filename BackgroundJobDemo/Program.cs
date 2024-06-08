using BackgroundJobDemo.Infrastructure;
using demo_background_job.Job;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Jobs
builder.Services.AddHostedService<TimeTriggerJob>();

var app = builder.Build();

app.MapGet("/", () => async (AppDbContext context) =>
{
    return Results.Ok(await context.Products.ToListAsync());
});

app.Run();
