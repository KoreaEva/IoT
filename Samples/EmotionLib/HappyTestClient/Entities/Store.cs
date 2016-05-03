using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyTestClient.Entities
{
    public class Store
    {
        public Store(string storeName, double latiude, double longitude)
        {
            this.StoreName = storeName;
            this.Longitude = longitude;
            this.Latitude = latiude;
        }

        public string StoreName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
