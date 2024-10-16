using Newtonsoft.Json;

namespace OCPPGateway.Module.Messages_OCPP16;

public class SendLocalListResponse
{
    [JsonProperty("status", Required = Required.Always)]
    public UpdateStatus Status { get; set; }
}

public enum UpdateStatus
{
    [System.Runtime.Serialization.EnumMember(Value = @"Accepted")]
    Accepted,

    [System.Runtime.Serialization.EnumMember(Value = @"Failed")]
    Failed,

    [System.Runtime.Serialization.EnumMember(Value = @"NotSupported")]
    NotSupported,

    [System.Runtime.Serialization.EnumMember(Value = @"VersionMismatch")]
    VersionMismatch
}