using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cloudsdale {
    public partial class CreateCloud {
        public CreateCloud() {
            InitializeComponent();
        }

        private void DoneClick(object sender, RoutedEventArgs e) {
            CloudName.IsEnabled = false;
            DoneBtn.IsEnabled = false;
            PostData(CloudName.Text);
        }

        private void PostData(string name) {
            var data = Encoding.UTF8.GetBytes(JObject.FromObject(new {
                cloud = new { name }
            }).ToString());

            var request = WebRequest.CreateHttp("http://www.cloudsdale.org/v1/clouds");
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers["X-Auth-Token"] = Connection.CurrentCloudsdaleUser.auth_token;
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

                Cloud cloud;
                using (var response = request.EndGetResponse(asyncResult))
                using (var responseStream = response.GetResponseStream())
                using (var responseReader = new StreamReader(responseStream)) {
                    cloud = JObject.Parse(responseReader.ReadToEnd())["result"].ToObject<Cloud>();
                }

                Dispatcher.BeginInvoke(() => {
                    if (cloud.is_transient ?? true) {
                        MessageBox.Show("Failure creating cloud (maybe one with the same name already exists?)");
                        CloudName.IsEnabled = true;
                        DoneBtn.IsEnabled = true;
                        return;
                    }

                    Connection.CurrentCloudsdaleUser.clouds = Connection.CurrentCloudsdaleUser.clouds.Concat(new[] { PonyvilleDirectory.RegisterCloud(cloud) }).ToArray();
                    Connection.CurrentCloud = PonyvilleDirectory.RegisterCloud(cloud);
                    NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative));
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
                CloudName.IsEnabled = true;
                DoneBtn.IsEnabled = true;
            });
        }
    }
}