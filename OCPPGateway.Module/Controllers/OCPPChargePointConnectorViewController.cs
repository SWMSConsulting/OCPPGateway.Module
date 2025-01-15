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
            TargetObjectsCriteria = "ActiveTransaction == null",
        };
        showPopUpAction.CustomizePopupWindowParams += showPopUpAction_CustomizePopupWindowParams;

        SimpleAction stopTransactionAction = new SimpleAction(this, "StopTransactionAction", PredefinedCategory.View)
        {
            Caption = "Stop Transaction",
            ImageName = "",
            TargetObjectsCriteria = "ActiveTransaction != null"
        };
        stopTransactionAction.Execute += stopTransactionAction_Execute;
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

    public void stopTransactionAction_Execute(object sender, SimpleActionExecuteEventArgs e)
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
