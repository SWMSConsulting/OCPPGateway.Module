using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using OCPPGateway.Module.Messages_OCPP16;

namespace OCPPGateway.Module.NonPersistentObjects;

[DomainComponent]
public class SupportedRequest: NonPersistentBaseObject
{
    public string Name { get; set; }

    public Type RequestType { get; set; }

    public Type ResponseType { get; set; }

    public static IList<SupportedRequest> OCPP16SupportedRequests
    {
        get
        {
            return [
                new SupportedRequest
                {
                    Name = "RemoteStartTransaction",
                    RequestType = typeof(RemoteStartTransactionRequest),
                    ResponseType = typeof(RemoteStartTransactionResponse)
                },
                new SupportedRequest
                {
                    Name = "RemoteStopTransaction",
                    RequestType = typeof(RemoteStopTransactionRequest),
                    ResponseType = typeof(RemoteStopTransactionResponse)
                },
                new SupportedRequest
                {
                    Name = "ChangeConfiguration",
                    RequestType = typeof(ChangeConfigurationRequest),
                    ResponseType = typeof(ChangeConfigurationResponse)
                },
                new SupportedRequest
                {
                    Name = "SetChargingProfile",
                    RequestType = typeof(SetChargingProfileRequest),
                    ResponseType = typeof(SetChargingProfileResponse)
                },
                new SupportedRequest
                {
                    Name = "DataTransfer",
                    RequestType = typeof(DataTransferRequest),
                    ResponseType = typeof(DataTransferResponse)
                },
            ]; 
        }
    }
}
