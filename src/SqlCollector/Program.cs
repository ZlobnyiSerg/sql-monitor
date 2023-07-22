var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IMetricsCollector, SqlOverallCollector>();
builder.Services.AddSingleton<IHostedService, MetricsWorker>();

var app = builder.Build();
app.MapMetrics();

app.Run();