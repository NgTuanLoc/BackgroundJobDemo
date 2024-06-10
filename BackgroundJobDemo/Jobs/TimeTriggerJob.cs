using BackgroundJobDemo.Infrastructure;
using BackgroundJobDemo.Infrastructure.Models;
using MassTransit;
using MassTransit.Transports;
using Medallion.Threading.Redis;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace demo_background_job.Job;

public class TimeTriggerJob(ILogger<TimeTriggerJob> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory) : IHostedService, IDisposable
{
    private readonly ILogger<TimeTriggerJob> _logger = logger;
    private Timer? _timer = null;
    private readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("redis") ?? "");
    private readonly string _lockKey = "TimedHostedServiceLock";
    private readonly TimeSpan _lockExpiry = TimeSpan.FromMinutes(2); // Ensure lock expiry is slightly more than the interval
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(_ => DoWorkAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30)); // Set the interval to 5 seconds

        return Task.CompletedTask;
    }

    private async void DoWorkAsync()
    {
        var now = DateTime.UtcNow;

        var @lock = new RedisDistributedLock($"{_lockKey}/{now}", _redis.GetDatabase(), options => options.Expiry(_lockExpiry));

        await using var handle = await @lock.TryAcquireAsync();
        if (handle != null)
        {
            _logger.LogInformation("Another instance is working. Skipping this iteration. Time: {time}", now);
            await handle.DisposeAsync();
            return;
        }

        await PublishMessageAsync();
        //await UpdateProductAsync(now.Second.ToString());

        _logger.LogInformation("Timed Background Service is working. Time: {time}", now);
    }

    private async Task PublishMessageAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        await publishEndpoint.Publish(new TestModel() { Id = 123 });
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _redis?.Dispose();
    }
}
