using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OCPPGateway.Module.Messages_OCPP16;

public class RemoteStartTransactionResponse
{
    [JsonProperty("status", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public RemoteResponseStatus Status { get; set; }
}
