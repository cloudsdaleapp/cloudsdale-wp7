using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cloudsdale.Managers;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Screenshot {
    public partial class ViewShot {
        public static WriteableBitmap Image;
        public static Grid ScreenshotGrid;

        public ViewShot() {
            InitializeComponent();
            ImageView.Source = Image;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            ScreenshotGrid.Background = null;
        }

        private void SaveClick(object sender, RoutedEventArgs e) {
            TitleBox.IsEnabled = false;
            SaveButton.IsEnabled = false;
            UploadButton.IsEnabled = false;

            var screenshot = Image;
            var title = string.IsNullOrWhiteSpace(TitleBox.Text)
                            ? "Cloudsdale_" + DateTime.Now.TimeOfDay.Ticks
                            : TitleBox.Text;

            using (var ms = new MemoryStream()) {
                screenshot.SaveJpeg(ms, screenshot.PixelWidth, screenshot.PixelHeight, 0, 100);
                ms.Seek(0, SeekOrigin.Begin);

                var library = new MediaLibrary();
                library.SavePicture(title, ms);
            }

            MessageBox.Show("Picture saved successfully!");
            NavigationService.GoBack();
        }

        private void UploadClick(object sender, RoutedEventArgs e) {
            TitleBox.IsEnabled = false;
            SaveButton.IsEnabled = false;
            UploadButton.IsEnabled = false;
            LoadingBar.IsIndeterminate = true;

            var screenshot = Image;
            var title = string.IsNullOrWhiteSpace(TitleBox.Text)
                            ? "Cloudsdale_" + DateTime.Now.Ticks
                            : TitleBox.Text;

            var ms = new MemoryStream();
            screenshot.SaveJpeg(ms, screenshot.PixelWidth, screenshot.PixelHeight, 0, 100);

            ImagePost(ms, title, (success, response) => {
                if (success) {
                    ms.Seek(0, SeekOrigin.Begin);
                    var library = new MediaLibrary();
                    library.SavePicture(title, ms);
                    MessageBox.Show("Image saved and uploaded successfully!");

                    UploadResult.ViewResult = response;
                    NavigationService.Navigate(new Uri("/Screenshot/UploadResult.xaml", UriKind.Relative));
                } else {
                    MessageBox.Show("Uploading failed.");
                    TitleBox.IsEnabled = true;
                    SaveButton.IsEnabled = true;
                    UploadButton.IsEnabled = true;
                    LoadingBar.IsIndeterminate = false;
                }
                ms.Dispose();
            });

        }

        public void ImagePost(Stream imageStream, string title, Action<bool, JObject> callback) {
            imageStream.Seek(0, SeekOrigin.Begin);
            var boundary = "----CDAppBoundary" + Guid.NewGuid();

            byte[] data;
            using (var dataStream = new MemoryStream()) {
                dataStream.WriteLine("--" + boundary);
                dataStream.WriteLine("Content-Disposition: form-data; name=\"image\"; filename=\"" + Uri.EscapeDataString(title) + ".jpg\"");
                dataStream.WriteLine("Content-Type: image/jpeg");
                dataStream.WriteLine();
                imageStream.CopyTo(dataStream);
                dataStream.WriteLine();
                dataStream.WriteLine("--" + boundary + "--");

                data = dataStream.ToArray();
            }

            var request = WebRequest.CreateHttp("http://imm.io/store/");
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Headers["Content-Length"] = data.Length.ToString();

            request.BeginGetRequestStream(rsac => {
                try {
                    using (var requestStream = request.EndGetRequestStream(rsac)) {
                        requestStream.Write(data, 0, data.Length);
                        requestStream.Close();
                    }

                    request.BeginGetResponse(rac => {
                        try {
                            using (var response = request.EndGetResponse(rac))
                            using (var responseStream = response.GetResponseStream())
                            using (var responseReader = new StreamReader(responseStream, Encoding.UTF8)) {
                                var result = JObject.Parse(responseReader.ReadToEnd());
                                Dispatcher.BeginInvoke(() => callback((bool)result["success"], result));
                            }
                        } catch (WebException ex) {
                            using (var response = ex.Response)
                            using (var responseStream = response.GetResponseStream())
                            using (var responseReader = new StreamReader(responseStream, Encoding.UTF8)) {
                                var str = responseReader.ReadToEnd();
                                try {
                                    var result = JObject.Parse(str);
                                    Dispatcher.BeginInvoke(() => callback((bool)result["success"], result));
                                } catch {
                                    Dispatcher.BeginInvoke(() => callback(false, null));
                                }
                            }
                        }
                    }, null);
                } catch {
                    Dispatcher.BeginInvoke(() => callback(false, null));
                }
            }, null);
        }
    }
}