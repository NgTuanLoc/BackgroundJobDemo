namespace QuarztDemo.Infrastructure.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;

[DisallowConcurrentExecution]

public class HelloJob(ILogger<HelloJob> logger) : IJob
{
    private readonly ILogger<HelloJob> _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Hello world!");
        return Task.CompletedTask;
    }
}