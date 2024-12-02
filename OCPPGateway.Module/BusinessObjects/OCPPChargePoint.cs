using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using MQTTnet.Internal;
using OCPPGateway.Module.BusinessObjects.Events;
using OCPPGateway.Module.Models;
using OCPPGateway.Module.Services;
using SWMS.Influx.Module.Attributes;
using SWMS.Influx.Module.BusinessObjects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Charge Point")]
public abstract class OCPPChargePoint : AssetAdministrationShell
{
    public abstract OCPPProtocolVersion OCPPProtocolVersion { get; }

    public override string Caption => Name;

    [RuleRequiredField]
    [StringLength(20)]
    public virtual string Identifier { get; set; }

    [RuleRequiredField]
    public virtual string Name { get; set; }

    [Aggregated]
    public virtual IList<OCPPChargePointConnector> Connectors { get; set; } = new ObservableCollection<OCPPChargePointConnector>();

    [Aggregated]
    [Appearance("EventsDisabled", Enabled = false)]
    public virtual IList<OCPPEvent> Events { get; set; } = new ObservableCollection<OCPPEvent>();

    [NotMapped]
    public int NumberOfConnectors => Connectors.Count;

    [NotMapped]
    [LastDatapoint("is_online", "heartbeat")]
    [Appearance("LastHeartbeatDisabled", Enabled = false)]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm:ss}")]
    public DateTime? LastHeartbeat { get; set; }


    #region OCPP related

    [Browsable(false)]
    public static Type? AssignableType
    {
        get
        {
            var type = typeof(OCPPChargePoint);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t != type && type.IsAssignableFrom(t))
                .FirstOrDefault();
        }
    }

    [Browsable(false)]
    public ChargePoint ChargePoint
    {
        get
        {
            return new ChargePoint
            {
                ChargePointId = Identifier,
                Name = Name,
                NumberOfConnectors = NumberOfConnectors,
            };
        }
    }
    #endregion
}