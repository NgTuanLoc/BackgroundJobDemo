using StackExchange.Redis;

namespace demo_background_job.Job;

public class TimeTriggerJob : IHostedService, IDisposable
{
    private readonly ILogger<TimeTriggerJob> _logger;
    private Timer? _timer = null;
    private readonly ConnectionMultiplexer _redis;
    private readonly string _lockKey = "TimedHostedServiceLock";
    private readonly TimeSpan _lockExpiry = TimeSpan.FromSeconds(5); // Ensure lock expiry is slightly more than the interval

    public TimeTriggerJob(ILogger<TimeTriggerJob> logger, IConfiguration configuration)
    {
        _logger = logger;
        _redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("redis") ?? "");
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // Set the interval to 5 seconds

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        var now = DateTime.UtcNow;
        var lockKey = _lockKey;

        if (AcquireLock(lockKey, Environment.MachineName, _lockExpiry))
        {
            try
            {
                // Perform the task
                _logger.LogInformation("Timed Background Service is working. Time: {time}", now);
                // Your task logic here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task.");
            }
            finally
            {
                // Ensure the lock is released only if it's still held by this instance
                //ReleaseLock(lockKey, Environment.MachineName);
            }
        }
        else
        {
            _logger.LogInformation("Another instance is working. Skipping this iteration. Time: {time}", now);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private bool AcquireLock(string key, string value, TimeSpan expiration)
    {
        try
        {
            // Try to set the value of the lock key, only if it does not already exist
            return _redis.GetDatabase().StringSet(key, value, expiration, When.NotExists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire lock.");
            return false;
        }
    }

    private void ReleaseLock(string key, string value)
    {
        try
        {
            var db = _redis.GetDatabase();
            var lockValue = db.StringGet(key);

            // Release the lock only if it is held by the current instance
            if (lockValue == value)
            {
                db.KeyDelete(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release lock.");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _redis?.Dispose();
    }
}
