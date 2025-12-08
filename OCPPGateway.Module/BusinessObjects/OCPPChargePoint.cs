using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using OCPPGateway.Module.BusinessObjects.Events;
using OCPPGateway.Module.Models;
using SWMS.Influx.Module.BusinessObjects;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[DisplayName("Charge Point")]
[Microsoft.EntityFrameworkCore.Index(nameof(Identifier), IsUnique = false)]
public abstract class OCPPChargePoint : AssetAdministrationShell
{
    public override bool UpdatePropertiesOnLoaded => false;

    public abstract OCPPProtocolVersion OCPPProtocolVersion { get; }

    public override string Caption => Name;

    [RuleRequiredField]
    [StringLength(20)]
    public virtual string Identifier { get; set; }

    [RuleRequiredField]
    public virtual string Name { get; set; }

    [Aggregated]
    public virtual IList<OCPPChargePointConnector> Connectors { get; set; } = new ObservableCollection<OCPPChargePointConnector>();

    [NotMapped]
    public int NumberOfConnectors => Connectors.Count;


    #region OCPP related
    [Browsable(false)]
    public static IEnumerable<Type> AssignableTypes
    {
        get
        {
            var type = typeof(OCPPChargePoint);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t != type && type.IsAssignableFrom(t));
        }
    }

    [Browsable(false)]
    public static Type? AssignableType => AssignableTypes.FirstOrDefault();

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