using BackgroundJobDemo.Infrastructure.Repositories;
using MassTransit;
using static BackgroundJobDemo.Infrastructure.Models.TimeHasPassed;

namespace BackgroundJobDemo.IntegrationEvents;

public class VerificationExpiredStatusUpdateConsumer(IUserRepository userRepository) : IConsumer<MinuteHasPassed>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task Consume(ConsumeContext<MinuteHasPassed> context)
    {
        var message = context.Message.Now.Second;
        await _userRepository.UpdateProductAsync(message.ToString());
    }
}