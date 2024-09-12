using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Unknown")]
public class UnknownAccessTag: BaseObject
{
    public virtual string Identifier { get; set; }
}
