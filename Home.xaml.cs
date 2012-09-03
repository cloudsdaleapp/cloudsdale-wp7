using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Res = Cloudsdale.Resources;

namespace Cloudsdale {
    public partial class Home {
        public static bool comingfromhome = false;
        public static bool comingfromlogin = true;

        public static readonly ObservableCollection<Cloud> ExploreClouds = new ObservableCollection<Cloud>(); 

        public Home() {
            comingfromhome = true;

            InitializeComponent();


            UserInfoPane.DataContext = CurrentUser;

            searchResults.ItemsSource = ExploreClouds;

            PopularClick(null, null);

            if (IsolatedStorageSettings.ApplicationSettings.Contains("ux.allowaltcodes")) {
                allowaltcodes.IsChecked = true;
                recursivealtcodes.Visibility = Visibility.Visible;
            }
            recursivealtcodes.IsChecked =
                IsolatedStorageSettings.ApplicationSettings.Contains("ux.recursivealtcodeentry");
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            CloudList.ItemsSource = Connection.CurrentCloudsdaleUser.Clouds;
            if (comingfromlogin) {
                comingfromlogin = false;
                while (NavigationService.CanGoBack) {
                    NavigationService.RemoveBackEntry();
                }
            }
        }

        public LoggedInUser CurrentUser {
            get { return Connection.CurrentCloudsdaleUser; }
        }

        private void PopularClick(object sender, RoutedEventArgs e) {
            searchResults.ItemsSource = new Cloud[0];
            WebPriorityManager.BeginHighPriorityRequest(new Uri(
                Res.PopularCloudsEndpoint), result => {
                    var response = JObject.Parse(result.Result);
                    var jclouds = (JArray)response["result"];
                    var clouds = from jcloud in jclouds select PonyvilleDirectory.RegisterCloud(jcloud.ToObject<Cloud>());
                    Dispatcher.BeginInvoke(() => {
                        searchResults.ItemsSource = clouds;
                    });
                });
        }

        private void RecentClick(object sender, RoutedEventArgs e) {
            searchResults.ItemsSource = new Cloud[0];
            WebPriorityManager.BeginHighPriorityRequest(new Uri(
                Res.RecentCloudsEndpoint), result => {
                    var response = JObject.Parse(result.Result);
                    var jclouds = (JArray)response["result"];
                    var clouds = from jcloud in jclouds select PonyvilleDirectory.RegisterCloud(jcloud.ToObject<Cloud>());
                    Dispatcher.BeginInvoke(() => {
                        searchResults.ItemsSource = clouds;
                    });
                });
        }

        private void SearchClick(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(SearchQuery.Text)) {
                MessageBox.Show("Please enter some text");
                return;
            }


            searchResults.ItemsSource = new Cloud[0];
            WebPriorityManager.BeginHighPriorityRequest(new Uri(
                Res.SearchCloudsEndpoint.Replace("{query}", SearchQuery.Text)), result => {
                    var response = JObject.Parse(result.Result);
                    var jclouds = (JArray)response["result"];
                    var clouds = from jcloud in jclouds select PonyvilleDirectory.RegisterCloud(jcloud.ToObject<Cloud>());
                    Dispatcher.BeginInvoke(() => {
                        searchResults.ItemsSource = clouds;
                    });
                });
        }

        private void LogoutClick(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("lastuser");
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void AllowaltcodesChecked(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["ux.allowaltcodes"] = true;
            settings.Save();
            recursivealtcodes.Visibility = Visibility.Visible;
        }

        private void AllowaltcodesUnchecked(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("ux.allowaltcodes");
            settings.Save();
            recursivealtcodes.Visibility = Visibility.Collapsed;
        }

        private void RecursivealtcodesChecked(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["ux.recursivealtcodeentry"] = true;
            settings.Save();
        }

        private void RecursivealtcodesUnchecked(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("ux.recursivealtcodeentry");
            settings.Save();
        }

        private void CloudClick(object sender, RoutedEventArgs e) {
            var cloud = (Cloud)((FrameworkElement)sender).DataContext;
            Connection.CurrentCloud = cloud;
            NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative));
        }
    }
}