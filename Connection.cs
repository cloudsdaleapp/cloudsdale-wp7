using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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

        public static void Connect(Page page = null) {
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
                    FacebookLogin(page);
                    return;
                default:
                    return;
            }

            Faye = new FayeConnector.FayeConnector(Resources.pushUrl);

            Faye.HandshakeComplete += (sender, args) => {
                if (page == null) {
                    var phoneApplicationFrame = Application.Current.RootVisual as PhoneApplicationFrame;
                    if (phoneApplicationFrame != null)
                        phoneApplicationFrame.Navigate(new Uri("/Home.xaml", UriKind.Relative));
                } else {
                    page.Dispatcher.BeginInvoke(
                        () => page.NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative)));
                }
            };

            Faye.Handshake();
        }

        static void FacebookLogin(Page page) {
#if !DEBUG
            try {
#endif
            var token = BCrypt.Net.BCrypt.HashPassword(FacebookUid + "facebook", Resources.InternalToken);
            var oauth = string.Format(Resources.OAuthFormat, "facebook", token, FacebookUid);
            var are = new AutoResetEvent(false);
            var data = Encoding.UTF8.GetBytes(oauth);
            var request = WebRequest.CreateHttp(Resources.loginUrl);
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["Content-Length"] = data.Length.ToString();
            request.BeginGetRequestStream(ar => {
                var stream = request.EndGetRequestStream(ar);
                stream.Write(data, 0, data.Length);
                stream.Close();
                are.Set();
            }, null);
            are.WaitOne();
            Debug.WriteLine("Getting response...");
            request.BeginGetResponse(ar => {
                Debug.WriteLine("Got response");
                string responseData;
                try {
                    var response = request.EndGetResponse(ar);
                    var stream = response.GetResponseStream();
                    var sr = new StreamReader(stream);
                    responseData = sr.ReadToEnd();
                    sr.Close();
                    response.Close();
                } catch (WebException ex) {
                    if (ex.Message == "The remote server returned an error: NotFound.") {
                        page.Dispatcher.BeginInvoke(() => MessageBox.Show("Login failed!"));
                    } else {
                        Debug.WriteLine(ex);
                        page.Dispatcher.BeginInvoke(() => MessageBox.Show("Unkown error connecting to the server"));
                    }
                    page.Dispatcher.BeginInvoke(() => page.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)));
                    return;
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                    page.Dispatcher.BeginInvoke(() => MessageBox.Show("Unkown error connecting to the server"));
                    page.Dispatcher.BeginInvoke(() => page.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)));
                    return;
                }
                var json = new CodeTitans.JSon.JSonReader();
                LoginType = 0;
                var settings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Error = (sender, args) => page.Dispatcher.BeginInvoke(() => {
                        MessageBox.Show("Error receiving data from the server");
                        page.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    })
                };
                LoginResult = JsonConvert.DeserializeObject<LoginResponse>(responseData, settings);
                CloudsdaleClientId = LoginResult.result.client_id;
                CurrentCloudsdaleUser = LoginResult.result.user;
                Connect(page);
            }, null);
#if !DEBUG
            } catch (Exception ex) {
                Debug.WriteLine(ex);
                page.Dispatcher.BeginInvoke(() => MessageBox.Show("Unkown error connecting to the server"));
                page.Dispatcher.BeginInvoke(() => page.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)));
            }
#endif
        }
    }
}
