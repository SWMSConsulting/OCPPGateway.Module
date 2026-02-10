namespace OCPPGateway.Module.Models;

public class UnknownChargeTag
{
    public string TagId { get; set; }
    public DateTime? Timestamp { get; set; }
    public string Action { get; set; }
    public string? ChargePointIdentifier { get; set; }
}
