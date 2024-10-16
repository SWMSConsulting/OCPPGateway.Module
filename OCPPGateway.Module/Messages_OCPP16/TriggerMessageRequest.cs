using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OCPPGateway.Module.Messages_OCPP16;

public class TriggerMessageRequest
{
    /// <summary>
    /// Required. The type of the message to trigger.
    /// </summary>
    [JsonProperty("requestedMessage", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public MessageTrigger RequestedMessage { get; set; }

    /// <summary>
    /// Optional. Specifies the ID of the connector for the triggered message (if applicable).
    /// </summary>
    [JsonProperty("connectorId", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
    public int? ConnectorId { get; set; } = null;
}

public enum MessageTrigger
{
    [System.Runtime.Serialization.EnumMember(Value = @"BootNotification")]
    BootNotification = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"DiagnosticsStatusNotification")]
    DiagnosticsStatusNotification = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"FirmwareStatusNotification")]
    FirmwareStatusNotification = 2,

    [System.Runtime.Serialization.EnumMember(Value = @"Heartbeat")]
    Heartbeat = 3,

    [System.Runtime.Serialization.EnumMember(Value = @"MeterValues")]
    MeterValues = 4,

    [System.Runtime.Serialization.EnumMember(Value = @"StatusNotification")]
    StatusNotification = 5,

    [System.Runtime.Serialization.EnumMember(Value = @"TransactionEvent")]
    TransactionEvent = 6
}
