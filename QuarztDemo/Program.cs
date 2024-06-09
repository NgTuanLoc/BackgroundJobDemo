using Quartz;
using QuarztDemo.Infrastructure.Jobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQuartz(q =>
{
    // You can configure Quartz here if needed
});

builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});

var app = builder.Build();

var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();

// define the job and tie it to our HelloJob class
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

await scheduler.ScheduleJob(job, trigger);

// will block until the last running job completes
await app.RunAsync();
