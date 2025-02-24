using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.ComponentModel;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Unknown Charge Tag")]
public class UnknownOCPPChargeTag: BaseObject
{
    public virtual string Identifier { get; set; }

    public virtual DateTime Timestamp { get; set; }

}
