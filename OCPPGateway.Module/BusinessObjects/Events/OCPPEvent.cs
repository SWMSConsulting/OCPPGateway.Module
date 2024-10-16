using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace OCPPGateway.Module.BusinessObjects.Events;

[NavigationItem("Events")]
public abstract class OCPPEvent: BaseObject
{
    public virtual OCPPChargePoint ChargePoint { get; set; }

    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm:ss}")]
    public virtual DateTime Timestamp { get; set; }

    public abstract string Category { get; }

    public abstract string Description { get; }
}
