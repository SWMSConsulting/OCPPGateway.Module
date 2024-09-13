using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using MQTTnet.Internal;
using OCPPGateway.Module.Models;
using OCPPGateway.Module.Services;
using System.ComponentModel;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Master Data")]
[DisplayName("Unknown Charge Tag")]
public class UnknownOCPPChargeTag: BaseObject
{
    public virtual string Identifier { get; set; }



    [Action(Caption = "Add to Charge Tags")]
    public void AddToKnownChargeTags()
    {
        var type = OCPPChargeTag.AssignableType;
        if (type == null)
        {
            return;
        }

        var existingChargeTag = (OCPPChargeTag)ObjectSpace.FindObject(type, CriteriaOperator.Parse("Identifier = ?", Identifier));

        if (existingChargeTag == null)
        {
            existingChargeTag = (OCPPChargeTag)ObjectSpace.CreateObject(type);
            existingChargeTag.Identifier = Identifier;
            existingChargeTag.Name = Identifier;
            existingChargeTag.Blocked = true;
        }

        var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.ClearRetainFlag(typeof(UnknownChargeTag), Identifier, true).RunInBackground();

        ObjectSpace.Delete(this);

        ObjectSpace.CommitChanges();
    }
}
