using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace SqlCollector.Sources;

public class SqlOverallCollector : IMetricsCollector
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlOverallCollector> _logger;
    private readonly SqlMonitoringOptions _options;

    public SqlOverallCollector(
        IConfiguration configuration,
        IOptions<SqlMonitoringOptions> options,
        ILogger<SqlOverallCollector> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _options = options.Value;
    }

    private readonly long[] GaugeTypes = { 65792L, 537003264L };

    private const string Query = """
select
	trim(c.object_name) as object_name,
	trim(c.counter_name) as counter_name,
	trim(c.instance_name) as instance_name,
	c.cntr_value,
	c.cntr_type,
	coalesce(b.cntr_value,0) as cntr_base
from
	sys.dm_os_performance_counters c
left join
	sys.dm_os_performance_counters b
		on (c.object_name=b.object_name and trim(b.counter_name)=trim(c.counter_name)+' base' and c.instance_name=b.instance_name)
where
	trim(c.counter_name) not like '%base'
order by
	object_name,
	counter_name
""";

    private Counter _collectorCounter = Metrics.CreateCounter("sql_collector_collecting", "Counter for SQL metric's collector");

    public async IAsyncEnumerable<MetricValue> GetMetrics([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("SqlServer");
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var command = conn.CreateCommand();
        command.CommandText = Query;

        var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!reader.HasRows) yield break;

        var groups = new HashSet<string>(_options.Groups);

        var stopwatch = Stopwatch.StartNew();
        while (await reader.ReadAsync(cancellationToken))
        {
            var groupName = reader.GetString(0);

            if (!groups.Contains(groupName))
                continue;

            var counterName = reader.GetString(1);
            var metricName = groupName + ":" + counterName;
            var instanceName = reader.GetString(2);
            var value = reader.GetInt64(3);
            var counterType = reader.GetInt32(4);
            var valueBase = reader.GetInt64(5);

            double calculatedValue = value;

            if (valueBase != 0)
                calculatedValue = calculatedValue * 100 / valueBase;

            yield return new MetricValue(
                GaugeTypes.Contains(counterType) ? MetricType.Gauge : MetricType.Counter,
                metricName,
                new[] { "instance_name" },
                new[] { instanceName },
                calculatedValue)
            {
                HelpText = counterName
            };

            if (cancellationToken.IsCancellationRequested)
                yield break;
        }
        _collectorCounter.Inc(stopwatch.ElapsedMilliseconds);
    }
}