#nullable disable

namespace OCPPGateway.Module.Models;

public partial class ConnectorStatus
{
    public string ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public string LastStatus { get; set; }
    public double? LastMeter { get; set; }

    public string ToLineProtocol()
    {
        return $"connector," +
            $"terminal_id={ChargePointId},connector_id={ConnectorId} " +
            $"meter_reading={LastMeter}, status={LastStatus}";
    }
}

