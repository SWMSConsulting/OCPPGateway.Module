using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using OCPPGateway.Module.NonPersistentObjects;
using OCPPGateway.Module.Services;
using MQTTnet.Internal;
using Newtonsoft.Json;

namespace OCPPGateway.Module.Controllers;

public class OCPPRemoteControlDetailViewController : ObjectViewController<DetailView, OCPPRemoteControl>
{

    private OcppGatewayMqttService? mqttService;

    public OCPPRemoteControlDetailViewController()
    {
        TargetObjectType = typeof(OCPPRemoteControl);
        SimpleAction action = new SimpleAction(this, "RemoteControlSendAction", DevExpress.Persistent.Base.PredefinedCategory.View)
        {
            Caption = "Send",
            ImageName = "Actions_Send",
        };
        action.Execute += RemoteControlSendAction_Execute;

    }
    protected override void OnActivated()
    {
        base.OnActivated();
        if ((ObjectSpace is NonPersistentObjectSpace) && (View.CurrentObject == null))
        {
            View.CurrentObject = ObjectSpace.CreateObject(View.ObjectTypeInfo.Type);
            View.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ObjectSpace.RemoveFromModifiedObjects(View.CurrentObject);
        }

        mqttService = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        if (mqttService != null)
        {
            mqttService.DataToGatewayReceived += MqttService_OnDataSend;
            mqttService.DataFromGatewayReceived += MqttService_OnDataReceived;
        }
    }
    private void RemoteControlSendAction_Execute(Object sender, SimpleActionExecuteEventArgs e)
    {
        var control = View.CurrentObject as OCPPRemoteControl;
        if (control == null)
        {
            return;
        }

        control.Response = "";

        if (control.ChargePoint == null)
        {
            Toast("ChargePoint is required", InformationType.Warning);
            return;
        }

        if (control.IsValidPayload == false)
        {
            Toast("Payload does not match the request", InformationType.Warning);
            return;
        }

        var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.Publish(control).RunInBackground();
    }

    private void MqttService_OnDataSend(object? sender, DataReceivedEventArgs args)
    {
        var message = $"{args.Type} send to {args.Identifier}";
        Toast(message, InformationType.Info);
    }

    private void MqttService_OnDataReceived(object? sender, DataReceivedEventArgs args)
    {

        var control = View.CurrentObject as OCPPRemoteControl;
        if (control == null)
        {
            return;
        }

        var responseType = control.Request.ResponseType;
        try
        {
            var message = $"{args.Type} received from {args.Identifier}: {args.Payload}";
            Toast(message, InformationType.Success);
            var response = JsonConvert.DeserializeObject(args.Payload, responseType);
            control.Response = args.Payload;
        }
        catch (JsonException e)
        {
            var message = $"Received invalid response for {args.Type} from {args.Identifier}";
            Toast(message, InformationType.Warning);
            control.Response = "";
            return;
        }
    }

    private void Toast(string message, InformationType type)
    {
        MessageOptions options = new MessageOptions();
        options.Duration = 4000;
        options.Message = message;
        options.Type = type;
        Application.ShowViewStrategy.ShowMessage(options);
    }
}
