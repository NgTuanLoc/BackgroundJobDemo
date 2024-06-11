using BackgroundJobDemo.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using static BackgroundJobDemo.Infrastructure.Models.TimeHasPassed;

namespace BackgroundJobDemo.IntegrationEvents;

public class VerificationExpiredStatusUpdateConsumer(AppDbContext dbContext) : IConsumer<MinuteHasPassed>
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task Consume(ConsumeContext<MinuteHasPassed> context)
    {
        var message = context.Message.Now.Second;
        await UpdateProductAsync(message.ToString());
    }

    private async Task UpdateProductAsync(string second)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync();
        if (product != null)
        {
            product.Price++;
            product.Name = $"{product.Name}==={second}";
            await _dbContext.SaveChangesAsync();
        }
    }
}