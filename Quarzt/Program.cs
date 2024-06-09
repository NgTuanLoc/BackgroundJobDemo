using Microsoft.EntityFrameworkCore;
using Quarzt.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();



app.MapGet("/", async (AppDbContext context, CancellationToken cancellationToken) =>
{
    return Results.Ok(await context.Products.ToListAsync(cancellationToken));
});

app.Run();
