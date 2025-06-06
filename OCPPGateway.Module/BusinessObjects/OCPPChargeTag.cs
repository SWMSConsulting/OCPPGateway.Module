﻿using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using OCPPGateway.Module.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Charge Tag")]
[Microsoft.EntityFrameworkCore.Index(nameof(Identifier), IsUnique = false)]
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
    public static IEnumerable<Type> AssignableTypes
    {
        get
        {
            var type = typeof(OCPPChargeTag);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t != type && type.IsAssignableFrom(t));
        }
    }

    [Browsable(false)]
    public static Type? AssignableType => AssignableTypes.FirstOrDefault();

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
