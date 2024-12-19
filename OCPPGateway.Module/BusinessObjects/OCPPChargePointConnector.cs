using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
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

    public virtual int Identifier { get; set; }

    [RuleRequiredField]
    public virtual string Name { get; set; } = "";

    public virtual string? LastStatus { get; set; }
    public virtual double? LastConsumption { get; set; }
    public virtual double? LastMeter { get; set; }

    [ModelDefault("DisplayFormat", "{0:P0}")]
    public virtual double? LastStateOfCharge { get; set; }

    public virtual IList<OCPPTransaction> Transactions { get; set; } = new ObservableCollection<OCPPTransaction>();

    [NotMapped]
    public OCPPTransaction? ActiveTransaction => Transactions.OrderByDescending(t => t.StartTime).FirstOrDefault(t => !t.IsStopped);

    [NotMapped]
    public bool IsInUse => ActiveTransaction != null;

    [NotMapped]
    public bool IsOffline => LastStatus == "Undefined";

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
