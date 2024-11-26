using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using MQTTnet.Internal;
using OCPPGateway.Module.Models;
using OCPPGateway.Module.Services;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Charge Tag")]
public abstract class OCPPChargeTag : BaseObject
{
    [RuleRequiredField]
    [StringLength(20)]
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
    #endregion
}
