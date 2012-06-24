using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Cloudsdale.Models;
using CodeTitans.Bayeux;
using Microsoft.Phone.Controls;

namespace Cloudsdale {
    public static class Connection {
        public static string CurrentCloudId;
        public static string CurrentCloudName;
        public static string FacebookUid;
        public static int LoginType;
        public static object LoginResult;
        public static string CloudsdaleClientId;
        public static User CurrentCloudsdaleUser;

        private static BayeuxConnection bayeux;

        public static void Connect(Page page = null) {
            if (bayeux != null && bayeux.State == BayeuxConnectionState.Connected) return;

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

            var data = (Dictionary<string, object>)LoginResult;
            var result = (Dictionary<string, object>)data["result"];
            CloudsdaleClientId = (string)result["client_id"];
            var user = (Dictionary<string, object>)result["user"];
            CurrentCloudsdaleUser = new User(user);

            bayeux = new BayeuxConnection(Resources.pushUrl);
            bayeux.Connected += Connected;
            bayeux.ConnectionFailed += (sender, args) => Debug.WriteLine("Failed to connect");
            bayeux.DataFailed += (sender, args) => Debug.WriteLine("Data receive failed");
            bayeux.DataReceived += (sender, args) => Debug.WriteLine("Data received: " + args.Data);
            bayeux.Disconnected += (sender, args) => Debug.WriteLine("Connection lost");
            bayeux.ResponseReceived += (sender, args) => Debug.WriteLine("Response received: " + args.Message);
            Debug.WriteLine("Beginning handshake...");
            bayeux.Handshake();
        }

        static void Connected(object sender, BayeuxConnectionEventArgs e) {
            Debug.WriteLine("Handshake complete. Beginning subscribe...");
            bayeux.Connect();
            bayeux.Subscribe("/clouds/4fd8328bcff4e82543000229/chat/messages");
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
                    LoginResult = json.Read(responseData);
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
