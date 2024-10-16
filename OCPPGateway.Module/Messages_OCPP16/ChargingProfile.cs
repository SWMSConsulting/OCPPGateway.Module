using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace OCPPGateway.Module.Messages_OCPP16;

public class ChargingProfile
{
    [JsonProperty("chargingProfileId", Required = Required.Always)]
    public int ChargingProfileId { get; set; }


    [JsonProperty("transactionId", Required = Required.Default)]
    public int TransactionId { get; set; }


    [JsonProperty("stackLevel", Required = Required.Always)]
    public int StackLevel { get; set; }


    [JsonProperty("chargingProfilePurpose", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public ChargingProfilePurpose ChargingProfilePurpose { get; set; }


    [JsonProperty("chargingProfileKind", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public ChargingProfileKind ChargingProfileKind { get; set; }


    [JsonProperty("recurrencyKind", Required = Required.Default)]
    [JsonConverter(typeof(StringEnumConverter))]
    public RecurrencyKind RecurrencyKind { get; set; }


    [JsonProperty("validFrom", Required = Required.Default)]
    public DateTime ValidFrom { get; set; }


    [JsonProperty("validTo", Required = Required.Default)]
    public DateTime ValidTo { get; set; }


    [JsonProperty("chargingSchedule", Required = Required.Always)]
    public ChargingProfileSchedule ChargingProfileSchedule { get; set; }
}

public enum ChargingProfilePurpose
{
    [System.Runtime.Serialization.EnumMember(Value = @"ChargePointMaxProfile")]
    ChargePointMaxProfile = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"TxDefaultProfile")]
    TxDefaultProfile = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"TxProfile")]
    TxProfile = 2,
}

public enum ChargingProfileKind
{
    [System.Runtime.Serialization.EnumMember(Value = @"Absolute")]
    Absolute = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Recurring")]
    Recurring = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"Relative")]
    Relative = 2,
}

public enum RecurrencyKind
{
    [System.Runtime.Serialization.EnumMember(Value = @"Daily")]
    Daily = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Weekly")]
    Weekly = 1,
}

public class ChargingProfileSchedule
{
    [JsonProperty("duration", Required = Required.Default)]
    public int Duration { get; set; }


    [JsonProperty("startSchedule", Required = Required.Default)]
    public DateTime StartSchedule { get; set; }


    [JsonProperty("chargingRateUnit", Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public ChargingRateUnit ChargingRateUnit { get; set; }


    [JsonProperty("chargingSchedulePeriod", Required = Required.Always)]
    public ChargingSchedulePeriod[] ChargingSchedulePeriod { get; set; }


    [JsonProperty("minChargingRate", Required = Required.Default)]
    public double MinChargingRate { get; set; } // multiple of 0.1
}

public enum ChargingRateUnit
{
    [System.Runtime.Serialization.EnumMember(Value = @"W")]
    W = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"A")]
    A = 1,
}

public class ChargingSchedulePeriod
{
    [JsonProperty("startPeriod", Required = Required.Always)]
    public int StartPeriod { get; set; }


    [JsonProperty("limit", Required = Required.Always)]
    public double Limit { get; set; } // multiple of 0.1
}
