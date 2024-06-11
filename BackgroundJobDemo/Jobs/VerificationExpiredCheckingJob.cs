using BackgroundJobDemo.Infrastructure.Extensions;
using MassTransit;
using Medallion.Threading;

namespace demo_background_job.Job;

public class VerificationExpiredCheckingJob(ILogger<VerificationExpiredCheckingJob> logger, IServiceScopeFactory scopeFactory, TimeProvider timeProvider) : IHostedService
{
    private readonly ILogger<VerificationExpiredCheckingJob> _logger = logger;
    private Timer? _timer = null;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly TimeUnit _timeUnit = TimeUnit.Minute;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(_ => DoWorkAsync(), null, TimeSpan.Zero, _timeUnit.ToTimeSpan());

        return Task.CompletedTask;
    }

    public async void DoWorkAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var @lock = scope.ServiceProvider.GetRequiredService<IDistributedLock>();

        await using var handle = await @lock.TryAcquireAsync();
        if (handle is not null)
        {
            _logger.LogInformation("Timed Background Service is working.");
            await PublishMessageAsync();
            await Task.Delay(10000);
            return;
        }
        _logger.LogInformation("Another instance is working. Skipping this iteration.");
    }

    private async Task PublishMessageAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        await publishEndpoint.Publish((object)_timeUnit.ToEvent(_timeProvider.GetUtcNow()));
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }
}