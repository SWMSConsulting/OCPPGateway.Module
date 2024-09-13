using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using MQTTnet.Internal;
using OCPPGateway.Module.Models;
using OCPPGateway.Module.Services;
using SWMS.Influx.Module.Attributes;
using SWMS.Influx.Module.BusinessObjects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Master Data")]
[DisplayName("Charge Point")]
public abstract class OCPPChargePoint : AssetAdministrationShell
{
    public override string Caption => Name;

    [RuleRequiredField]
    public virtual string Identifier { get; set; }

    [RuleRequiredField]
    public virtual string Name { get; set; }

    [Aggregated]
    public virtual IList<OCPPChargePointConnector> Connectors { get; set; } = new ObservableCollection<OCPPChargePointConnector>();


    [NotMapped]
    [LastDatapoint("is_online", "heartbeat")]
    [Appearance("LastHeartbeatDisabled", Enabled = false)]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm:ss}")]
    public DateTime? LastHeartbeat { get; set; }

    public override void OnSaving()
    {
        base.OnSaving();

        if (ObjectSpace.IsDeletedObject(this))
        {
            var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
            service?.ClearRetainFlag(typeof(ChargePoint), Identifier, true).RunInBackground();
        }
        else
        {
            Publish();
        }
    }

    #region OCPP related

    [Browsable(false)]
    public static Type? AssignableType
    {
        get
        {
            var type = typeof(OCPPChargePoint);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t != type && type.IsAssignableFrom(t))
                .FirstOrDefault();
        }
    }

    [Browsable(false)]
    public ChargePoint ChargePoint
    {
        get
        {
            return new ChargePoint
            {
                ChargePointId = Identifier,
                Name = Name,
            };
        }
    }

    public void Publish()
    {
        var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.Publish(ChargePoint).RunInBackground();
    }
    #endregion
}