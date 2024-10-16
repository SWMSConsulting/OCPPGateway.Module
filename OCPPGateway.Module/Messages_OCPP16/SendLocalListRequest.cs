using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace OCPPGateway.Module.Messages_OCPP16;

public class SendLocalListRequest
{
    [JsonProperty("listVersion", Required = Required.Always)]
    public int ListVersion { get; set; }

    [JsonProperty("localAuthorizationList", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public List<AuthorizationData>? LocalAuthorizationList { get; set; } = null;

    [JsonProperty("updateType", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public UpdateType UpdateType { get; set; }
}

public class AuthorizationData
{
    [StringLength(20)]
    [JsonProperty("idTag", Required = Required.Always)]
    public string IdTag { get; set; }

    [JsonProperty("idTagInfo", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public IdTagInfo IdTagInfo { get; set; }
}

public enum UpdateType
{
    [System.Runtime.Serialization.EnumMember(Value = "Differential")]
    Differential = 0,

    [System.Runtime.Serialization.EnumMember(Value = "Full")]
    Full = 1
}
