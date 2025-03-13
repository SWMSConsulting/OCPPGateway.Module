using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using OCPPGateway.Module.BusinessObjects;
using OCPPGateway.Module.Extensions;
using OCPPGateway.Module.Messages_OCPP16;
using OCPPGateway.Module.Services;
using System.Linq;

namespace OCPPGateway.Module.OCPPMessageCallback;

public class SendLocalListCallback : IMessageCallbackOCPP16
{
    public void OnMessageReceived(MessageReceivedEventArgs args, IObjectSpace objectSpace, ILogger logger)
    {
        var type = typeof(OCPPChargeTag).GetImplementingTypes().FirstOrDefault();
        var chargeTags = objectSpace.GetObjects(type).Cast<OCPPChargeTag>().ToList();

        var validChargeTags = chargeTags.Select(t =>
        {
            var tagInfo = new IdTagInfo();
            if (t.ExpiryDate.HasValue)
            {
                tagInfo.ExpiryDate = new DateTimeOffset(t.ExpiryDate.Value);
            }

            if (!string.IsNullOrEmpty(t.ChargeTagGroup?.Identifier))
            {
                tagInfo.ParentIdTag = t.ChargeTagGroup?.Identifier;
            }

            tagInfo.Status = t.Blocked ? IdTagInfoStatus.Blocked : IdTagInfoStatus.Accepted;

            if (t.ExpiryDate < DateTime.Now)
            {
                tagInfo.Status = IdTagInfoStatus.Expired;
            }

            return new AuthorizationData
            {
                IdTag = t.Identifier,
                IdTagInfo = tagInfo
            };
        }).Where(t => t.IdTagInfo.Status == IdTagInfoStatus.Accepted).ToList();

        var localList = new SendLocalListRequest
        {
            ListVersion = 1,
            UpdateType = UpdateType.Full,
            LocalAuthorizationList = validChargeTags
        };

        var service = objectSpace.ServiceProvider.GetService(typeof(OcppGatewayMqttService)) as OcppGatewayMqttService;
        service?.Publish(localList, args.Identifier);
    }
}
