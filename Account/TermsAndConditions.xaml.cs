using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Cloudsdale.Account {
    public partial class TermsAndConditions {
        public TermsAndConditions() {
            InitializeComponent();
        }

        private void AcceptClick(object sender, RoutedEventArgs e) {
            var request = WebRequest.CreateHttp("http://www.cloudsdale.org/v1/users/:id/accept_tnc"
                                                .Replace(":id", Connection.CurrentCloudsdaleUser.id));
            request.Method = "PUT";
            request.Accept = "application/json";
            request.Headers["X-Auth-Token"] = Connection.CurrentCloudsdaleUser.auth_token;
            request.BeginGetResponse(ar => {
                using (var response = request.EndGetResponse(ar))
                using (var stream = response.GetResponseStream()) {
                    stream.Close();
                }
                Dispatcher.BeginInvoke(() => {
                    NavigationService.Navigate(new Uri("/Connecting.xaml", UriKind.Relative));
                    Connection.Connect((Page)((PhoneApplicationFrame)Application.Current.RootVisual).Content, pulluserclouds: true);
                });
            }, null);
        }

        private void RejectClick(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("lastuser");
            settings.Save();
            MainPage.reconstruction = true;
            ((PhoneApplicationFrame)Application.Current.RootVisual)
                .Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
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