using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Unknown")]
public class UnknownChargePoint: BaseObject
{
    public virtual string Identifier { get; set; }
}
