using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Happy;

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

namespace HappyTestClient
{
    class IoTHubProxy
    {
        public static DeviceClient deviceClient;

        private static string IotHubUri = "winkeyiot.azure-devices.net";
        private static string DeviceID = "dev001";
        private static string DeviceKey = "ssAR8osgG7BGzjn1g9zZbrsJTZFdn/J6TCyvWYF7IqA=";

        public IoTHubProxy()
        {
            deviceClient = DeviceClient.Create(IotHubUri, 
                new DeviceAuthenticationWithRegistrySymmetricKey(DeviceID, DeviceKey));
        }
        public async void SendMessage(HappyModel model)
        {
            var messageString = JsonConvert.SerializeObject(model);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
        }
        
        /// <summary>
        /// GetDeviceID()
        /// </summary>
        /// <returns>DeviceID</returns>
        public static string GetDeviceID()
        {
            return DeviceID;
        }
    }
}
