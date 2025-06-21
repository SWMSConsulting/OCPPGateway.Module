﻿using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.ComponentModel;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Unknown Charge Tag")]
public class UnknownOCPPChargeTag: BaseObject
{
    public virtual string Identifier { get; set; }


    [ModelDefault("DisplayFormat", "{0:G}")]
    public virtual DateTime Timestamp { get; set; }

}
