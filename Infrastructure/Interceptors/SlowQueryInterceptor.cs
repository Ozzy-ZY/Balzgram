using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Interceptors;

public class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger, int slowQueryThresholdMs = 10)
    : DbCommandInterceptor
{
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogIfSlow(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        LogIfSlow(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        LogIfSlow(command, eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        LogIfSlow(command, eventData);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogIfSlow(DbCommand command, CommandExecutedEventData eventData)
    {
        var executionTimeMs = eventData.Duration.TotalMilliseconds;

        if (executionTimeMs > slowQueryThresholdMs)
        {
            logger.LogWarning(
                "Slow query detected! Execution time: {ExecutionTime}ms (threshold: {Threshold}ms)\n" +
                "Query: {Query}\n" +
                "Parameters: {Parameters}",
                executionTimeMs,
                slowQueryThresholdMs,
                command.CommandText,
                GetParametersAsString(command));
        }
    }

    private static string GetParametersAsString(DbCommand command)
    {
        if (command.Parameters.Count == 0)
            return "None";

        var parameters = new List<string>();
        foreach (DbParameter parameter in command.Parameters)
        {
            var value = parameter.Value == null ? "NULL" : parameter.Value.ToString();
            parameters.Add($"{parameter.ParameterName}={value}");
        }

        return string.Join(", ", parameters);
    }
}
