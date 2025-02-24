using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using InfluxDB.Client.Api.Domain;
using OCPPGateway.Module.BusinessObjects;
using System;

namespace OCPPGateway.Module.Controllers;

public class UnknownOCPPChargePointViewController : ObjectViewController<ObjectView, UnknownOCPPChargePoint>
{
    public UnknownOCPPChargePointViewController()
    {
        PopupWindowShowAction showPopUpAction = new PopupWindowShowAction(this, "Add to Charge Points", "View")
        {
            ImageName = "Actions_Add",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        showPopUpAction.CustomizePopupWindowParams += showPopUpAction_CustomizePopupWindowParams;        
    }
    public void showPopUpAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        if(OCPPChargePoint.AssignableTypes.Count() > 1)
        {
            throw new UserFriendlyException("There are multiple classes implementing OCPPChargePoint. You need to manually add the charge point.");
        }

        var type = OCPPChargePoint.AssignableType;
        if (type == null)
        {
            return;
        }

        var unknown = View.CurrentObject as UnknownOCPPChargePoint;
        if(unknown == null || string.IsNullOrEmpty(unknown.Identifier))
        {
            return;
        }

        var identifier = unknown.Identifier;

        var existingChargePoint = (OCPPChargePoint)ObjectSpace.FindObject(type, CriteriaOperator.Parse("Identifier = ?", identifier));

        if (existingChargePoint == null)
        {
            existingChargePoint = (OCPPChargePoint)ObjectSpace.CreateObject(type);
            existingChargePoint.Identifier = identifier;
            existingChargePoint.Name = identifier;
        }

        ObjectSpace.Delete(unknown);
        ObjectSpace.CommitChanges();
        ObjectSpace.Refresh();

        var newObjectSpace = Application.CreateObjectSpace<OCPPChargePoint>();
        var chargeTag = newObjectSpace.FindObject(typeof(OCPPChargePoint), CriteriaOperator.Parse("Identifier = ?", identifier));
        DetailView detailView = Application.CreateDetailView(newObjectSpace, chargeTag);
        detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
        e.View = detailView;
    }
}
