using OCPPGateway.Module.Models;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using OCPPGateway.Module.BusinessObjects;
using OCPPGateway.Module.Messages_OCPP16;
using System.Text;
using OCPPGateway.Module.NonPersistentObjects;
using DevExpress.ExpressApp;
using MQTTnet.Internal;
using DevExpress.ExpressApp.Security;

namespace OCPPGateway.Module.Services;

public enum OCPPVersion
{
    OCPP16,
    OCPP20
}
public class MessageReceivedEventArgs : EventArgs
{
    public string Action { get; set; }
    public string Identifier { get; set; }
    public bool FromChargePoint { get; set; }
    public string Payload { get; set; }
    public string CorrelationData { get; set; }
}

public class OcppGatewayMqttService
{
    public static readonly string VendorId = "SWMS";

    // this should be overridden in the derived class, because this is project specific
    public virtual void OnDataTransferReceived(MessageReceivedEventArgs args) { }
    public virtual void OnTransactionUpdated(OCPPTransaction transaction) { }
    public virtual void OnChargePointConnectorUpdated(OCPPChargePointConnector connector) { }

    public EventHandler<MessageReceivedEventArgs> DataFromGatewayReceived;
    public EventHandler<MessageReceivedEventArgs> DataToGatewayReceived;

    public EventHandler<MessageReceivedEventArgs> OCPPMessageReceived;

    private IMqttClient mqttClient;
    private MqttClientOptions options;
    public readonly ILogger<OcppGatewayMqttService> _logger;

    private static string TopicSubscribeDataFromChargePoint => MqttTopicService.GetDataTopic("+", "+", true);
    private static string TopicSubscribeDataToChargePoint => MqttTopicService.GetDataTopic("+", "+", false);


    private static string TopicSubscribeOcpp16FromChargePoint => MqttTopicService.GetOcppTopic(OCPPVersion.OCPP16, "+", "+", true);
    private static string TopicSubscribeOcpp16ToChargePoint => MqttTopicService.GetOcppTopic(OCPPVersion.OCPP16, "+", "+", false);


    private string[] topicsToSubscribe => [
        TopicSubscribeDataFromChargePoint,
        TopicSubscribeDataToChargePoint,

        TopicSubscribeOcpp16FromChargePoint,
        TopicSubscribeOcpp16ToChargePoint
    ];

    public readonly IServiceScopeFactory _serviceScopeFactory;

    public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
    };

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

    #region RemoteControl
    public async Task RemoteStartTransaction(OCPPChargePointConnector connector, OCPPChargeTag chargeTag)
    {
        var transaction = connector.ActiveTransaction;
        if (transaction != null)
        {
            return;
        }

        var request = new RemoteStartTransactionRequest
        {
            connectorId = connector.Identifier,
            idTag = chargeTag.Identifier
        };

        var payload = Serialize(request);
        var chargePoint = connector.ChargePoint.Identifier;
        var topic = MqttTopicService.GetDataTopic("RemoteStartTransaction", chargePoint, false);

        await PublishStringAsync(topic, payload, false);
    }

    public async Task RemoteStopTransaction(OCPPChargePointConnector connector)
    {
        var transaction = connector.ActiveTransaction;
        if (transaction == null || transaction.StopTime.HasValue)
        {
            return;
        }

        var request = new RemoteStopTransactionRequest
        {
            transactionId = transaction.TransactionId
        };

        var payload = Serialize(request);
        var chargePoint = connector.ChargePoint.Identifier;
        var topic = MqttTopicService.GetDataTopic("RemoteStopTransaction", chargePoint, false);

        await PublishStringAsync(topic, payload, false);
    }

    #endregion

    #region OnDataFromGatewayReceived
    public async Task OnDataFromGatewayReceived(MessageReceivedEventArgs args)
    {
        DataFromGatewayReceived?.Invoke(this, args);

        if (args.Action == nameof(UnknownChargePoint))
        {
            HandleUnknownChargePoint(args.Payload);
            return;
        }

        if (args.Action == nameof(UnknownChargeTag))
        {
            HandleUnknownChargeTag(args.Payload);
            return;
        }

        if (args.Action == nameof(Transaction))
        {
            await HandleTransaction(args.Payload, args.CorrelationData);
            return;
        }

        if (args.Action == nameof(ConnectorStatus))
        {
            HandleConnectorStatus(args.Payload);
            return;
        }

        if (args.Action == "DataTransfer")
        {
            OnDataTransferReceived(args);
            return;
        }

        if (args.Action == nameof(ChargePoint))
        {
            await HandleChargePoint(args.Identifier, args.CorrelationData);
            return;
        }

        if (args.Action == nameof(ChargeTag))
        {
            await HandleChargeTag(args.Identifier, args.CorrelationData);
            return;
        }
    }

    public void HandleUnknownChargePoint(string payload)
    {
        var chargePoint = JsonConvert.DeserializeObject<UnknownChargePoint>(payload, JsonSerializerSettings);
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
        var chargeTag = JsonConvert.DeserializeObject<UnknownChargeTag>(payload, JsonSerializerSettings);
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
            existing.ChargePointIdentifier = chargeTag.ChargePointIdentifier ?? "";
            existing.Timestamp = DateTime.Now;

            objectSpace.CommitChanges();
        }
    }

    public async Task HandleTransaction(string payload, string correlationData)
    {
        var transaction = JsonConvert.DeserializeObject<Transaction>(payload, JsonSerializerSettings);
        if (transaction == null)
        {
            _logger.LogError("Failed to deserialize Transaction");
            return;
        }
        transaction.StartTime = transaction.StartTime.ToLocalTime();
        transaction.StopTime = transaction.StopTime?.ToLocalTime();

        using var scope = _serviceScopeFactory.CreateScope();
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


        // requesting transactions
        if (transaction.TransactionId == null)
        {
            List<Transaction> openTransactions = chargePoint.Connectors
                .SelectMany(c => c.Transactions)
                .Where(t => !t.StopTime.HasValue)
                .Select(c => c.ToTransaction())
                .ToList() ?? [];
            if(transaction.ConnectorId > 0)
            {
                openTransactions = openTransactions.Where(t => t.ConnectorId == transaction.ConnectorId).ToList();
            }

            await Publish(openTransactions, transaction.ChargePointId, correlationData);
            return;
        }

        var connector = chargePoint.Connectors.FirstOrDefault(c => c.Identifier == transaction.ConnectorId);
        if (connector == null)
        {
            _logger.LogError("Connector not found");
            return;
        }

        var unstoppedTransaction = connector.Transactions.FirstOrDefault(t => t.TransactionId != transaction.TransactionId && !t.StopTime.HasValue && t.StartTime < transaction.StartTime);
        if (unstoppedTransaction != null)
        {
            unstoppedTransaction.StopMeter = transaction.MeterStart;
            unstoppedTransaction.StopTime = transaction.StartTime;
            unstoppedTransaction.StopTagId = unstoppedTransaction.StartTagId ?? "";
            unstoppedTransaction.StopReason = "Another transaction started";
        }

        var existingTransaction = connector.Transactions.FirstOrDefault(t => t.TransactionId == transaction.TransactionId && !t.StopTime.HasValue);
        if (existingTransaction == null)
        {
            existingTransaction = (OCPPTransaction)objectSpace.CreateObject(type);
            existingTransaction.TransactionId = transaction.TransactionId ?? 0;
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

        OnTransactionUpdated(existingTransaction);
    }

    public void HandleConnectorStatus(string payload)
    {
        var status = JsonConvert.DeserializeObject<ConnectorStatus>(payload, JsonSerializerSettings);
        if (status == null)
        {
            _logger.LogError("Failed to deserialize ConnectorStatus");
            return;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<OCPPChargePointConnector>();

            var chargePoint = objectSpace.FindObject<OCPPChargePoint>(CriteriaOperator.Parse("Identifier = ?", status.ChargePointId));
            if (chargePoint == null)
            {
                _logger.LogError("ChargePoint not found: {ChargePointId}", [status.ChargePointId]);
                return;
            }

            var connector = chargePoint.Connectors.FirstOrDefault(c => c.Identifier == status.ConnectorId);
            if (connector == null)
            {
                _logger.LogError("Connector not found {ConnectorId} for {ChargePointId}", [status.ConnectorId, status.ChargePointId]);
                return;
            }

            connector.LastStatus = status.LastStatus;
            connector.LastConsumption = status.LastConsumption;
            connector.LastMeter = status.LastMeter;
            connector.LastStateOfCharge = status.StateOfCharge;

            objectSpace.CommitChanges();

            OnChargePointConnectorUpdated(connector);
        }
    }
    
    public async Task HandleChargePoint(string identifier, string correlationData)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
        var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<OCPPChargePoint>();

        if(identifier == "all")
        {
            var chargePoints = objectSpace?.GetObjects<OCPPChargePoint>().Select(p => p.ChargePoint).ToList();

            if(chargePoints != null)
            {
                await Publish(chargePoints, identifier, correlationData);
                return;
            }
        }

        var chargePoint = objectSpace?.FindObject<OCPPChargePoint>(CriteriaOperator.Parse("Identifier = ?", identifier));
        if (chargePoint != null)
        {
            await Publish(chargePoint.ChargePoint, correlationData);
            return;
        }
        await PublishNull(nameof(ChargePoint), identifier, correlationData);
    }
    
    public async Task HandleChargeTag(string identifier, string correlationData)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var objectSpaceFactory = scope.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<OCPPChargeTag>();

        if (identifier == "all")
        {
            var chargeTags = objectSpace.GetObjects<OCPPChargeTag>().Select(t => t.ChargeTag).ToList();

            if (chargeTags != null)
            {
                await Publish(chargeTags, identifier, correlationData);
                return;
            }
        }

        if (identifier == "blocked")
        {
            var chargeTags = objectSpace.GetObjects<OCPPChargeTag>().Where(t => t.Blocked).Select(t => t.ChargeTag).ToList();

            if (chargeTags != null)
            {
                await Publish(chargeTags, identifier, correlationData);
                return;
            }
        }

        if (identifier == "master")
        {
            var chargeTags = objectSpace.GetObjects<OCPPChargeTag>().Where(t => t.ChargeTagGroup?.Identifier == identifier).Select(t => t.ChargeTag).ToList();

            if (chargeTags != null)
            {
                await Publish(chargeTags, identifier, correlationData);
                return;
            }
        }

        var chargeTag = objectSpace?.FindObject<OCPPChargeTag>(CriteriaOperator.Parse("Identifier = ?", identifier));
        if (chargeTag != null)
        {
            await Publish(chargeTag.ChargeTag, correlationData);
            return;
        }

        await PublishNull(nameof(ChargeTag), identifier, correlationData);
    }
    #endregion

    #region OnDataToGatewayReceived
    public async void OnDataToGatewayReceived(MessageReceivedEventArgs args)
    {
        DataToGatewayReceived?.Invoke(this, args);
    }
    #endregion

    #region publish
    public async Task Publish(ChargingConfiguration configuration, string chargePointId, string correlationData = "")
    {
        var payload = Serialize(configuration);
        var topic = MqttTopicService.GetDataTopic("ChargingConfiguration", chargePointId, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }

    public async Task Publish(SendLocalListRequest request, string chargePointId)
    {
        var payload = Serialize(request);
        var topic = MqttTopicService.GetDataTopic("SendLocalList", chargePointId, false);
        await PublishStringAsync(topic, payload, false);
    }
    public async Task Publish(DataTransferRequest dataTransferRequest, string chargePointId, string correlationData = "")
    {
        var payload = Serialize(dataTransferRequest);
        var topic = MqttTopicService.GetDataTopic("DataTransfer", chargePointId, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }
    public async Task Publish(DataTransferResponse dataTransferResponse, string chargePointId, string correlationData)
    {
        var payload = Serialize(dataTransferResponse);
        var topic = MqttTopicService.GetDataTopic("DataTransfer", chargePointId, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }
    public async Task Publish(ChargePoint chargePoint, string correlationData)
    {
        var payload = Serialize(chargePoint);
        var topic = MqttTopicService.GetDataTopic(nameof(ChargePoint), chargePoint.ChargePointId, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }
    public async Task Publish(List<ChargePoint> chargePoints, string groupIdentifier, string correlationData)
    {
        var payload = Serialize(chargePoints);
        var topic = MqttTopicService.GetDataTopic(nameof(ChargePoint), groupIdentifier, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }

    public async Task Publish(ChargeTag chargeTag, string correlationData)
    {
        var payload = Serialize(chargeTag);
        var topic = MqttTopicService.GetDataTopic(nameof(ChargeTag), chargeTag.TagId, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }
    public async Task Publish(List<ChargeTag> chargeTags, string groupIdentifier, string correlationData)
    {
        var payload = Serialize(chargeTags);
        var topic = MqttTopicService.GetDataTopic(nameof(ChargeTag), groupIdentifier, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }

    public async Task PublishNull(string dataType, string identifier, string correlationData)
    {
        var payload = "null";
        var topic = MqttTopicService.GetDataTopic(dataType, identifier, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }
    public async Task Publish(OCPPRemoteControl control)
    {
        if (control.ChargePoint == null || control.Request == null || !control.IsValidPayload)
        {
            return;
        }
        string type = control.Request.Name;
        var topic = MqttTopicService.GetDataTopic(type, control.ChargePoint.Identifier, false);
        await PublishStringAsync(topic, control.Payload, false);
    }

    public async Task Publish(List<Transaction> transactions, string identifier, string correlationData)
    {
        var payload = Serialize(transactions);
        var topic = MqttTopicService.GetDataTopic(nameof(Transaction), identifier, false);
        await PublishStringAsync(topic, payload, false, correlationData);
    }
    #endregion

    #region MQTT related functions
    private string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, JsonSerializerSettings);
    }

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
        var args = new MessageReceivedEventArgs
        {
            Action = decodedTopic["action"],
            Identifier = decodedTopic["identifier"],
            FromChargePoint = decodedTopic["direction"] == "in",
            Payload = payload
        };

        var correlationData = arg.ApplicationMessage.CorrelationData;
        if (correlationData != null && correlationData.Count() > 0)
        {
            args.CorrelationData = Encoding.ASCII.GetString(correlationData);
        }

        if (MatchesWildcard(arg.ApplicationMessage.Topic, TopicSubscribeDataFromChargePoint))
        {
            OnDataFromGatewayReceived(args).RunInBackground();
        }

        if (MatchesWildcard(arg.ApplicationMessage.Topic, TopicSubscribeDataToChargePoint))
        {
            OnDataToGatewayReceived(args);
        }

        if(MatchesWildcard(arg.ApplicationMessage.Topic, TopicSubscribeOcpp16FromChargePoint))
        {
            OCPPMessageReceived?.Invoke(this, args);
        }

        if (MatchesWildcard(arg.ApplicationMessage.Topic, TopicSubscribeOcpp16ToChargePoint))
        {
            OCPPMessageReceived?.Invoke(this, args);
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

        message.Replace("\n", "");
        message.Replace("\r", "");

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
            CorrelationData = !string.IsNullOrEmpty(correlationData) ? Encoding.ASCII.GetBytes(correlationData) : []
        };

        await mqttClient.PublishAsync(mqttMessage);
    }
    #endregion
}
