using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

using Windows.Web.Http;
using Microsoft.Azure.Devices.Client;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CognitiveClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string DeviceConnectionString = "HostName=WinkeyIoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=MlTZyPsMpWGRdUGqVOao/i1isYzdxQwJ35gh6nftIMo=";
        private const string DeviceID = "Device1";
        private const string CognitiveApiKey = "1e6a0214e8e243f69825e35b19270bbc"; //Replace this with your own Microsoft Cognitive Services Emotion API key from https://www.microsoft.com/cognitive-services/en-us/emotion-api. Please do not use my key. I include it here so you can get up and running quickly
        private static DeviceClient SensorDevice = null;
        private DispatcherTimer timer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();

            SensorDevice = DeviceClient.CreateFromConnectionString(DeviceConnectionString, "Device1");
        }
        

        private async void btnTakeaPhoto_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                // User cancelled photo capture
                return;
            }

            string fileName = CreateTimeToFileName();


            StorageFolder destinationFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePhotoFolder", CreationCollisionOption.OpenIfExists);
            await photo.CopyAsync(destinationFolder, fileName, NameCollisionOption.ReplaceExisting);

            IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();

            await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);

            imageControl.Source = bitmapSource;

            HttpStreamContent streamContent = new HttpStreamContent(stream);
            streamContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/octet-stream");

            Uri apiEndPoint = new Uri("https://api.projectoxford.ai/emotion/v1.0/recognize");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiEndPoint);
            request.Content = streamContent;

            // Do an asynchronous POST.            
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveApiKey);
            HttpResponseMessage response = await httpClient.SendRequestAsync(request).AsTask();

            // Read response
            string responseContent = await response.Content.ReadAsStringAsync();

            SendMessage(responseContent);

            await photo.DeleteAsync();
        }

        private string CreateTimeToFileName()
        {
            string fileName;

            fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";

            return fileName;
        }

        private async void SendMessage(string message)
        {
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(Sensor.GetWetherData(DeviceID));
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(message));
            await SensorDevice.SendEventAsync(eventMessage);
        }
    }
}
