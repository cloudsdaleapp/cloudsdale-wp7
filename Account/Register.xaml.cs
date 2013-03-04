using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BugSense;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Account {
    public partial class Register {
        public Register() {
            InitializeComponent();
        }

        private void CreateAccountClick(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(Username.Text)) {
                MessageBox.Show("Please enter a username");
                return;
            }

            Email.IsEnabled = false;
            Password.IsEnabled = false;
            Username.IsEnabled = false;
            CreateButton.IsEnabled = false;

            CreateAccount(Email.Text, Password.Password, Username.Text);
        }

        private void CreateAccount(string email, string password, string name) {
            var data = Encoding.UTF8.GetBytes(JObject.FromObject(new {
                user = new {
                    email,
                    password,
                    name,
                }
            }).ToString());

            var request = WebRequest.CreateHttp("http://www.cloudsdale.org/v1/users");
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Method = "POST";

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
            } catch (Exception ex) {
                LoginError(ex);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            if (!Email.IsEnabled) {
                e.Cancel = true;
            }
        }

        private void ProcessError(WebException exception) {
            if (exception.Response == null) {
                LoginError(exception);
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
                Reenable();
            });
        }

        private void Reenable() {
            Dispatcher.BeginInvoke(() => {
                Email.IsEnabled = true;
                Password.IsEnabled = true;
                Username.IsEnabled = true;
                CreateButton.IsEnabled = true;
            });
        }

        private void LoginError(Exception ex, string data = null) {
            Dispatcher.BeginInvoke(() => {
                BugSenseHandler.Instance.LogError(ex, data, new NotificationOptions {
                    Type = enNotificationType.MessageBox,
                    Text = "An unknown error occurred trying to create your account! " +
                           "We have logged the error and should have a fix for the next version.",
                    Title = "That's odd...",
                });

                Email.IsEnabled = true;
                Password.IsEnabled = true;
                Username.IsEnabled = true;
                CreateButton.IsEnabled = true;
            });
        }
    }
}