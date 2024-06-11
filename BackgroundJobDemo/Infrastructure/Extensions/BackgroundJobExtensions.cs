using BackgroundJobDemo.IntegrationEvents;
using demo_background_job.Job;
using MassTransit;

namespace BackgroundJobDemo.Infrastructure.Extensions;

public static class BackgroundJobExtensions
{
    public static IServiceCollection ConfigureBackgroundJobExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumersFromNamespaceContaining<VerificationExpiredStatusUpdateConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseMessageRetry(r =>
                {
                    r.Handle<RabbitMqConnectionException>();
                    r.Interval(5, TimeSpan.FromSeconds(10));
                });

                cfg.Host(new Uri(configuration.GetConnectionString("rabbitmq") ?? ""));

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHostedService<VerificationExpiredCheckingJob>();

        return services;
    }
}