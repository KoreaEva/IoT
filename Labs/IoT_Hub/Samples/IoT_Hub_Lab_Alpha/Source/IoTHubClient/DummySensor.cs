using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubClient
{
    class DummySensor
    {
        private Random _Random = new Random();

        public WetherModel GetWetherData(string deviceID)
        {
            WetherModel wetherModel = new WetherModel();

            wetherModel.DeviceID = deviceID;
            wetherModel.Temperature = _Random.Next(25, 32);
            wetherModel.Humidity = _Random.Next(60, 80);
            wetherModel.Dust = 50 + wetherModel.Temperature + _Random.Next(1, 5);
            return wetherModel;
        }
    }


    public class WetherModel
    {
        public string DeviceID { get; set; }
        public int Temperature { get; set; }
        public int Humidity { get; set; }
        public int Dust { get; set; }
    }
}
