using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.BusinessObjects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Charge Point Connector")]
[DefaultProperty(nameof(DisplayName))]
public abstract class OCPPChargePointConnector: AssetAdministrationShell
{
    public virtual OCPPChargePoint ChargePoint { get; set; }

    public override string Caption => $"{ChargePoint.Name} - {Identifier}";

    [RuleRange(1, 255)]
    public virtual int Identifier { get; set; }

    public virtual IList<OCPPTransaction> Transactions { get; set; } = new ObservableCollection<OCPPTransaction>();

    [Browsable(false)]
    public string ChargePointIdentifier => ChargePoint?.Identifier;

    [NotMapped]
    public string DisplayName => $"{ChargePoint?.Name} - {Identifier}";
}
