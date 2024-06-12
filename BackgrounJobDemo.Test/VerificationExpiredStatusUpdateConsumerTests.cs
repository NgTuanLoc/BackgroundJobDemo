using BackgroundJobDemo.Infrastructure.Repositories;
using BackgroundJobDemo.IntegrationEvents;
using MassTransit;
using NSubstitute;
using static BackgroundJobDemo.Infrastructure.Models.TimeHasPassed;

namespace BackgrounJobDemo.Test;

public class VerificationExpiredStatusUpdateConsumerTests
{
    public VerificationExpiredStatusUpdateConsumerTests()
    {
    }

    [Fact]
    public async Task Consume_ShouldCallUpdateProductAsync_WithCorrectMessage()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var consumer = new VerificationExpiredStatusUpdateConsumer(userRepository);

        var context = Substitute.For<ConsumeContext<MinuteHasPassed>>();
        var message = new MinuteHasPassed(new System.DateTimeOffset(2024, 6, 12, 10, 0, 45, System.TimeSpan.Zero), null);
        context.Message.Returns(message);

        // Act
        await consumer.Consume(context);

        // Assert
        await userRepository.Received(1).UpdateProductAsync("45");
    }
}
