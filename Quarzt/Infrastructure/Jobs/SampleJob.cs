using MassTransit;

namespace Quarzt.Infrastructure.Jobs;

public class SampleJob(IServiceScopeFactory scopeFactory) :
    BackgroundService
{
    readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var messageScheduler = scope.ServiceProvider.GetRequiredService<IMessageScheduler>();

        await messageScheduler.SchedulePublish(TimeSpan.FromSeconds(15), new DemoMessage { Value = "Hello, World" }, stoppingToken);
    }
}