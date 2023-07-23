var builder = WebApplication
    .CreateBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration
    .AddYamlFile("appsettings.yaml", false)
    .AddYamlFile("sql-collector.yaml", true, true);

builder.Services.AddSingleton<IMetricsCollector, SqlOverallCollector>();
builder.Services.AddSingleton<IHostedService, MetricsWorker>();
builder.Services.Configure<SqlMonitoringOptions>(opts => builder.Configuration.GetSection(SqlMonitoringOptions.SqlMonitoring).Bind(opts));

var app = builder.Build();
app.MapMetrics();

app.Run();