using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace demo_background_job.Job;

public class TimeTriggerJob(ILogger<TimeTriggerJob> logger, IConfiguration configuration) : IHostedService, IDisposable
{
    private readonly ILogger<TimeTriggerJob> _logger = logger;
    private Timer? _timer = null;
    private readonly ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("redis") ?? "");
    private readonly string _lockKey = "TimedHostedServiceLock";
    private readonly TimeSpan _lockExpiry = TimeSpan.FromSeconds(5); // Ensure lock expiry is slightly more than the interval

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(DoWorkAsync, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Set the interval to 5 seconds

        return Task.CompletedTask;
    }

    private async void DoWorkAsync(object? state)
    {
        var now = DateTime.UtcNow;

        var @lock = new RedisDistributedLock("MyLockName", _redis.GetDatabase());
        await using var handle = await @lock.TryAcquireAsync();
        if (handle != null)
        {
            _logger.LogInformation("Timed Background Service is working. Time: {time}", now);
            return;
        }

        //_logger.LogInformation("Another instance is working. Skipping this iteration. Time: {time}", now);
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
