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

//Preview를 위해서 추가된 부분
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.Media.MediaProperties;

using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Emotion.Contract;

using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CognitiveClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string DeviceConnectionString = "HostName=WinkeyIoT2.azure-devices.net;SharedAccessKeyName=device;SharedAccessKey=poG/3FG6/aHs5vDEa2u33kxO+O+QoIv3sSkSgD7uVbo=";
        private const string DeviceID = "Device1";
        private const string CognitiveApiKey = "1e6a0214e8e243f69825e35b19270bbc"; //Replace this with your own Microsoft Cognitive Services Emotion API key from https://www.microsoft.com/cognitive-services/en-us/emotion-api. Please do not use my key. I include it here so you can get up and running quickly
        private static DeviceClient SensorDevice = null;
        private DispatcherTimer timer = new DispatcherTimer();

        MediaCapture _mediaCapture;
        bool _isPreviewing;
        DisplayRequest _displayRequest;

        public MainPage()
        {
            this.InitializeComponent();

            Application.Current.Suspending += Application_Suspending;
            StartPreviewAsync();
            SensorDevice = DeviceClient.CreateFromConnectionString(DeviceConnectionString, "Device1");
        }
        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        private async void StartPreviewAsync()
        {
            try
            {

                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();

                CapPreview.Source = _mediaCapture;
                await _mediaCapture.StartPreviewAsync();
                _isPreviewing = true;

                _displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                System.Diagnostics.Debug.WriteLine("The app was denied access to the camera");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MediaCapture initialization failed. {0}", ex.Message);
            }
        }

        private async Task CleanupCameraAsync()
        {
            if (_mediaCapture != null)
            {
                if (_isPreviewing)
                {
                    await _mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CapPreview.Source = null;
                    if (_displayRequest != null)
                    {
                        _displayRequest.RequestRelease();
                    }

                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                });
            }
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await CleanupCameraAsync();
        }



        private async void btnTakeaPhoto_Click(object sender, RoutedEventArgs e)
        {
            var captureStream = new InMemoryRandomAccessStream();
            
            await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(captureStream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();

            await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);

            imageControl.Source = bitmapSource;

            EmotionServiceClient emotionServiceClient = new EmotionServiceClient(CognitiveApiKey);
            Emotion[] emotionResult = await emotionServiceClient.RecognizeAsync(captureStream.CloneStream().AsStream());

            EmotionEntity[] emotions = new EmotionEntity[emotionResult.Count()];

            float totalHappiness = 0;
            float totalSadness = 0;
            stackResult.Children.Clear();

            for(int i=0;i<emotionResult.Count();i++)
            {
                EmotionEntity emotionEntity = new EmotionEntity();

                emotionEntity.Anger = emotionResult[i].Scores.Anger * 100;
                emotionEntity.Contempt = emotionResult[i].Scores.Contempt * 100;
                emotionEntity.Disgust = emotionResult[i].Scores.Disgust * 100;
                emotionEntity.Fear = emotionResult[i].Scores.Fear * 100;
                emotionEntity.Happiness = emotionResult[i].Scores.Happiness * 100;
                emotionEntity.Neutral = emotionResult[i].Scores.Neutral * 100;
                emotionEntity.Sadness = emotionResult[i].Scores.Sadness * 100;
                emotionEntity.Surprise = emotionResult[i].Scores.Surprise * 100;

                totalHappiness += emotionEntity.Happiness;
                totalSadness += emotionEntity.Sadness;

                TextBlock textblock = new TextBlock();

                string emotionString;
                emotionString = "Anger:  " + emotionEntity.Anger.ToString() + "\n";
                emotionString += "Contempt:  " + emotionEntity.Contempt.ToString() + "\n";
                emotionString += "Disgust:  " + emotionEntity.Disgust.ToString() + "\n";
                emotionString += "Fear:  " + emotionEntity.Fear.ToString() + "\n";
                emotionString += "Happiness:  " + emotionEntity.Happiness.ToString() + "\n";
                emotionString += "Neutral:  " + emotionEntity.Neutral.ToString() + "\n";
                emotionString += "Sadness:  " + emotionEntity.Sadness.ToString() + "\n";
                emotionString += "Suprise:  " + emotionEntity.Surprise.ToString() + "\n";

                textblock.Text = emotionString;

                stackResult.Children.Add(textblock);

                string json = JsonConvert.SerializeObject(emotionEntity);
                //SendMessage(json);

                emotions[i] = emotionEntity;
            }

            float avgHappiness = totalHappiness / emotionResult.Count();
            float avgSadness = totalSadness / emotionResult.Count();

            lblGroupResult.Text = String.Format(" 행복지수는 {0} 이며\n 슬픔지수는 {1} 입니다.", avgHappiness, avgSadness);
            //lblGroupResult.Text = String.Format(" {0}명의 사람의 행복지수는 {1} 이며\n 슬픔지수는 {2} 입니다.", emotionResult.Count(), avgHappiness, avgSadness);
        }

        //private async void SendMessage(string message)
        //{
        //    Message eventMessage = new Message(Encoding.UTF8.GetBytes(message));
        //    await SensorDevice.SendEventAsync(eventMessage);
        //}
    }
}
