using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace OCPPGateway.Module.Messages_OCPP16;
public class ChangeConfigurationResponse
{
    [JsonProperty("status", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public ConfigurationStatus Status { get; set; }
}

public enum ConfigurationStatus
{
    [EnumMember(Value = @"Accepted")]
    Accepted,

    [EnumMember(Value = @"Rejected")]
    Rejected,

    [EnumMember(Value = @"RebootRequired")]
    RebootRequired,

    [EnumMember(Value = @"NotSupported")]
    NotSupported
}
