using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Master Data")]
public abstract class OCPPChargePointConnector: BaseObject
{
    public virtual OCPPChargePoint ChargePoint { get; set; }
}
