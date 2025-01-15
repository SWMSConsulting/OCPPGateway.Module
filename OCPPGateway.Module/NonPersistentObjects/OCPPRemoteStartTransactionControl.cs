using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using OCPPGateway.Module.BusinessObjects;

namespace OCPPGateway.Module.NonPersistentObjects;

[DomainComponent]
[DefaultClassOptions]
[NavigationItem("OCPP Control")]
public class OCPPRemoteStartTransactionControl : NonPersistentBaseObject
{
    public OCPPChargePointConnector ChargePointConnector { get; set; }
    public OCPPChargeTag ChargeTag { get; set; }
}
