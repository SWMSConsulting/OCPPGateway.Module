using DevExpress.Persistent.BaseImpl.EF;

namespace OCPPGateway.Module.BusinessObjects.Events;

public abstract class OCPPEvent: BaseObject
{
    public virtual OCPPChargePoint ChargePoint { get; set; }
    
    public virtual DateTime Timestamp { get; set; }

    public abstract string Category { get; }

    public abstract string Description { get; }
}
