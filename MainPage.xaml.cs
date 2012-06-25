﻿using System;
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
        // Constructor
        public MainPage() {
            InitializeComponent();
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
                if (iso.FileExists("userpass"))
                    using (var file = iso.OpenFile("userpass", FileMode.Open, FileAccess.Read))
                    using (var sw = new StreamReader(file)) {
                        var user = sw.ReadLine();
                        var pass = sw.ReadLine();
                        if (user != null) UserBox.Text = user;
                        if (pass != null) PassBox.Password = pass;
                    }
        }

        private void LoginClick(object sender, RoutedEventArgs e) {
            fbbtn.IsEnabled = false;
            emailbtn.IsEnabled = false;
            EmailLogin();
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication()) {
                using (var file = iso.OpenFile("userpass", FileMode.OpenOrCreate, FileAccess.Write)) {
                    using (var sw = new StreamWriter(file)) {
                        sw.WriteLine(UserBox.Text);
                        sw.WriteLine(PassBox.Password);
                    }
                }
            }
        }

        private void FacebookClick(object sender, RoutedEventArgs e) {
            FacebookLogin();
        }

        private void TwitterClick(object sender, RoutedEventArgs e) {
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
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Incorrect email or password"));
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
    }
}