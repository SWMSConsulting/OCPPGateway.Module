using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using MQTTnet.Internal;
using OCPPGateway.Module.Models;
using OCPPGateway.Module.Services;
using System.ComponentModel;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Master Data")]
public abstract class OCPPChargePoint: BaseObject
{
    public override void OnSaving()
    {
        base.OnSaving();

        Publish();
    }

    [RuleRequiredField]
    public virtual string Identifier { get; set; }

    [RuleRequiredField]
    public virtual string Name { get; set; }


    #region OCPP related

    [Browsable(false)]
    public static Type? AssignableType
    {
        get
        {
            var type = typeof(OCPPChargePoint);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(type.IsAssignableFrom)
                .Where(t => t != type);
            return types.FirstOrDefault();
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
