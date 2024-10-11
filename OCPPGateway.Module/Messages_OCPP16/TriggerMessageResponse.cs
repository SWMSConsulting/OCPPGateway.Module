using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OCPPGateway.Module.Messages_OCPP16;

public class TriggerMessageResponse
{
    /// <summary>
    /// Required. Indicates whether the request was accepted or rejected.
    /// </summary>
    [JsonProperty("status", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public TriggerMessageStatus Status { get; set; }
}

public enum TriggerMessageStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Rejected")]
    Rejected = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"NotImplemented")]
    NotImplemented = 2
}
