﻿namespace OCPPGateway.Module.Messages_OCPP16;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;


public class SetChargingProfileRequest
{
    [JsonProperty("connectorId", Required = Required.Always)]
    public int ConnectorId { get; set; }


    [JsonProperty("csChargingProfiles", Required = Required.Always)]
    public ChargingProfile CsChargingProfiles { get; set; }
}
