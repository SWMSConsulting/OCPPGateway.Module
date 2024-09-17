using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OCPPGateway.Module.Messages_OCPP16;

public class SetChargingProfileResponse
{
    [JsonProperty("status", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public SetChargingProfileStatus Status { get; set; }
}

public enum SetChargingProfileStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Rejected")]
    Rejected = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"NotSupported")]
    NotSupported = 2,
}
