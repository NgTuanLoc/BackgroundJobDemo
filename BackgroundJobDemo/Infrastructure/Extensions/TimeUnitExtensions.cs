using BackgroundJobDemo.Infrastructure.Models;
using static BackgroundJobDemo.Infrastructure.Models.TimeHasPassed;

namespace BackgroundJobDemo.Infrastructure.Extensions;

public enum TimeUnit
{
    Minute,
    Hour,
    Day
}

public static class TimeUnitExtensions
{
    public static TimeUnit ToTimeUnit(this string timeUnitString) =>
    Enum.Parse<TimeUnit>(timeUnitString);

    public static TimeSpan ToTimeSpan(this TimeUnit timeUnit) =>
        timeUnit switch
        {
            TimeUnit.Minute => TimeSpan.FromSeconds(20),
            TimeUnit.Hour => TimeSpan.FromHours(1),
            TimeUnit.Day => TimeSpan.FromDays(1),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), $"Not expected time unit value: {timeUnit}")
        };

    public static TimeHasPassed ToEvent(this TimeUnit timeUnit, DateTimeOffset now)
    {
        var previous = now - timeUnit.ToTimeSpan();
        return timeUnit switch
        {
            TimeUnit.Minute => new MinuteHasPassed(now, previous),
            TimeUnit.Hour => new HourHasPassed(now, previous),
            TimeUnit.Day => new DayHasPassed(now, previous),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), $"Not expected time unit value: {timeUnit}")
        };
    }
}