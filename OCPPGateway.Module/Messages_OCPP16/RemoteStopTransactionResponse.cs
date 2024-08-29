using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OCPPGateway.Module.Messages_OCPP16;

public class RemoteStopTransactionResponse
{
    [JsonProperty("status", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public RemoteResponseStatus status { get; set; }
}

