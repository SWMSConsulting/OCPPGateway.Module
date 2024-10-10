using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OCPPGateway.Module.OCPPMessageCallback;
using DevExpress.ExpressApp;
using OCPPGateway.Module.BusinessObjects;
using System.Linq;
using DevExpress.ExpressApp.Core;
using OCPPGateway.Module.Extensions;

namespace OCPPGateway.Module.Services;

public class OcppMessageCallbackService
{

    #region setup
    public readonly ILogger<OcppGatewayMqttService> _logger;

    public readonly IServiceScopeFactory _serviceScopeFactory;


    public OcppMessageCallbackService(
            ILogger<OcppGatewayMqttService> logger,
            IServiceScopeFactory serviceScopeFactory,
            OcppGatewayMqttService ocppGatewayMqttService
        )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        ocppGatewayMqttService.OCPPMessageReceived += OnOCPPMessageReceived;
    }

    public void OnOCPPMessageReceived(object? sender, MessageReceivedEventArgs args)
    {
        // get 
        if(string.IsNullOrEmpty(args.Payload))
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var objectSpaceFactory = scope.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<OCPPMessageCallbackLink>();

        var actionCallbackLinks = objectSpace.GetObjects<OCPPMessageCallbackLink>()
            .Where(l => l.Action == args.Action && (l.FromChargePoint == null || l.FromChargePoint == args.FromChargePoint))
            .ToList();
        
        actionCallbackLinks.ForEach(callbackLink =>
        {
            var implementingType = typeof(IMessageCallbackOCPP16)
                .GetImplementingTypes()
                .FirstOrDefault(t => t.Name == callbackLink.OCPP16Callback);
            if(implementingType != null) {
                var messageCallback = Activator.CreateInstance(implementingType) as IMessageCallbackOCPP16;
                messageCallback?.OnMessageReceived(args, objectSpace);
            }
        });

    }
    #endregion
}
