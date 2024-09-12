namespace OCPPGateway.Module.Services;

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using OCPPGateway.Module.BusinessObjects;
using OCPPGateway.Module.Models;
using System;
using System.Text;

public enum OCPPVersion
{
    OCPP16,
    OCPP20
}
public class DataReceivedEventArgs : EventArgs
{
    public string Type { get; set; }
    public string Identifier { get; set; }
    public string Payload { get; set; }
}
public class OcppGatewayMqttService
{
    private IMqttClient mqttClient;
    private MqttClientOptions options;
    private ILogger<OcppGatewayMqttService> _logger;

    // private string TopicSubscribeOcpp16 => MqttTopicService.GetOcppTopic(CommunicationProtocol.OCPP16, "+", "+", true, "+");
    // private string TopicSubscribeOcpp20 => MqttTopicService.GetOcppTopic(CommunicationProtocol.OCPP20, "+", "+", true, "+");
    private string TopicSubscribeData => MqttTopicService.GetDataTopic("+", "+", true);
    private string[] topicsToSubscribe => [TopicSubscribeData];

    private readonly IServiceScopeFactory _serviceScopeFactory;


    #region setup
    public OcppGatewayMqttService(
        ILogger<OcppGatewayMqttService> logger,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        // Create a new MQTT client.
        var factory = new MqttFactory();
        mqttClient = factory.CreateMqttClient();

        var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST");
        var mqttPort = Environment.GetEnvironmentVariable("MQTT_PORT");
        var mqttClientId = Environment.GetEnvironmentVariable("MQTT_CLIENT_ID");
        var mqttUser = Environment.GetEnvironmentVariable("MQTT_USER");
        var mqttPassword = Environment.GetEnvironmentVariable("MQTT_PASSWORD");

        // mqttUser and mqttPassword can be empty
        if (mqttHost == null || mqttPort == null || mqttClientId == null)
        {
            _logger.LogWarning("MQTT ENVIRONMENT VARIABLE IS MISSING");
            return;
        }
        _logger.LogInformation($"MQTT SERVICE SETUP {mqttHost}, {mqttPort}, {mqttClientId}");
        options = new MqttClientOptionsBuilder()
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithClientId(mqttClientId)
            .WithTcpServer(mqttHost, Convert.ToInt32(mqttPort))
            .WithCredentials(mqttUser, mqttPassword)
            //.WithTls()
            .WithCleanSession()
            .Build();

        OnConfiguring();
    }
    #endregion

    #region OnDataReceived
    public async void OnDataReceived(DataReceivedEventArgs args)
    {
        Console.WriteLine($"Payload: {args.Payload}");

        if(args.Type == nameof(UnknownChargePoint))
        {
            HandleUnknownChargePoint(args.Payload);
            return;
        }
        if (args.Type == nameof(UnknownChargeTag))
        {
            HandleUnknownChargeTag(args.Payload);
            return;
        }

    }

    public void HandleUnknownChargePoint(string payload)
    {
        var chargePoint = JsonConvert.DeserializeObject<UnknownChargePoint>(payload);
        if(chargePoint == null)
        {
            _logger.LogError("Failed to deserialize UnknownChargePoint");
            return;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<UnknownOCPPChargePoint>();

            var existing = objectSpace.FindObject<UnknownOCPPChargePoint>(CriteriaOperator.Parse("Identifier = ?", chargePoint.ChargePointId));
            if (existing == null)
            {
                existing = objectSpace.CreateObject<UnknownOCPPChargePoint>();
                existing.Identifier = chargePoint.ChargePointId;
            }

            objectSpace.CommitChanges();
        }
    }

    public void HandleUnknownChargeTag(string payload)
    {
        var chargePoint = JsonConvert.DeserializeObject<UnknownChargeTag>(payload);
        if (chargePoint == null)
        {
            _logger.LogError("Failed to deserialize UnknownChargeTag");
            return;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<UnknownOCPPChargeTag>();

            var existing = objectSpace.FindObject<UnknownOCPPChargeTag>(CriteriaOperator.Parse("Identifier = ?", chargePoint.TagId));
            if (existing == null)
            {
                existing = objectSpace.CreateObject<UnknownOCPPChargeTag>();
                existing.Identifier = chargePoint.TagId;
            }

            objectSpace.CommitChanges();
        }
    }
    #endregion

    #region publish
    public async Task Publish(ChargePoint chargePoint)
    {
        var payload = JsonConvert.SerializeObject(chargePoint);
        var topic = MqttTopicService.GetDataTopic(nameof(ChargePoint), chargePoint.ChargePointId, false);
        await PublishStringAsync(topic, payload, true);
    }

    public async Task Publish(ChargeTag chargeTag)
    {
        var payload = JsonConvert.SerializeObject(chargeTag);
        var topic = MqttTopicService.GetDataTopic(nameof(ChargeTag), chargeTag.TagId, false);
        await PublishStringAsync(topic, payload, true);
    }

    public async Task ClearRetainFlag(Type type, string identifier, bool fromGateway)
    {
        var topic = MqttTopicService.GetDataTopic(type.Name, identifier, fromGateway);
        await PublishStringAsync(topic, "", true);
    }
    #endregion

    #region MQTT related functions
    protected async void OnConfiguring()
    {
        await ConnectMqtt();
        _logger.LogInformation("MQTT SERVICE SETUP END");
    }

    private async Task ConnectMqtt()
    {
        mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;

        mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;

        mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

        await mqttClient.ConnectAsync(options, CancellationToken.None);
    }

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var payload = arg.ApplicationMessage.ConvertPayloadToString();

        if (string.IsNullOrEmpty(payload))
        {
            return;
        }

        var decodedTopic = MqttTopicService.DecodeTopic(arg.ApplicationMessage.Topic);
        
        if(MatchesWildcard(arg.ApplicationMessage.Topic, TopicSubscribeData))
        {
            OnDataReceived(new DataReceivedEventArgs
            {
                Type = decodedTopic["type"],
                Identifier = decodedTopic["identifier"],
                Payload = payload
            });
        }

    }

    private async Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        _logger.LogWarning("DISCONNECTED FROM SERVER");
        await Task.Delay(TimeSpan.FromSeconds(5));

        try
        {
            await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
        }
        catch
        {
            _logger.LogError("RECONNECTING FAILED");
        }
    }

    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _logger.LogInformation("CONNECTED TO SERVER");

        if (mqttClient.IsConnected)
        {
            foreach (var topic in topicsToSubscribe)
            {
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            }
        }
        _logger.LogInformation("SUBCRIPTIONS SUCCESSFULL");
    }

    public static bool MatchesWildcard(string topic, string wildcardTopic)
    {
        return MqttTopicFilterComparer.Compare(topic, wildcardTopic) == MqttTopicFilterCompareResult.IsMatch;
    }

    public async Task PublishStringAsync(string topic, string? message, bool retain = false, string correlationData = "")
    {
        if (message == null)
            return;

        while (!mqttClient.IsConnected)
        {
            Thread.Sleep(1000);
        }
        _logger.LogInformation(message);

        var mqttMessage = new MqttApplicationMessage()
        {
            Topic = topic,
            PayloadSegment = Encoding.ASCII.GetBytes(message),
            QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce,
            Retain = retain,
            CorrelationData = Encoding.ASCII.GetBytes(correlationData)
        };

        await mqttClient.PublishAsync(mqttMessage);
    }
    #endregion
}
