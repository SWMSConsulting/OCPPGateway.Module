using OCPPGateway.Module.Models;

namespace OCPPGateway.Module.Services;

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
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

    private static string TopicSubscribeDataFromGateway => MqttTopicService.GetDataTopic("+", "+", true);
    private static string TopicSubscribeDataToGateway => MqttTopicService.GetDataTopic("+", "+", false);

    private string[] topicsToSubscribe => [
        TopicSubscribeDataFromGateway,
        TopicSubscribeDataToGateway
    ];

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

    #region OnDataFromGatewayReceived
    public async void OnDataFromGatewayReceived(DataReceivedEventArgs args)
    {
        Console.WriteLine($"Payload: {args.Payload}");

        if (args.Type == nameof(UnknownChargePoint))
        {
            HandleUnknownChargePoint(args.Payload);
            return;
        }

        if (args.Type == nameof(UnknownChargeTag))
        {
            HandleUnknownChargeTag(args.Payload);
            return;
        }

        if (args.Type == nameof(Transaction))
        {
            HandleTransaction(args.Payload);
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
        var chargeTag = JsonConvert.DeserializeObject<UnknownChargeTag>(payload);
        if (chargeTag == null)
        {
            _logger.LogError("Failed to deserialize UnknownChargeTag");
            return;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<UnknownOCPPChargeTag>();

            var existing = objectSpace.FindObject<UnknownOCPPChargeTag>(CriteriaOperator.Parse("Identifier = ?", chargeTag.TagId));
            if (existing == null)
            {
                existing = objectSpace.CreateObject<UnknownOCPPChargeTag>();
                existing.Identifier = chargeTag.TagId;
            }

            objectSpace.CommitChanges();
        }
    }

    public void HandleTransaction(string payload)
    {
        var transaction = JsonConvert.DeserializeObject<Transaction>(payload);
        if (transaction == null)
        {
            _logger.LogError("Failed to deserialize Transaction");
            return;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var type = OCPPTransaction.AssignableType;
            if (type == null)
            {
                _logger.LogError("AssignableType not found");
                return;
            }

            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace(OCPPTransaction.AssignableType);

            var chargePoint = objectSpace.FindObject<OCPPChargePoint>(CriteriaOperator.Parse("Identifier = ?", transaction.ChargePointId));
            if (chargePoint == null)
            {
                _logger.LogError("ChargePoint not found");
                return;
            }

            var connector = chargePoint.Connectors.FirstOrDefault(c => c.Identifier == transaction.ConnectorId);
            if (connector == null)
            {
                _logger.LogError("Connector not found");
                return;
            }

            var existingTransaction = connector.Transactions.FirstOrDefault(t => t.TransactionId == transaction.TransactionId);
            if (existingTransaction == null)
            {
                existingTransaction = (OCPPTransaction)objectSpace.CreateObject(type);
                existingTransaction.TransactionId = transaction.TransactionId;
                connector.Transactions.Add(existingTransaction);
            }

            existingTransaction.StartTime = transaction.StartTime;
            existingTransaction.StartMeter = transaction.MeterStart;
            existingTransaction.StartTagId = transaction.StartTagId ?? "";

            existingTransaction.StopTime = transaction.StopTime;
            existingTransaction.StopMeter = transaction.MeterStop;
            existingTransaction.StopTagId = transaction.StopTagId ?? "";
            existingTransaction.StopReason = transaction.StopReason ?? "";

            objectSpace.CommitChanges();
        }
    }
    #endregion

    #region OnDataToGatewayReceived
    public async void OnDataToGatewayReceived(DataReceivedEventArgs args)
    {
        Console.WriteLine($"Payload: {args.Payload}");

        if (args.Type == nameof(ChargePoint))
        {
            HandleChargePoint(args.Payload);
            return;
        }
        if (args.Type == nameof(ChargeTag))
        {
            HandleChargeTag(args.Payload);
            return;
        }
    }

    public void HandleChargePoint(string payload)
    {
        var chargePoint = JsonConvert.DeserializeObject<ChargePoint>(payload);
        if (chargePoint == null)
        {
            _logger.LogError("Failed to deserialize ChargePoint");
            return;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<OCPPChargePoint>();

            var existing = objectSpace.FindObject<OCPPChargePoint>(CriteriaOperator.Parse("Identifier = ?", chargePoint.ChargePointId));
            if (existing == null)
            {
                existing = (OCPPChargePoint)objectSpace.CreateObject(OCPPChargePoint.AssignableType);
                existing.Identifier = chargePoint.ChargePointId;
                existing.Name = chargePoint.Name;
            }

            objectSpace.CommitChanges();
        }
    }

    public void HandleChargeTag(string payload)
    {
        var chargeTag = JsonConvert.DeserializeObject<ChargeTag>(payload);
        if (chargeTag == null)
        {
            _logger.LogError("Failed to deserialize ChargeTag");
            return;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var type = OCPPChargeTag.AssignableType;

            var objectSpaceFactory = scope.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace(type);

            OCPPChargeTag? existing = objectSpace.FindObject(type, CriteriaOperator.Parse("Identifier = ?", chargeTag.TagId)) as OCPPChargeTag;
            if (existing == null)
            {
                existing = (OCPPChargeTag)objectSpace.CreateObject(type);
                existing.Identifier = chargeTag.TagId;
                existing.Name = chargeTag.TagName;
                existing.ExpiryDate = chargeTag.ExpiryDate;
                existing.Blocked = chargeTag.Blocked ?? false;
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
        
        if(MatchesWildcard(arg.ApplicationMessage.Topic, TopicSubscribeDataFromGateway))
        {
            OnDataFromGatewayReceived(new DataReceivedEventArgs
            {
                Type = decodedTopic["type"],
                Identifier = decodedTopic["identifier"],
                Payload = payload
            });
        }

        if (MatchesWildcard(arg.ApplicationMessage.Topic, TopicSubscribeDataToGateway))
        {
            OnDataToGatewayReceived(new DataReceivedEventArgs
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
