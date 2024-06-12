using BackgroundJobDemo.Infrastructure.Models;
using BackgroundJobDemo.Infrastructure.Repositories;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Configuration;

namespace BackgroundJobDemo.Infrastructure.Extensions;

public static class ExternalExtensions
{
    public static IServiceCollection ConfigureExternalExtensions(this IServiceCollection service, IConfiguration configuration)
    {
        service.AddSingleton(TimeProvider.System);

        service.AddScoped<IDistributedLock>(x =>
        {
            var redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("redis") ?? "");
            return new RedisDistributedLock($"TimedHostedServiceLock", redis.GetDatabase());
        });

        service.Configure<AppSettings>(configuration);

        service.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        service.AddScoped<IUserRepository, UserRepository>();

        return service;
    }
}
