using DevExpress.ExpressApp.DC;

namespace OCPPGateway.Module.NonPersistentObjects;

[DomainComponent]
public class OCPP16RemoteControl : OCPPRemoteControl
{
    public override IList<SupportedRequest> SupportedRequests => SupportedRequest.OCPP16SupportedRequests;

}
