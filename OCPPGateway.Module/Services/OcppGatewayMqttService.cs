namespace OCPPGateway.Module.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using OCPPGateway.Module.Models;
using System;

public enum CommunicationProtocol
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
public abstract class OcppGatewayMqttService
{
    private IMqttClient mqttClient;
    private MqttClientOptions options;
    private ILogger<OcppGatewayMqttService> _logger;

    // private string TopicSubscribeOcpp16 => MqttTopicService.GetOcppTopic(CommunicationProtocol.OCPP16, "+", "+", true, "+");
    // private string TopicSubscribeOcpp20 => MqttTopicService.GetOcppTopic(CommunicationProtocol.OCPP20, "+", "+", true, "+");
    private string TopicSubscribeData => MqttTopicService.GetDataTopic("+", "+", true);
    private string[] topicsToSubscribe => [TopicSubscribeData];

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public abstract IEnumerable<ChargePoint> GetChargePoints(string? identifier = null);
    public abstract IEnumerable<ChargeTag> GetChargeTags(string? identifier = null);

    public async void OnDataReceived(DataReceivedEventArgs args)
    {
        Console.WriteLine($"Payload: {args.Payload}");

        string? identifier = args.Identifier == "all" ? null : args.Identifier;

        switch (args.Type)
        {
            case nameof(ChargePoint):
                var cps = GetChargePoints(identifier);
                if (cps.Count() < 1)
                {
                    Console.WriteLine($"Charge Point {args.Identifier} not found");
                    return;
                }
                foreach (var cp in cps)
                {
                    await Publish(cp);
                }
                break;

            case nameof(ChargeTag):
                var cts = GetChargeTags(identifier);
                if(cts.Count() < 1)
                {
                    Console.WriteLine($"Charge Point {args.Identifier} not found");
                    return;
                }
                foreach (var cp in cts)
                {
                    await Publish(cp);
                }
                break;
        }
    }

    public async void SendInitialData()
    {

        foreach (var cp in GetChargePoints())
        {
            await Publish(cp);
        }
        foreach (var ct in GetChargeTags())
        {
            await Publish(ct);
        }
    }

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
            .WithClientId(mqttClientId)
            .WithTcpServer(mqttHost, Convert.ToInt32(mqttPort))
            .WithCredentials(mqttUser, mqttPassword)
            //.WithTls()
            .WithCleanSession()
            .Build();

        OnConfiguring();
    }

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

        var decodedTopic = MqttTopicService.DecodeTopic(arg.ApplicationMessage.Topic);
        
        if(MatchesWildcard(arg.ApplicationMessage.Topic, TopicSubscribeData))
        {
            OnDataReceived(new DataReceivedEventArgs
            {
                Type = decodedTopic["type"],
                Identifier = decodedTopic["identifier"]
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
            SendInitialData();
        }
        _logger.LogInformation("SUBCRIPTIONS SUCCESSFULL");
    }

    public static bool MatchesWildcard(string topic, string wildcardTopic)
    {
        return MqttTopicFilterComparer.Compare(topic, wildcardTopic) == MqttTopicFilterCompareResult.IsMatch;
    }
    public async Task PublishBinaryAsync(string topic, byte[] payload, bool retain = false)
    {
        await mqttClient.PublishBinaryAsync(topic, payload, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce, retain);
        _logger.LogInformation("MESSAGE SENT");
    }
    public async Task PublishStringAsync(string topic, string? message, bool retain = false)
    {
        if (message == null)
            return;

        while (!mqttClient.IsConnected)
        {
            Thread.Sleep(1000);
        }
        _logger.LogInformation(message);
        await mqttClient.PublishStringAsync(topic, message, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce, retain);
    }
    #endregion
}
