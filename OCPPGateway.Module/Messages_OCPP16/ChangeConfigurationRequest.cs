using Newtonsoft.Json;

namespace OCPPGateway.Module.Messages_OCPP16;

public class ChangeConfigurationRequest
{
    [JsonProperty("key", Required = Required.Always)]
    public string Key { get; set; }

    [JsonProperty("value", Required = Required.Always)]
    public string Value { get; set; }
}