namespace BackgroundJobDemo.Infrastructure.Repositories;

public interface IUserRepository
{
    Task VerificationExpiredStatusUpdateAsync();
    Task UpdateProductAsync(string second);
}
