using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
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

        public static void Connect(Page page = null, Dispatcher dispatcher = null, bool pulluserclouds = false) {
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

            if (!(CurrentCloudsdaleUser.is_member_of_a_cloud ?? false)) {
                JoinCloud(Resources.HammockID);
                CurrentCloudsdaleUser.is_member_of_a_cloud = true;
            }

            if (pulluserclouds) {
                PullUserClouds(() => FinishConnecting(page, dispatcher));
            } else {
                SaveUser();
                FinishConnecting(page, dispatcher);
            }
        }

        public static void FinishConnecting(Page page = null, Dispatcher dispatcher = null) {
            Faye = new FayeConnector.FayeConnector(Resources.pushUrl);

            Faye.HandshakeComplete += (sender, args) => {
                if (dispatcher == null)
                    foreach (var cloud in CurrentCloudsdaleUser.clouds) {
                        Managers.DerpyHoovesMailCenter.Subscribe(cloud.id);
                    }


                Managers.DerpyHoovesMailCenter.Init();

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

        public static void PullUserClouds(Action complete) {
            var wc = new WebClient();
            wc.DownloadStringCompleted += (sender, args) => {
                var settings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.Populate,
                };
                var result = JsonConvert.DeserializeObject<CloudGetResponse>(args.Result, settings).result;
                CurrentCloudsdaleUser.clouds = result;
                SaveUser();
                complete();
            };
            wc.DownloadStringAsync(new Uri(Resources.UserCloudsEndpoint.Replace("{userid}", CurrentCloudsdaleUser.id)));
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

        public static void JoinCloud(string id) {
            var data = new byte[0];
            var request = WebRequest.CreateHttp(Resources.JoinCloudEndpoint.Replace("{cloudid}", id));
            request.Accept = "application/json";
            request.Method = "POST";
        }

        public static void SaveUser() {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["lastuser"] = new SavedUser { id = CloudsdaleClientId, user = CurrentCloudsdaleUser };
            settings.Save();
        }
    }

    public class CloudGetResponse {
        public Cloud[] result;
    }
}
