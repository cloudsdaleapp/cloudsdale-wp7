using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;

namespace Cloudsdale {
    public partial class MainPage {
        public static bool reconstruction = false;

        // Constructor
        public MainPage() {
            InitializeComponent();
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("lastuser")) {
                try {
                    var user = JsonConvert.DeserializeObject<SavedUser>((string)settings["lastuser"]);
                    if (user.user != null) {
                        Connection.CloudsdaleClientId = user.id;
                        Connection.CurrentCloudsdaleUser = user.user;
                        Dispatcher.BeginInvoke(() => {
                            NavigationService.Navigate(new Uri("/Connecting.xaml", UriKind.Relative));
                            Connection.Connect(dispatcher: Dispatcher, pulluserclouds: true);
                        });
                        return;
                    }
                } catch (Exception e) {
                    Debug.WriteLine(e);
                    settings.Remove("lastuser");
                }
            }
            if (settings.Contains("email")) {
                UserBox.Text = (string)settings["email"];
            }
            ContentPanel.Visibility = Visibility.Visible;
        }

        private void LoginClick(object sender, RoutedEventArgs e) {
            reconstruction = true;
            Home.comingfromhome = false;
            fbbtn.IsEnabled = false;
            emailbtn.IsEnabled = false;
            EmailLogin();
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["email"] = UserBox.Text;
            settings.Save();
        }

        private void FacebookClick(object sender, RoutedEventArgs e) {
            reconstruction = true;
            Home.comingfromhome = false;
            FacebookLogin();
        }

        private void TwitterClick(object sender, RoutedEventArgs e) {
            reconstruction = true;
            Home.comingfromhome = false;
            TwitterLogin();
        }

        private void EmailLogin() {
            var are = new AutoResetEvent(false);
            var data = Encoding.UTF8.GetBytes("email=" + Uri.EscapeUriString(UserBox.Text) +
                "&password=" + Uri.EscapeUriString(PassBox.Password));
            var request = WebRequest.CreateHttp(Cloudsdale.Resources.loginUrl);
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
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
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Couldn't log in"));
                    } else {
                        Debug.WriteLine(ex);
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Unkown error connecting to the server"));
                    }
                    Dispatcher.BeginInvoke(() => fbbtn.IsEnabled = true);
                    Dispatcher.BeginInvoke(() => emailbtn.IsEnabled = true);
                    return;
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Unkown error connecting to the server"));
                    Dispatcher.BeginInvoke(() => fbbtn.IsEnabled = true);
                    Dispatcher.BeginInvoke(() => emailbtn.IsEnabled = true);
                    return;
                }
                Connection.LoginType = 0;
                var settings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Error = (sender, args) => Dispatcher.BeginInvoke(() => {
                        MessageBox.Show("Error receiving data from the server");
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    })
                };
                Connection.LoginResult = JsonConvert.DeserializeObject<LoginResponse>(responseData, settings);
                Connection.CloudsdaleClientId = Connection.LoginResult.result.client_id;
                Connection.CurrentCloudsdaleUser = Connection.LoginResult.result.user;
                Dispatcher.BeginInvoke(() => {
                    NavigationService.Navigate(new Uri("/Connecting.xaml", UriKind.Relative));
                    Connection.Connect((Page)((PhoneApplicationFrame)Application.Current.RootVisual).Content);
                });
            }, null);
        }

        private void FacebookLogin() {
            NavigationService.Navigate(new Uri("/FacebookAuth/Login.xaml", UriKind.Relative));
        }

        private void TwitterLogin() {

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            fbbtn.IsEnabled = true;
            emailbtn.IsEnabled = true;

            try {
                while (NavigationService.CanGoBack) {
                    NavigationService.RemoveBackEntry();
                }
            } catch (Exception x) {
                Debug.WriteLine("Error removing backstack: " + x);
            }

            if (!reconstruction && e.IsNavigationInitiator && IsolatedStorageSettings.ApplicationSettings.Contains("lastuser")) {
                throw new ApplicationTerminationException();
            }

            if (reconstruction) reconstruction = false;

            base.OnNavigatedTo(e);
        }
    }

    [JsonObject]
    public class SavedUser {
        [JsonProperty]
        public LoggedInUser user;
        [JsonProperty]
        public string id;
    }

    public class ApplicationTerminationException : Exception {
    }
}