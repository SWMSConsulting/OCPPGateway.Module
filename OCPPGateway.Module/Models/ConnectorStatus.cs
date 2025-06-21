#nullable disable

namespace OCPPGateway.Module.Models;

public partial class ConnectorStatus
{
    public string ChargePointId { get; set; }
    public int ConnectorId { get; set; }

    public string LastStatus { get; set; }
    public DateTime? LastStatusTimestamp { get; set; }

    public double? LastMeter { get; set; }
    public DateTime? LastMeterTimestamp { get; set; }

    public double? LastConsumption { get; set; }

    public double? StateOfCharge { get; set; }
}

