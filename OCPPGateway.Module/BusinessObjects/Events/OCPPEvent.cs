using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using Microsoft.EntityFrameworkCore;

namespace OCPPGateway.Module.BusinessObjects.Events;

[NavigationItem("Events")]
[Microsoft.EntityFrameworkCore.Index(nameof(ChargePointIdentifier), nameof(Timestamp), IsUnique = false)]
public abstract class OCPPEvent: BaseObject
{
    public virtual string ChargePointIdentifier { get; set; }

    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm:ss}")]
    public virtual DateTime Timestamp { get; set; }

    public abstract string Category { get; }

    public abstract string Description { get; }
}
