using DevExpress.ExpressApp;
using OCPPGateway.Module.Services;

namespace OCPPGateway.Module.OCPPMessageCallback;

public interface IMessageCallbackOCPP16
{
    public abstract void OnMessageReceived(MessageReceivedEventArgs eventArgs, IObjectSpace objectSpace);
}
