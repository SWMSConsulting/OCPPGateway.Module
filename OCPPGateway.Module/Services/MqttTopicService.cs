namespace OCPPGateway.Module.Services
{
    public static class MqttTopicService
    {
        private static string baseTopic = Environment.GetEnvironmentVariable("MQTT_BASE_TOPIC") ?? "";

        public static string GetOcppTopic(OCPPVersion protocol, string clientId, string action, bool fromGateway)
        {
            return GetOcppTopic(protocol.ToString().ToLowerInvariant(), clientId, action, fromGateway);
        }

        private static string GetOcppTopic(string protocol, string clientId, string action, bool fromGateway)
        {
            var direction = fromGateway ? "out" : "in";
            return $"{baseTopic}{protocol}/{clientId}/{action}/{direction}";
        }

        public static string GetDataTopic(string type, string identifier, bool fromGateway)
        {
            var direction = fromGateway ? "out" : "in";
            return $"{baseTopic}data/{type}/{identifier}/{direction}";
        }

        public static string GetIotTopic(string measurement)
        {
            return $"{baseTopic}iot/{measurement}";
        }

        public static Dictionary<string, string> DecodeTopic(string topic)
        {
            if(!string.IsNullOrEmpty(baseTopic)){
                topic.Replace(baseTopic, "");
            }

            var segments = topic.Split('/');
            string protocol = segments[0];

            if (protocol == "data")
            {
                return new Dictionary<string, string>
                {
                    { "protocol", protocol },
                    { "type", segments[1] },
                    { "identifier", segments[2] },
                    { "direction", segments[3] }
                };
            }
            else
            {
                return new Dictionary<string, string>
                {
                    { "protocol", protocol },
                    { "clientId", segments[1] },
                    { "action", segments[2] },
                    { "direction", segments[3] },
                    { "messageId", segments[4] }
                };
            }
        }
    
    }
}
