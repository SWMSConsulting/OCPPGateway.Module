using DevExpress.ExpressApp.DC;

namespace OCPPGateway.Module.BusinessObjects.Events;

public class OCPPMessageLogEvent : OCPPEvent
{
    public virtual string Protocol { get; set; }

    public virtual string MessageType { get; set; }

    public virtual string MessageId { get; set; }

    public virtual string MessageAction { get; set; }

    [FieldSize(FieldSizeAttribute.Unlimited)]
    public virtual string MessagePayload { get; set; }


    #region OCPPEvent
    public override string Category => "OCPPMessageLog";

    public override string Description => $"Received {MessageAction} message of type {MessageType} for {ChargePoint.Identifier}";
    #endregion
}
