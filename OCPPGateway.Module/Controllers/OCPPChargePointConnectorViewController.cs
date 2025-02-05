using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using MQTTnet.Internal;
using OCPPGateway.Module.BusinessObjects;
using OCPPGateway.Module.NonPersistentObjects;
using OCPPGateway.Module.Services;
using System;

namespace OCPPGateway.Module.Controllers;

public class OCPPChargePointConnectorViewController : ObjectViewController<ObjectView, OCPPChargePointConnector>
{
    public OCPPChargePointConnectorViewController()
    {
        PopupWindowShowAction showPopUpAction = new PopupWindowShowAction(this, "Start Transaction", "View"){
            ImageName = "Next",
            TargetObjectsCriteria = "RemoteStartTransactionSupported",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        showPopUpAction.CustomizePopupWindowParams += showPopUpAction_CustomizePopupWindowParams;

        SimpleAction startTransactionAction = new SimpleAction(this, "StartTransactionAction", "StartTransaction")
        {
            Caption = "Start Transaction",
            ImageName = "",
        };
        startTransactionAction.Execute += StartTransactionAction_Execute;

        SimpleAction stopTransactionAction = new SimpleAction(this, "StopTransactionAction", PredefinedCategory.View)
        {
            Caption = "Stop Transaction",
            ImageName = "Stop",
            TargetObjectsCriteria = "RemoteStopTransactionSupported",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        stopTransactionAction.Execute += StopTransactionAction_Execute;

    }

    public void showPopUpAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var nonPersistentOS = Application.CreateObjectSpace(typeof(OCPPRemoteStartTransactionControl));
        OCPPRemoteStartTransactionControl remoteControl = nonPersistentOS.CreateObject<OCPPRemoteStartTransactionControl>();
        remoteControl.ChargePointConnector = ViewCurrentObject;

        nonPersistentOS.CommitChanges();
        DetailView detailView = Application.CreateDetailView(nonPersistentOS, remoteControl);
        detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
        e.View = detailView;
        e.DialogController.SaveOnAccept = false;
        e.DialogController.CancelAction.Active["NothingToCancel"] = false;
    }


    public void StartTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var remoteControl = View.CurrentObject as OCPPRemoteStartTransactionControl;
        if (remoteControl == null)
        {
            return;
        }

        if (remoteControl.ChargePointConnector == null || remoteControl.ChargeTag == null)
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

    public void StopTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {

        var connector = View.CurrentObject as OCPPChargePointConnector;
        if(connector == null)
        {
            return;
        }

        if (connector.ActiveTransaction == null)
        {
            return;
        }

        var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.RemoteStopTransaction(connector).RunInBackground();        
    }
}
