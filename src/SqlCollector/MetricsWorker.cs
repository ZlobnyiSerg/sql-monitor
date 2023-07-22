namespace SqlCollector;

public class MetricsWorker : BackgroundService
{
    private readonly IEnumerable<IMetricsCollector> _collectors;
    private readonly ILogger<MetricsWorker> _logger;

    public MetricsWorker(
        IEnumerable<IMetricsCollector> collectors,
        ILogger<MetricsWorker> logger)
    {
        _collectors = collectors;
        _logger = logger;
    }

    private readonly Dictionary<string, Counter> _collectorCounters = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var collector in _collectors)
            {
                var metrics = collector.GetMetrics(stoppingToken);
                await foreach (var metric in metrics.WithCancellation(stoppingToken))
                {
                    if (!_collectorCounters.TryGetValue(metric.MetricName, out var counter))
                    {
                        counter = Metrics.CreateCounter(metric.MetricName, metric.HelpText, metric.Labels);
                        _collectorCounters.Add(metric.MetricName, counter);
                    }
                    counter.WithLabels(metric.LabelValues).IncTo(metric.Value);
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}