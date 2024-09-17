#nullable disable

namespace OCPPGateway.Module.Models;

public partial class ConnectorStatus
{
    public string ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public string LastStatus { get; set; }
    public double? LastMeter { get; set; }
}

