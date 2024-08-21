namespace OCPPGateway.Module.Services;

public static class TopicBuilder
{

    private static string baseTopic = Environment.GetEnvironmentVariable("MQTT_BASE_TOPIC") ?? "";

    public static string GetTopic(string protocolVersion, bool raw, bool fromClient, OCPPMessage? message = null, string? connectId = null)
    {
        var level = raw ? "raw" : "processed";
        var connection = connectId ?? "+";
        var action = message?.Action ?? "+";
        var direction = fromClient ? "out" : "in";
        var msgId = message?.UniqueId ?? "+";
        return $"{baseTopic}ocpp{protocolVersion}/{level}/{connection}/{action}/{direction}/{msgId}";
    }
}
