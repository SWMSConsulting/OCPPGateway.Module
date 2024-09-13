using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.BusinessObjects;
using System.ComponentModel;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Master Data")]
public abstract class OCPPChargePointConnector: AssetAdministrationShell
{
    public virtual OCPPChargePoint ChargePoint { get; set; }

    public override string Caption => $"{ChargePoint.Name} - {Identifier}";

    [RuleRange(1, 255)]
    public virtual int Identifier { get; set; }

    [Browsable(false)]
    public string ChargePointIdentifier => ChargePoint?.Identifier;
}
