using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.Base;
using MQTTnet.Internal;
using OCPPGateway.Module.Services;
using SWMS.Influx.Module.BusinessObjects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Charge Point Connector")]
[DefaultProperty(nameof(Caption))]
public abstract class OCPPChargePointConnector: AssetAdministrationShell
{
    public override void OnSaving()
    {
        base.OnSaving();

        if (Identifier == 0 && ChargePoint != null && ChargePoint.Connectors.Count > 0)
        {
            Identifier = ChargePoint.Connectors.Max(c => c.Identifier) + 1;
        }
    }
    public virtual OCPPChargePoint ChargePoint { get; set; }

    [Appearance("IdentifierDisabled", Enabled = false)]
    public virtual int Identifier { get; set; }
    public virtual string Name { get; set; } = "";

    public virtual IList<OCPPTransaction> Transactions { get; set; } = new ObservableCollection<OCPPTransaction>();

    [NotMapped]
    public OCPPTransaction? ActiveTransaction => Transactions.OrderByDescending(t => t.StartTime).FirstOrDefault(t => !t.IsStopped);

    [NotMapped]
    public bool IsInUse => ActiveTransaction != null;

    [NotMapped]
    public string? ConnectedRfid => ActiveTransaction?.StartTag?.Identifier;

    [NotMapped]
    public override string Caption => string.IsNullOrEmpty(Name) ? $"{ChargePoint.Name} - {Identifier}" : Name;

    [Action(
        Caption = "Start Transaction",
        TargetObjectsCriteria = "IsInUse == false"
    )]
    public void RemoteStartTransaction()
    {
        if (ActiveTransaction != null)
        {
            return;
        }

        var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.RemoteStartTransaction(this).RunInBackground();
    }

    [Action(
        Caption = "Stop Transaction",
        TargetObjectsCriteria = "IsInUse"
    )]
    public void RemoteStopTransaction()
    {
        if (ActiveTransaction == null)
        {
            return;
        }

        var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.RemoteStopTransaction(this).RunInBackground();
    }
}
