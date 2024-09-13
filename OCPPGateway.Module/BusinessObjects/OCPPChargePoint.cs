using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using MQTTnet.Internal;
using OCPPGateway.Module.Models;
using OCPPGateway.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Master Data")]
[DisplayName("Charge Point")]
public abstract class OCPPChargePoint : BaseObject
{
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

    [RuleRequiredField]
    public virtual string Identifier { get; set; }

    [RuleRequiredField]
    public virtual string Name { get; set; }

    public virtual IList<OCPPChargePointConnector> Connectors { get; set; } = new ObservableCollection<OCPPChargePointConnector>();

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