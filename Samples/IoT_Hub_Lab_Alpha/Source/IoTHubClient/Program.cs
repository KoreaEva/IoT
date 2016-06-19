using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Client;
using System.Timers;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IoTHubClient
{
    class Program
    {
        private const string DeviceConnectionString = "HostName=WinkeyIoTHub.azure-devices.net;SharedAccessKeyName=device;SharedAccessKey=DHKD/Pkrh/b6lsG+f42ql+W4dnRenSNSsMFMjVUpcLY=";
        private const string DeviceID = "Device1";
        private static System.Timers.Timer SensorTimer;

        private static DeviceClient SensorDevice = null;
        
        private static int MESSAGE_COUNT = 1;

        private static DummySensor Sensor = new DummySensor();

        static void Main(string[] args)
        {
            SetTimer();

            SensorDevice = DeviceClient.CreateFromConnectionString(DeviceConnectionString, "Device1");

            if(SensorDevice == null)
            {
                Console.WriteLine("Failed to create DeviceClient!");
                SensorTimer.Stop();
            }

            Console.WriteLine("\nPress the Enter key to exit the application...\n");
            Console.WriteLine("The application started at {0:HH:mm:ss.fff}", DateTime.Now);
            Console.ReadLine();
            SensorTimer.Stop();
            SensorTimer.Dispose();
        }

        private static void SetTimer()
        {
            SensorTimer = new Timer(2000);
            SensorTimer.Elapsed += SensorTimer_Elapsed;
            SensorTimer.AutoReset = true;
            SensorTimer.Enabled = true;
        }

        private async static void SensorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
            await SendEvent();
            await ReceiveCommands();
        }

        static async Task SendEvent()
        {
            string dataBuffer;

            Console.WriteLine("Device sending {0} messages to IoTHub...\n", MESSAGE_COUNT, TransportType.Amqp);

            dataBuffer = Guid.NewGuid().ToString();
            

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Sensor.GetWetherData(DeviceID));

            Console.WriteLine(json);
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(json));
            await SensorDevice.SendEventAsync(eventMessage);
        }

        static async Task ReceiveCommands()
        {
            Message receivedMessage;
            string messageData;

            receivedMessage = await SensorDevice.ReceiveAsync(TimeSpan.FromSeconds(1));

            if (receivedMessage != null)
            {
                messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

                await SensorDevice.CompleteAsync(receivedMessage);
            }
        }
    }
}
