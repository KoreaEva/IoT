using Microsoft.ProjectOxford.Emotion.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Happy;

using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Azure.Devices.Client;


using Windows.Devices.Gpio;
using HappyTestClient.Entities;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HappyTestClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string[] Person = new string[5];
        private Entities.Store[] Store = new Entities.Store[5];
        private int PersonIndex = 0;
        private int StoreIndex = 0;

        BMP280Dumi BMP280 = new BMP280Dumi();
        EmotionClient client = new EmotionClient();

        IoTHubProxy IoTProxy = new IoTHubProxy();

        public MainPage()
        {
            this.InitializeComponent();
            
            InitPersons();
            InitStore();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Start();

            DispatcherTimer timerStatus = new DispatcherTimer();
            timerStatus.Tick += TimerStatus_Tick;
            timerStatus.Interval = new TimeSpan(0, 0, 2);
            timerStatus.Start();

            PersonIndex = 2;
        }

        private async void ReceiveC2dAsync()
        {
            Debug.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await HappyTestClient.IoTHubProxy.deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                var status = Encoding.ASCII.GetString(receivedMessage.GetBytes());


                Debug.WriteLine("Received message: {0}", status);


                if (status.Substring(24, 2) == "ON")
                {
                    SolidColorBrush brush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
                    ellStatus.Fill = brush;
                }
                else
                {
                    SolidColorBrush brush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 255));
                    ellStatus.Fill = brush;
                }

                await HappyTestClient.IoTHubProxy.deviceClient.CompleteAsync(receivedMessage);
            }
        }


        private void TimerStatus_Tick(object sender, object e)
        {
            ReceiveC2dAsync();
        }
        
        private async void Timer_Tick(object sender, object e)
        {
            try
            {
                //emotion api calling
                float humidity = BMP280.GetHumidity();
                float temperature = BMP280.GetTemperature();

                Scores scores = await client.RecognizeAsync(Person[PersonIndex]);
                HappyModel model = new HappyModel(Store[StoreIndex].StoreName, temperature, humidity, 
                    Store[StoreIndex].Longitude.ToString(), 
                    Store[StoreIndex].Latitude.ToString(), 
                    scores);

                DisplayModel(model);

                //Image setting
                Image img = sender as Image;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(Person[PersonIndex]);

                imgPerson.Source = bitmapImage;


                //Map Setting
                Windows.Devices.Geolocation.BasicGeoposition geo = new Windows.Devices.Geolocation.BasicGeoposition();
                geo.Latitude = Store[StoreIndex].Latitude;
                geo.Longitude = Store[StoreIndex].Longitude;

                Windows.UI.Xaml.Controls.Maps.MapIcon icon = new Windows.UI.Xaml.Controls.Maps.MapIcon();
                icon.Location = new Windows.Devices.Geolocation.Geopoint(geo);
                icon.NormalizedAnchorPoint = new Point(1, 1);
                icon.Title = Store[StoreIndex].StoreName;

                mapStoreLocation.MapElements.Clear();
                mapStoreLocation.MapElements.Add(icon);

                await mapStoreLocation.TrySetViewAsync(new Windows.Devices.Geolocation.Geopoint(geo), 18, 0, 0, Windows.UI.Xaml.Controls.Maps.MapAnimationKind.Bow);

                //IoTHubProxy data sending
                try
                {
                    IoTProxy.SendMessage(model);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("IoTHub======" + ex.Message);
                }

                PersonIndex++;
                StoreIndex++;


                //Init index
                if (PersonIndex == Person.Count() - 1) PersonIndex = 0;
                if (StoreIndex == Store.Count() - 1) StoreIndex = 0;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("System======" + ex.Message);
            }
        }
        
        private void InitPersons()
        {
            Person[0] = "http://i.imgur.com/OipSklR.jpg";
            Person[1] = "http://4.bp.blogspot.com/-gtvjiFZcQHE/Tx4XCeKa8oI/AAAAAAAABOM/_iLbmIPCOyo/s1600/barack-obama.jpg";
            Person[2] = "http://cfile25.uf.tistory.com/image/147D563F5110963307FDC1";
            Person[3] = "http://cfile22.uf.tistory.com/image/177FEE4F508F91132390FF";
            Person[4] = "https://pbs.twimg.com/media/CbA6qThVAAEpmLy.jpg";
        }

        private void InitStore()
        {
            Store[0] = new Store("광주횟집", 37.541360, 127.129656);
            Store[1] = new Store("쌍다리돼지불백", 37.593439, 126.995641);
            Store[2] = new Store("월향광화문점", 37.567855, 126.975755);
            Store[3] = new Store("삼해집", 37.569918, 126.990889);
            Store[4] = new Store("산모퉁이", 37.595684, 126.968018);
        }

        private void DisplayModel(HappyModel model)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("storeID : {0}\n", model.storeID);
            sb.AppendFormat("time : {0}\n", model.time);
            sb.AppendFormat("Anger : {0}\n", model.Anger);
            sb.AppendFormat("Contempt : {0}\n", model.Contempt);
            sb.AppendFormat("Disgust : {0}\n", model.Disgust);
            sb.AppendFormat("Fear : {0}\n", model.Fear);
            sb.AppendFormat("Happiness : {0}\n", model.Happiness);
            sb.AppendFormat("Neutral : {0}\n", model.Neutral);
            sb.AppendFormat("Sadness : {0}\n", model.Sadness);
            sb.AppendFormat("Surprise : {0}\n", model.Surprise);
            sb.AppendFormat("Temperature: {0}\n", model.temperature);
            sb.AppendFormat("Humidity: {0}\n", model.humidity);
            textbox.Text = sb.ToString();
        }

    }
}
