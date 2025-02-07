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
    OCPPRemoteStartTransactionControl? RemoteControl = null;

    public OCPPChargePointConnectorViewController()
    {
        PopupWindowShowAction showPopUpAction = new PopupWindowShowAction(this, "Start Transaction", "View"){
            ImageName = "Next",
            TargetObjectsCriteria = "RemoteStartTransactionSupported",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        showPopUpAction.CustomizePopupWindowParams += showPopUpAction_CustomizePopupWindowParams;

        SimpleAction stopTransactionAction = new SimpleAction(this, "StopTransactionAction", PredefinedCategory.View)
        {
            Caption = "Stop Transaction",
            ImageName = "Stop",
            ConfirmationMessage = "Do you really want to stop the transaction?",
            TargetObjectsCriteria = "RemoteStopTransactionSupported",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        stopTransactionAction.Execute += StopTransactionAction_Execute;
    }

    public void showPopUpAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var nonPersistentOS = Application.CreateObjectSpace(typeof(OCPPRemoteStartTransactionControl));
        RemoteControl = nonPersistentOS.CreateObject<OCPPRemoteStartTransactionControl>();
        RemoteControl.ChargePointConnector = ViewCurrentObject;

        nonPersistentOS.CommitChanges();
        DetailView detailView = Application.CreateDetailView(nonPersistentOS, RemoteControl);
        detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
        e.View = detailView;
        e.DialogController.SaveOnAccept = false;
        e.DialogController.AcceptAction.Caption = "Start Transaction";
        e.DialogController.AcceptAction.Execute += StartTransactionAction_Execute;
    }

    public void StartTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if(RemoteControl == null)
        {
            return;
        }

        if (RemoteControl.ChargePointConnector == null || RemoteControl.ChargeTag == null)
        {
            var errorMessage = "Please select a valid Connector and Charge Tag";
            Application.ShowViewStrategy.ShowMessage(errorMessage, InformationType.Error);
            return;
        }

        var service = Application.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.RemoteStartTransaction(RemoteControl.ChargePointConnector, RemoteControl.ChargeTag).RunInBackground();

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
