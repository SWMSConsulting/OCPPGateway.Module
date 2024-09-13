﻿using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using MQTTnet.Internal;
using OCPPGateway.Module.Models;
using OCPPGateway.Module.Services;
using System.ComponentModel;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("Master Data")]
[DisplayName("Charge Tag")]
public abstract class OCPPChargeTag : BaseObject
{
    public override void OnSaving()
    {
        base.OnSaving();

        if(ObjectSpace.IsDeletedObject(this))
        {
            var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
            service?.ClearRetainFlag(typeof(ChargePoint), Identifier, true).RunInBackground();
        } else
        {
            Publish();
        }
    }


    [RuleRequiredField]
    public virtual string Identifier { get; set; }

    [RuleRequiredField]
    public virtual string Name { get; set; }

    public virtual DateTime? ExpiryDate { get; set; }

    public virtual bool Blocked { get; set; }


    [Browsable(false)]
    public abstract IChargeTagGroup? ChargeTagGroup { get; }


    #region OCPP related

    [Browsable(false)]
    public static Type? AssignableType
    {
        get
        {
            var type = typeof(OCPPChargeTag);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t != type && type.IsAssignableFrom(t))
                .FirstOrDefault();
        }
    }

    [Browsable(false)]
    public ChargeTag ChargeTag
    {
        get
        {
            return new ChargeTag
            {
                TagId = Identifier,
                TagName = Name,
                ExpiryDate = ExpiryDate,
                Blocked = Blocked,
                ParentTagId = ChargeTagGroup?.Identifier,
            };
        }
    }

    public void Publish()
    {
        var service = ObjectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.Publish(ChargeTag).RunInBackground();
    }
    #endregion
}
