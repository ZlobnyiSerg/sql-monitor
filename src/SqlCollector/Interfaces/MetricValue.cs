using System.Globalization;

namespace SqlCollector.Interfaces;

public record MetricValue
{
    public string MetricName { get; }

    public string HelpText { get; set; }

    public string[] Labels { get; }

    public string[] LabelValues { get; }

    public double Value { get; }

    public MetricValue(string metricName, string[] labels, string[] labelValues, double value)
    {
        MetricName = metricName
            .Replace(' ', '_')
            .Replace(':', '_')
            .Replace("(", "")
            .Replace(")", "")
            .Replace(".", "")
            .Replace("%", "p")
            .Replace("&", "")
            .Replace("=", "")
            .Replace('/', '_')
            .Replace('-', '_')
            .Replace('>', '_')
            .Replace('<', '_')
            .Replace("__", "_")
            .Replace("__", "_")
            .ToLower(CultureInfo.InvariantCulture);
        Labels = labels;
        LabelValues = labelValues;
        Value = value;
    }
}