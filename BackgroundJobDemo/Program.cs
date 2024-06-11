using BackgroundJobDemo.Infrastructure;
using BackgroundJobDemo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureExternalExtensions(builder.Configuration);
builder.Services.ConfigureBackgroundJobExtensions(builder.Configuration);

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