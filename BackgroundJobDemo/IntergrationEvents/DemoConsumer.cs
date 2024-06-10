using BackgroundJobDemo.Infrastructure;
using BackgroundJobDemo.Infrastructure.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BackgroundJobDemo.IntergrationEvents;

public class DemoConsumer(AppDbContext dbContext) : IConsumer<TestModel>
{
    private readonly AppDbContext _dbContext = dbContext;
    public async Task Consume(ConsumeContext<TestModel> context)
    {
        await UpdateProductAsync(context.Message.Id.ToString());
    }

    private async Task UpdateProductAsync(string second)
    {
        //using var scope = _scopeFactory.CreateScope();
        //var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var product = await dbContext.Products.FirstOrDefaultAsync();
        if (product != null)
        {
            product.Price++;
            product.Name = $"{product.Name}==={second}";
            await dbContext.SaveChangesAsync();
        }
    }
}
