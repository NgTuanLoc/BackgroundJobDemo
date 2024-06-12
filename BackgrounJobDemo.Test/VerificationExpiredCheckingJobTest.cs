using BackgroundJobDemo.Infrastructure.Models;
using demo_background_job.Job;
using MassTransit;
using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace VerificationExpiredCheckingJobTests;

public class VerificationExpiredCheckingJobTests
{
    private readonly ILogger<VerificationExpiredCheckingJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedLock _distributedLock;
    private readonly IOptions<AppSettings> _options;

    public VerificationExpiredCheckingJobTests()
    {
        // Mock appsettings
        var appSettings = new AppSettings
        {
            SchedulingJobTimeUnit = "Hour"
        };

        _options = Substitute.For<IOptions<AppSettings>>();
        _options.Value.Returns(appSettings);
        // Mock other dependency
        _logger = Substitute.For<ILogger<VerificationExpiredCheckingJob>>();
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _timeProvider = Substitute.For<TimeProvider>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _distributedLock = Substitute.For<IDistributedLock>();

        // Setup service scope
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IPublishEndpoint)).Returns(_publishEndpoint);
        serviceProvider.GetService(typeof(IDistributedLock)).Returns(_distributedLock);
        serviceProvider.GetService(typeof(TimeProvider)).Returns(_timeProvider);

        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(serviceProvider);

        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    [Fact]
    public async Task StartAsync_ShouldStartTimer()
    {
        // Arrange
        var job = new VerificationExpiredCheckingJob(_logger, _scopeFactory, _timeProvider, _options);

        // Act
        await job.StartAsync(CancellationToken.None);

        // Assert
        // Verify that the logger logs the start information
        _logger.Received().LogInformation("Timed Hosted Service running.");
    }

    [Fact]
    public async Task DoWorkAsync_ShouldPublishMessageAndLogInformation()
    {
        // Arrange
        var job = new VerificationExpiredCheckingJob(_logger, _scopeFactory, _timeProvider, _options);

        // Simulate acquiring the lock
        var handle = Substitute.For<IDistributedSynchronizationHandle>();

        _distributedLock.TryAcquireAsync().Returns(handle);
        _timeProvider.GetUtcNow().Returns(TimeProvider.System.GetUtcNow());

        // Act
        job.DoWorkAsync();

        // Assert
        // Verify that the logger logs the information
        _logger.Received().LogInformation("Timed Background Service is working.");
        // Verify that the publish endpoint is NOT called
        await _publishEndpoint.Received().Publish(Arg.Any<object>());
    }

    [Fact]
    public async Task DoWorkAsync_Should_Skip_If_Lock_Cannot_Be_Acquired()
    {
        // Arrange
        var job = new VerificationExpiredCheckingJob(_logger, _scopeFactory, _timeProvider, _options);

        // Simulate acquiring the lock
        IDistributedSynchronizationHandle? handle = null;
        _distributedLock.TryAcquireAsync().Returns(handle);

        // Act
        job.DoWorkAsync();

        // Assert
        // Verify that the logger logs the information
        _logger.Received().LogInformation("Another instance is working. Skipping this iteration.");
        // Verify that the publish endpoint is called
        await _publishEndpoint.DidNotReceive().Publish(Arg.Any<object>());
    }

    [Fact]
    public async Task StopAsync_ShouldStopTimer()
    {
        // Arrange
        var job = new VerificationExpiredCheckingJob(_logger, _scopeFactory, _timeProvider, _options);

        // Act
        await job.StartAsync(CancellationToken.None);
        await job.StopAsync(CancellationToken.None);

        // Assert
        // Verify that the logger logs the stop information
        _logger.Received().LogInformation("Timed Hosted Service is stopping.");
    }
}