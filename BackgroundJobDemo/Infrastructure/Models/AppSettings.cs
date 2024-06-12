namespace BackgroundJobDemo.Infrastructure.Models;

public class LoggingOptions
{
    public LogLevelOptions LogLevel { get; set; }
}

public class LogLevelOptions
{
    public string Default { get; set; }
    public string MicrosoftAspNetCore { get; set; }
}

public class ConnectionStringsOptions
{
    public string DefaultConnection { get; set; }
    public string RabbitMq { get; set; }
}

public class AppSettings
{
    public LoggingOptions Logging { get; set; }
    public ConnectionStringsOptions ConnectionStrings { get; set; }
    public string Redis { get; set; }
    public string SchedulingJobTimeUnit { get; set; } = "";
}

