using System;
using System.Text.Json.Serialization;

namespace Pepwave_Gps_Middleware
{
    public class DeviceTrackerJson
    {
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("gps_accuracy")]
        public int GpsAccuracy { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("battery_level")]
        public double BatteryLevel { get; set; }

        [JsonPropertyName("retain")]
        public string MyProperty { get; set; }

        [JsonPropertyName("time_stamp")]
        public string TimeStamp { get; set; } = DateTime.UtcNow.ToString();
    }
}