using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using Newtonsoft.Json;
using OCPPGateway.Module.BusinessObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCPPGateway.Module.NonPersistentObjects;

[DomainComponent]
[NavigationItem("OCPP Control")]
public abstract class OCPPRemoteControl : NonPersistentBaseObject
{
    public OCPPChargePoint ChargePoint { get; set; }

    //public OCPPChargePointConnector? Connector { get; set; }

    [DataSourceProperty("SupportedRequests")]
    public SupportedRequest Request { get; set; }


    [FieldSize(FieldSizeAttribute.Unlimited)]
    public string Payload { get; set; }

    [FieldSize(FieldSizeAttribute.Unlimited)]
    [Appearance("ResponseDisabled", Enabled = false)]
    [Appearance("ResponseHidden", Visibility = ViewItemVisibility.Hide)]
    public string Response { get; set; } = "";


    [VisibleInListView(false), VisibleInDetailView(false)]
    public abstract IList<SupportedRequest> SupportedRequests { get; }


    [VisibleInListView(false), VisibleInDetailView(false)]
    //[RuleFromBoolProperty("IsValidPayload", DefaultContexts.Save, "The payload does not match the selected request", UsedProperties = "Request,Payload")]
    public bool IsValidPayload
    {
        get
        {
            var type = Request?.RequestType;
            if (type == null)
            {
                return true;
            }
            if (string.IsNullOrEmpty(Payload))
            {
                return false;
            }
            try
            {
                var request = JsonConvert.DeserializeObject(Payload, type);
                return request != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}