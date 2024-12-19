using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.ComponentModel;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Unknown Charge Point")]
public class UnknownOCPPChargePoint: BaseObject
{
    public virtual string Identifier { get; set; }


    [Action(Caption = "Add to Charge Points")]
    public void AddToKnownChargePoints()
    {
        var type = OCPPChargePoint.AssignableType;
        if (type == null)
        {
            return;
        }

        var existingChargePoint = (OCPPChargePoint)ObjectSpace.FindObject(type, CriteriaOperator.Parse("Identifier = ?", Identifier));

        if (existingChargePoint == null)
        {
            existingChargePoint = (OCPPChargePoint)ObjectSpace.CreateObject(type);
            existingChargePoint.Identifier = Identifier;
            existingChargePoint.Name = Identifier;
        }

        ObjectSpace.Delete(this);        
        ObjectSpace.CommitChanges();
    }
}
