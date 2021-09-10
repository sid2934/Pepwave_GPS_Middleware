using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ghostware.NMEAParser.Exceptions;
using Ghostware.NMEAParser.NMEAMessages;

namespace Pepwave_Gps_Middleware
{
    public class Pepwave_NMEA_Message
    {

        public string RawMessage { get; set; }

        public GprmcMessage GprmcData { get; set; }

        public GpvtgMessage GpvtgData { get; set; }

        public GpggaMessage GpggaData { get; set;}

        public GpgsaMessage GpgsaData { get; set;}

        //public List<GpgsvMessage> MyProperty { get; set; } //ToDo: Create PR for GPGSV messages

        public Pepwave_NMEA_Message(string receivedData)
        {
            RawMessage = receivedData;
            var receivedLines = receivedData.Split("\r\n");
            
            var nmeaParser = new Ghostware.NMEAParser.NmeaParser();
            foreach(var line in receivedLines)
            {
                try
                {
                    var message = nmeaParser.Parse(line);
                    switch (message)
                    {
                        case GprmcMessage:
                            GprmcData = message as GprmcMessage;
                            break;

                        case GpvtgMessage:
                            GpvtgData = message as GpvtgMessage;
                            break;

                        case GpggaMessage:
                            GpggaData = message as GpggaMessage;
                            break;

                        case GpgsaMessage:
                            GpgsaData = message as GpgsaMessage;
                            break;
                        // case GpgsvMessage:
                        //     break;

                        default:
                            //ignore lines that are not parseable
                            break;

                    }

                }
                catch(UnknownTypeException e)
                {
                    //squash errors
                    //throw new Exception($"{e}\n\nThe following line did not have a defined message type.\n\t{line}");
                }
            }
        }

        public string ToJson()
        {

            return JsonSerializer.Serialize(new DeviceTrackerJson()
            {
                Longitude = GprmcData.Longitude,
                Latitude = GprmcData.Latitude,
                BatteryLevel = 100.0,
                GpsAccuracy = 60

            });
        }


    }
}