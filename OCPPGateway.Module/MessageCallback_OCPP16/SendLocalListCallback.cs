using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using OCPPGateway.Module.BusinessObjects;
using OCPPGateway.Module.Messages_OCPP16;
using OCPPGateway.Module.Services;

namespace OCPPGateway.Module.OCPPMessageCallback;

public class SendLocalListCallback : IMessageCallbackOCPP16
{
    public void OnMessageReceived(MessageReceivedEventArgs eventArgs, IObjectSpace objectSpace, ILogger logger)
    {
        var chargeTags = objectSpace.GetObjects<OCPPChargeTag>();

        var localList = new SendLocalListRequest
        {
            ListVersion = 1,
            UpdateType = UpdateType.Full,
            LocalAuthorizationList = chargeTags.Select(t =>
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

                if(t.ExpiryDate < DateTime.Now)
                {
                    tagInfo.Status = IdTagInfoStatus.Expired;
                }

                return new AuthorizationData
                {
                    IdTag = t.Identifier,
                    IdTagInfo = tagInfo
                };
            }).ToList()
        };
    }
}
