using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace Pepwave_Gps_Middleware
{
    public class Worker : BackgroundService
    {

        public static IConfigurationRoot configuration;

        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Initial configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the
            // host running the application.  
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();//.Where(a => 1==1);
            var port = configuration.GetValue<int>("PepwaveTargetPort");
            var localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            var listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return;
            }
            Socket handler = listener.Accept();
            var options = new ManagedMqttClientOptionsBuilder()
                        .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                        .WithClientOptions(new MqttClientOptionsBuilder()
                            .WithTcpServer(configuration.GetValue<string>("MqttBrokerIp")).Build())
                        .Build();

            var mqttClient = new MqttFactory().CreateManagedMqttClient();
            await mqttClient.StartAsync(options);



            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Incoming data from the client.  
                    // Program is suspended while waiting for an incoming connection.  

                    string data = null;


                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    _logger.LogInformation($"GPS message received\nRaw Message:\n{data}");

                    var pepwaveMessage = new Pepwave_NMEA_Message(data);

                    _logger.LogInformation("MQTT message publishing");
                    await mqttClient.PublishAsync("location/pepwave", pepwaveMessage.ToJson());

                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.ToString());

                }


                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
