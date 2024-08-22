namespace OCPPGateway.Module.Models;

public class LineProtocolMessageBuilder
{
    private string Measurement = string.Empty;

    private Dictionary<string, string> Tags = new Dictionary<string, string>();

    private Dictionary<string, string> Fields = new Dictionary<string, string>();

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
            Fields.Add(key, value.Value.ToString().Replace(",","."));
        return this;
    }

    public LineProtocolMessageBuilder AddField(string key, int? value)
    {
        if (value.HasValue)
            Fields.Add(key, value.Value.ToString());
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
        return $"{Measurement},{tags} {fields}";
    }
}
