using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Quarzt.Infrastructure;

public class SqlServerHealthCheck(IConfiguration configuration) :
    IHealthCheck
{
    readonly string _connectionString = configuration.GetConnectionString("quartz") ?? "";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();

            command.CommandText = "SELECT 1";

            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("SqlServer");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SqlServer", ex);
        }
    }

    public static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
    {
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(result.ToJsonString());
    }
}