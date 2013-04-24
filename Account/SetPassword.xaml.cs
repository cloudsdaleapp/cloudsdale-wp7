using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Account {
    public partial class SetPassword {
        public SetPassword() {
            InitializeComponent();
        }

        private void DoneClick(object sender, RoutedEventArgs e) {
            Password.IsEnabled = false;
            PostData(Password.Password);
        }

        private void PostData(string password) {
            var data = Encoding.UTF8.GetBytes(JObject.FromObject(new {
                user = new { password }
            }).ToString());

            var request = WebRequest.CreateHttp("http://www.cloudsdale.org/v1/users/" +
                                                Connection.CurrentCloudsdaleUser.id);
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers["X-Auth-Token"] = Connection.CurrentCloudsdaleUser.auth_token;
            request.Method = "PUT";

            request.BeginGetRequestStream(ar => {
                using (var stream = request.EndGetRequestStream(ar)) {
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                }

                request.BeginGetResponse(OnResponse, request);
            }, null);
        }

        private void OnResponse(IAsyncResult asyncResult) {
            try {
                var request = (HttpWebRequest)asyncResult.AsyncState;

                string responseData;
                using (var response = request.EndGetResponse(asyncResult))
                using (var responseStream = response.GetResponseStream())
                using (var responseReader = new StreamReader(responseStream)) {
                    responseData = responseReader.ReadToEnd();
                }

                Connection.LoginType = 0;
                var settings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Error = (sender, args) => Dispatcher.BeginInvoke(() => {
                        MessageBox.Show("Error receiving data from the server");
                        NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                    })
                };
                var regResponse = JsonConvert.DeserializeObject<RegistrationResponse>(responseData, settings);
                Connection.LoginResult = new LoginResponse {
                    errors = new object[0],
                    flash = null,
                    status = 200,
                    result = new Result {
                        client_id = Guid.NewGuid().ToString(),
                        user = regResponse.result
                    }
                };
                Connection.CloudsdaleClientId = Connection.LoginResult.result.client_id;
                Connection.CurrentCloudsdaleUser = regResponse.result;
                Dispatcher.BeginInvoke(() => {
                    NavigationService.Navigate(new Uri("/Connecting.xaml", UriKind.Relative));
                    Connection.Connect((Page)((PhoneApplicationFrame)Application.Current.RootVisual).Content, pulluserclouds: true);
                });
            } catch (WebException ex) {
                ProcessError(ex);
            }
        }

        private void ProcessError(WebException exception) {
            if (exception.Response == null) {
                return;
            }

            var response = (HttpWebResponse)exception.Response;
            JObject data;
            using (var responseStream = response.GetResponseStream())
            using (var responseReader = new StreamReader(responseStream)) {
                data = JObject.Parse(responseReader.ReadToEnd());
            }
            Dispatcher.BeginInvoke(() => {
                if (data["flash"] != null) {
                    var message = data["flash"]["message"].ToString();
                    message = data["errors"].Aggregate(message, (current, error) => current +
                        ("\n - " + error["ref_node"].ToString().UppercaseFirst() + " \"" +
                        data["result"][error["ref_node"].ToString()] + "\" " + error["message"]));
                    MessageBox.Show(message, data["flash"]["title"].ToString(),
                                    MessageBoxButton.OK);
                } else {
                    MessageBox.Show("An unknown error occurred trying to register.");
                }
                Password.IsEnabled = true;
            });
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("lastuser");
            settings.Save();
            MainPage.reconstruction = true;
            ((PhoneApplicationFrame)Application.Current.RootVisual)
                .Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}