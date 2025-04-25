using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using OCPPGateway.Module.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCPPGateway.Module.BusinessObjects;

[NavigationItem("OCPP")]
[Microsoft.EntityFrameworkCore.Index(nameof(TransactionId), IsUnique = false)]
public abstract class OCPPTransaction: BaseObject
{
    public virtual int TransactionId { get; set; }
    public virtual OCPPChargePointConnector ChargePointConnector { get; set; }
    
    public virtual string StartTagId { get; set; }

    [ModelDefault("DisplayFormat", "{0:G}")]
    public virtual DateTime StartTime { get; set; }

    [ModelDefault("DisplayFormat", "{0:n1}")]
    public virtual double StartMeter { get; set; }
    // public string StartResult { get; set; }

    public virtual string StopTagId { get; set; }

    [ModelDefault("DisplayFormat", "{0:G}")]
    public virtual DateTime? StopTime { get; set; }

    [ModelDefault("DisplayFormat", "{0:n1}")]
    public virtual double? StopMeter { get; set; }
    public virtual string StopReason { get; set; }


    [NotMapped]
    public bool IsStopped => StopTime.HasValue;

    [NotMapped]
    [Browsable(false)]
    public OCPPChargeTag? StartTag => ObjectSpace?.FindObject<OCPPChargeTag>(CriteriaOperator.Parse("Identifier == ?", StartTagId));

    [NotMapped]
    [Browsable(false)]
    public OCPPChargeTag? StopTag => ObjectSpace?.FindObject<OCPPChargeTag>(CriteriaOperator.Parse("Identifier == ?", StopTagId));


    [NotMapped]
    public TimeSpan? Duration => StopTime.HasValue ? StopTime.Value - StartTime : null;

    [NotMapped]
    [ModelDefault("DisplayFormat", "{0:n1}")]
    public double? Consumption => StopMeter.HasValue ? StopMeter - StartMeter : null;

    [Browsable(false)]
    public static Type? AssignableType
    {
        get
        {
            var type = typeof(OCPPTransaction);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t != type && type.IsAssignableFrom(t))
                .FirstOrDefault();
        }
    }

    public Transaction ToTransaction()
    {
        return new Transaction
        {
            TransactionId = TransactionId,
            ChargePointId = ChargePointConnector.ChargePoint.Identifier,
            ConnectorId = ChargePointConnector.Identifier,

            StartTime = StartTime,
            MeterStart = StartMeter,
            StartTagId = StartTagId,

            StopTime = StopTime,
            MeterStop = StopMeter,
            StopTagId = StopTagId,
            StopReason = StopReason,
        };
    }

}
