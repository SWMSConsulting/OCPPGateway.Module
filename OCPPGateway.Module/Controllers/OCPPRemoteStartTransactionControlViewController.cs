using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using OCPPGateway.Module.NonPersistentObjects;
using OCPPGateway.Module.Services;
using MQTTnet.Internal;

namespace OCPPGateway.Module.Controllers;

public class OCPPRemoteStartTransactionControlViewController : ObjectViewController<DetailView, OCPPRemoteStartTransactionControl>
{
    public OCPPRemoteStartTransactionControlViewController()
    {
        SimpleAction startTransactionAction = new SimpleAction(this, "StartTransactionAction", "StartTransaction")
        {
            Caption = "Start Transaction",
            ImageName = "",
        };
        startTransactionAction.Execute += StartTransactionAction_Execute;
    }

    public void StartTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var remoteControl = View.CurrentObject as OCPPRemoteStartTransactionControl;
        if (remoteControl == null) {
            return;
        }

        if(remoteControl.ChargePointConnector == null || remoteControl.ChargeTag == null)
        {
            var errorMessage = "Please select a valid Connector and Charge Tag";
            Application.ShowViewStrategy.ShowMessage(errorMessage, InformationType.Error);
            return;
        }

        var service = Application.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.RemoteStartTransaction(remoteControl.ChargePointConnector, remoteControl.ChargeTag).RunInBackground();

        var message = "Sending Remote Start Transaction";
        Application.ShowViewStrategy.ShowMessage(message, InformationType.Info);

    }
}