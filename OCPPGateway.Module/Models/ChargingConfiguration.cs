namespace OCPPGateway.Module.Models;

public class ChargingConfiguration
{
    public string ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public ChargingPreset ChargingPreset { get; set; } = ChargingPreset.Default;
}
