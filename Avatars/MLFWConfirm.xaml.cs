using System;
using System.IO;
using System.Net;
using System.Windows;
using Cloudsdale.Avatars.Mlfw;

namespace Cloudsdale.Avatars {
    public partial class MLFWConfirm {
        public static MlfwImage target;

        public MLFWConfirm() {
            InitializeComponent();
            DataContext = target;
        }

        private void YesClick(object sender, RoutedEventArgs e) {
            YesBtn.IsEnabled = false;
            NoBtn.IsEnabled = false;
            LoadImage((picStream, mimeType) => {
                ChangeAvatar.target.UploadAvatar(picStream, mimeType, ChangeAvatar.UpdatedAvatar);
                Dispatcher.BeginInvoke(() => {
                    NavigationService.RemoveBackEntry();
                    NavigationService.GoBack();
                });
            }, () => {
                YesBtn.IsEnabled = true;
                NoBtn.IsEnabled = true;
            });
        }

        private void NoClick(object sender, RoutedEventArgs e) {
            NavigationService.GoBack();
        }

        private void LoadImage(Action<Stream, string> callback, Action failed) {
            var request = WebRequest.CreateHttp(target.ImageUri);
            request.BeginGetResponse(ar => {
                try {
                    using (var response = request.EndGetResponse(ar))
                    using (var responseStream = response.GetResponseStream()) {
                        callback(responseStream, response.ContentType);
                    }
                } catch {
                    Deployment.Current.Dispatcher.BeginInvoke(failed);
                }
            }, null);
        }
    }
}