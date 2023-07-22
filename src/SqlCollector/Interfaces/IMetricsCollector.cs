namespace SqlCollector.Interfaces;

public interface IMetricsCollector
{
    IAsyncEnumerable<MetricValue> GetMetrics(CancellationToken cancellationToken);
}