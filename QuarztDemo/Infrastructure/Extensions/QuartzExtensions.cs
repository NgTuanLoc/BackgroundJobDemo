using Quartz;
using QuarztDemo.Infrastructure.Jobs;

namespace QuarztDemo.Infrastructure.Extensions;
public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzDefaults(
        this IServiceCollection services
    ) =>
        services
            .AddQuartz(q => { })
            .AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    public static IServiceCollectionQuartzConfigurator AddPassageOfTime(
        this IServiceCollectionQuartzConfigurator q
    )
    {
        var job = JobBuilder.Create<HelloJob>()
                            .WithIdentity("myJob", "group1")
                            .Build();

        // Trigger the job to run now, and then every 40 seconds
        var trigger = TriggerBuilder.Create()
            .WithIdentity("myTrigger", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(5)
                .RepeatForever())
            .Build();

        //await scheduler.ScheduleJob(job, trigger);

        return q;
    }
}