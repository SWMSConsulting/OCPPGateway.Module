using Newtonsoft.Json;

namespace OCPPGateway.Module.Messages_OCPP16;

public class RemoteStartTransactionRequest
{
    [JsonProperty("connectorId", Required = Required.Always)]
    public int connectorId { get; set; }


    [JsonProperty("idTag", Required = Required.Always)]
    [System.ComponentModel.DataAnnotations.StringLength(20)]
    public string idTag { get; set; }


    [JsonProperty("chargingProfile", Required = Required.DisallowNull)]
    public ChargingProfile chargingProfile { get; set; }
}