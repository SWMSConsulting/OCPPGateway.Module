using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP Control")]
public class OCPPMessageCallbackLink : BaseObject
{
    public virtual string Action { get; set; }

    public virtual bool? FromChargePoint { get; set; }

    [EditorAlias("ImplementingTypeStringEditor")]
    public virtual string OCPP16Callback { get; set; }
}
