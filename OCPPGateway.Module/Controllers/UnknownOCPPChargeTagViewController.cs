using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using InfluxDB.Client.Api.Domain;
using OCPPGateway.Module.BusinessObjects;
using System;

namespace OCPPGateway.Module.Controllers;

public class UnknownOCPPChargeTagViewController : ObjectViewController<ObjectView, UnknownOCPPChargeTag>
{
    public UnknownOCPPChargeTagViewController()
    {
        PopupWindowShowAction showPopUpAction = new PopupWindowShowAction(this, "Add to Charge Tags", "View")
        {
            ImageName = "Actions_Add",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        showPopUpAction.CustomizePopupWindowParams += showPopUpAction_CustomizePopupWindowParams;        
    }
    public void showPopUpAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        if (OCPPChargeTag.AssignableTypes.Count() > 1)
        {
            throw new UserFriendlyException("There are multiple classes implementing OCPPChargeTag. You need to manually add the charge tag.");
        }

        var type = OCPPChargeTag.AssignableType;
        if (type == null || ObjectSpace == null)
        {
            return;
        }
        var unknown = View.CurrentObject as UnknownOCPPChargeTag;
        if(unknown == null || string.IsNullOrEmpty(unknown.Identifier))
        {
            return;
        }

        var identifier = unknown.Identifier;

        var existingChargeTag = (OCPPChargeTag)ObjectSpace.FindObject(type, CriteriaOperator.Parse("Identifier = ?", identifier));

        if (existingChargeTag == null)
        {
            existingChargeTag = (OCPPChargeTag)ObjectSpace.CreateObject(type);
            existingChargeTag.Identifier = identifier;
            existingChargeTag.Name = identifier;
            existingChargeTag.Blocked = true;
        }

        ObjectSpace.Delete(unknown);
        ObjectSpace.CommitChanges();
        ObjectSpace.Refresh();

        var newObjectSpace = Application.CreateObjectSpace<OCPPChargeTag>();
        var chargeTag = newObjectSpace.FindObject(typeof(OCPPChargeTag), CriteriaOperator.Parse("Identifier = ?", identifier));
        DetailView detailView = Application.CreateDetailView(newObjectSpace, chargeTag);
        detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
        e.View = detailView;
    }
}
