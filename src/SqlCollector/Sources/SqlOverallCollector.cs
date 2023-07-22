using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using SqlCollector.Interfaces;

namespace SqlCollector.Sources;

public class SqlOverallCollector : IMetricsCollector
{
    private readonly IConfiguration _configuration;

    public SqlOverallCollector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async IAsyncEnumerable<MetricValue> GetMetrics([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var conn = new SqlConnection(_configuration.GetConnectionString("SqlServer"));
        await conn.OpenAsync(cancellationToken);
        await using var command = conn.CreateCommand();

        command.CommandText = """
select * from sys.dm_os_performance_counters
""";

        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!reader.HasRows) yield break;

        while (await reader.ReadAsync(cancellationToken))
        {
            var metricName = reader.GetString(0).Trim() + ":" + reader.GetString(1).Trim();
            var instanceName = reader.GetString(2).Trim();
            var value = reader.GetInt64(3);

            yield return new MetricValue(metricName, new[] { "instance_name" }, new[] { instanceName }, value);

            if (cancellationToken.IsCancellationRequested)
                yield break;
        }
    }
}