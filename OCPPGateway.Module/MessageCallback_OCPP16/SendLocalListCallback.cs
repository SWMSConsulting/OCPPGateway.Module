using DevExpress.ExpressApp;
using OCPPGateway.Module.BusinessObjects;
using OCPPGateway.Module.Services;

namespace OCPPGateway.Module.OCPPMessageCallback;

public class SendLocalListCallback : IMessageCallbackOCPP16
{
    public void OnMessageReceived(MessageReceivedEventArgs eventArgs, IObjectSpace objectSpace)
    {
        var chargeTags = objectSpace.GetObjects<OCPPChargeTag>();
    }
}
