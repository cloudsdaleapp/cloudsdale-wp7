using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;

namespace Cloudsdale {
    /// <summary>
    /// Static class to aid in the static connection data that should be maintained for all things done with respect to the server
    /// </summary>
    public static class Connection {
        public static Cloud CurrentCloud;
        public static string FacebookUid;
        public static int LoginType;
        public static LoginResponse LoginResult;
        public static string CloudsdaleClientId;
        public static LoggedInUser CurrentCloudsdaleUser;

        public static FayeConnector.FayeConnector Faye;

        public static void Connect(Page page = null, Dispatcher dispatcher = null) {
            switch (LoginType) {
                case 0:
                    break;
                case 1:
                    // Should never be hit, but just in case
                    if (page == null) {
                        // ReSharper disable PossibleNullReferenceException
                        Deployment.Current.Dispatcher.BeginInvoke(
                            () => (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(
                                new Uri("/MainPage.xaml", UriKind.Relative)));
                        // ReSharper restore PossibleNullReferenceException
                    }
                    FacebookAuth.FBOANegotiator.FacebookLogin(page);
                    return;
                default:
                    return;
            }

            Faye = new FayeConnector.FayeConnector(Resources.pushUrl);

            Faye.HandshakeComplete += (sender, args) => {
                if (dispatcher == null)
                    foreach (var cloud in CurrentCloudsdaleUser.clouds) {
                        Managers.MessageCacheController.Subscribe(cloud.id);
                    }

                Managers.MessageCacheController.Init();

                if (page == null) {
                    if (dispatcher != null) {
                        dispatcher.BeginInvoke(() => {
                            var phoneApplicationFrame = Application.Current.RootVisual as PhoneApplicationFrame;
                            if (phoneApplicationFrame != null)
                                phoneApplicationFrame.Navigate(new Uri("/Home.xaml", UriKind.Relative));
                        });
                    }
                } else {
                    page.Dispatcher.BeginInvoke(
                        () => page.NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative)));
                }
            };

            Faye.Handshake();
        }

        public static void SendMessage(string cloud, string message) {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SentMessage { content = message, client_id = Faye.ClientId }));
            var request = WebRequest.CreateHttp(Resources.SendEndpoint.Replace("{cloudid}", cloud));
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["Content-Length"] = data.Length.ToString();
            request.Headers["X-Auth-Token"] = CurrentCloudsdaleUser.auth_token;
            request.BeginGetRequestStream(ar => {
                var reqs = request.EndGetRequestStream(ar);
                reqs.Write(data, 0, data.Length);
                reqs.Close();
                request.BeginGetResponse(a => request.EndGetResponse(a).Close(), null);
            }, null);
        }

        public static void PullUser() {
            var request = WebRequest.CreateHttp(Resources.getUserEndpoint.Replace("{0}", CurrentCloudsdaleUser.id));
            request.Accept = "application/json";
            request.Method = "GET";
            request.Headers["X-Auth-Token"] = CurrentCloudsdaleUser.auth_token;
            request.BeginGetResponse(ar => {
                var response = request.EndGetResponse(ar);
                var data = "";
                using (var stream = response.GetResponseStream())
                using (var sr = new StreamReader(stream)) {
                    data = sr.ReadToEnd();
                }
                var settings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    CheckAdditionalContent = false
                };
                JsonConvert.DeserializeObject<User>(data, settings).CopyTo(CurrentCloudsdaleUser);

            }, null);
        }
    }
}
