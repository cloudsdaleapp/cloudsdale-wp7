using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Models;
using Newtonsoft.Json;

namespace Cloudsdale.FacebookAuth {
    public class FBOANegotiator {

        public static void FacebookLogin(Page page) {
#if !DEBUG
            try {
#endif
            var token = BCrypt.Net.BCrypt.HashPassword(Connection.FacebookUid + "facebook", Resources.InternalToken);
            var oauth = string.Format(Resources.OAuthFormat, "facebook", token, Connection.FacebookUid);
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
                Connection.LoginType = 0;
                var settings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Error = (sender, args) => page.Dispatcher.BeginInvoke(() => {
                        MessageBox.Show("Error receiving data from the server");
                        page.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    })
                };
                Connection.LoginResult = JsonConvert.DeserializeObject<LoginResponse>(responseData, settings);
                Connection.CloudsdaleClientId = Connection.LoginResult.result.client_id;
                Connection.CurrentCloudsdaleUser = Connection.LoginResult.result.user;
                Connection.Connect(page);
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
