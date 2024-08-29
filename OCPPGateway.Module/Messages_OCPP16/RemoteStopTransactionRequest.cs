using Newtonsoft.Json;

namespace OCPPGateway.Module.Messages_OCPP16;

public class RemoteStopTransactionRequest
{
    [JsonProperty("transactionId", Required = Required.Always)]
    public int transactionId { get; set; }
}
