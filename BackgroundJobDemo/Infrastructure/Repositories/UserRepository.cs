using Microsoft.EntityFrameworkCore;

namespace BackgroundJobDemo.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task VerificationExpiredStatusUpdateAsync()
    {
        throw new NotImplementedException();
    }

    public async Task UpdateProductAsync(string second)
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
