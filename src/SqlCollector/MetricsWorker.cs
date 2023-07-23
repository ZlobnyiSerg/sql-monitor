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

    private readonly Dictionary<string, Collector> _collectorCounters = new();

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
                        counter = metric.Type == MetricType.Counter
                            ? Metrics.CreateCounter(metric.MetricName, metric.HelpText, metric.Labels)
                            : Metrics.CreateGauge(metric.MetricName, metric.HelpText, metric.Labels);

                        _collectorCounters.Add(metric.MetricName, counter);
                    }

                    switch (counter)
                    {
                        case Counter cntr:
                            cntr.WithLabels(metric.LabelValues).IncTo(metric.Value);
                            break;
                        default:
                            ((Gauge)counter).WithLabels(metric.LabelValues).Set(metric.Value);
                            break;
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}