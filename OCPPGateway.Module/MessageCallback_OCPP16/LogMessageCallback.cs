using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OCPPGateway.Module.BusinessObjects;
using OCPPGateway.Module.BusinessObjects.Events;
using OCPPGateway.Module.OCPPMessageCallback;
using OCPPGateway.Module.Services;

namespace OCPPGateway.Module.MessageCallback_OCPP16;

public class LogMessageCallback : IMessageCallbackOCPP16
{
    public void OnMessageReceived(MessageReceivedEventArgs eventArgs, IObjectSpace objectSpace, ILogger logger)
    {
        var message = JsonConvert.DeserializeObject<OCPPMessage>(eventArgs.Payload);
        if (message == null)
        {
            logger.LogInformation("Failed to parse message");
            return;
        }

        var chargePoint = objectSpace.FindObject<OCPPChargePoint>(CriteriaOperator.Parse($"Identifier = '{eventArgs.Identifier}'"));
        if (chargePoint == null)
        {
            logger.LogInformation($"ChargePoint with identifier {eventArgs.Identifier} not found");
            return;
        }

        var logEvent = objectSpace.CreateObject<OCPPMessageLogEvent>();
        logEvent.Timestamp = DateTime.Now;
        logEvent.ChargePoint = chargePoint;
        logEvent.Protocol = OCPPVersion.OCPP16.ToString();
        logEvent.MessageType = message.MessageType;
        logEvent.MessageAction = eventArgs.Action;
        logEvent.MessageId = message.UniqueId;
        logEvent.MessagePayload = eventArgs.Payload;

        chargePoint.Events.Add(logEvent);

        objectSpace.CommitChanges();
    }
}
