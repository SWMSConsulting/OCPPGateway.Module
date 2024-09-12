using Newtonsoft.Json.Linq;

namespace OCPPGateway.Module.Services;

public class LineProtocolMessageBuilder
{
    private string Measurement = string.Empty;

    private Dictionary<string, string> Tags = new Dictionary<string, string>();

    private Dictionary<string, string> Fields = new Dictionary<string, string>();

    private long? Timestamp = null; // nanoseconds
    public LineProtocolMessageBuilder AddMeasurement(string measurement)
    {
        Measurement = measurement;
        return this;
    }

    public LineProtocolMessageBuilder AddTag(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            Tags.Add(key, value);
        return this;
    }

    public LineProtocolMessageBuilder AddField(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            Fields.Add(key, value);
        return this;
    }

    public LineProtocolMessageBuilder AddField(string key, double? value)
    {
        if (value.HasValue)
            Fields.Add(key, value.Value.ToString().Replace(",", "."));
        return this;
    }

    public LineProtocolMessageBuilder AddField(string key, int? value)
    {
        if (value.HasValue)
            Fields.Add(key, value.Value.ToString());
        return this;
    }

    public LineProtocolMessageBuilder AddTimestamp(DateTime? timestamp)
    {
        return AddTimestamp(timestamp.HasValue ? new DateTimeOffset(timestamp.Value) : null);
    }

    public LineProtocolMessageBuilder AddTimestamp(DateTimeOffset? timestamp)
    {
        if (timestamp.HasValue)
            Timestamp = timestamp?.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).Ticks * 100;
        return this;
    }
    public string Build()
    {
        if (string.IsNullOrEmpty(Measurement))
            throw new InvalidOperationException("Measurement is required");

        if (Tags.Count == 0)
            throw new InvalidOperationException("At least one tag is required");

        if (Fields.Count == 0)
            throw new InvalidOperationException("At least one field is required");

        var tags = string.Join(",", Tags.Select(t => $"{t.Key}={t.Value}"));
        var fields = string.Join(",", Fields.Select(f => $"{f.Key}={f.Value}"));
        string timestamp = Timestamp.HasValue ? $" {Timestamp.Value}" : "";

        return $"{Measurement},{tags} {fields}";
    }
}
