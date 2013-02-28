using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Controls;
using Cloudsdale.Managers;
using Cloudsdale.Models;
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

            StatusBox.SelectedIndex = StatusIndex(Connection.CurrentCloudsdaleUser.status);
            Connection.CurrentCloudsdaleUser.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "status") {
                    Dispatcher.BeginInvoke( () => {
                        StatusBox.SelectionChanged -= StatusSelectionChanged;
                        StatusBox.SelectedIndex = StatusIndex(Connection.CurrentCloudsdaleUser.status);
                        StatusBox.SelectionChanged += StatusSelectionChanged;
                    });
                }
            };
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            StatusBox.SelectedIndex = StatusIndex(Connection.CurrentCloudsdaleUser.status);

            CloudList.ItemSource = Connection.CurrentCloudsdaleUser.Clouds;
            if (comingfromlogin) {
                comingfromlogin = false;
                while (NavigationService.CanGoBack) {
                    NavigationService.RemoveBackEntry();
                }
            }
            if (CurrentUser.needs_name_change ?? false) {

            }
        }

        public LoggedInUser CurrentUser {
            get { return Connection.CurrentCloudsdaleUser; }
        }


        private void ShowCloudResult(string result, bool append = false) {
            var response = JObject.Parse(result);
            var jclouds = (JArray)response["result"];
            var clouds = from jcloud in jclouds select PonyvilleDirectory.RegisterCloud(jcloud.ToObject<Cloud>());
            Dispatcher.BeginInvoke(() => {
                if (append) {
                    var old = (IEnumerable<Cloud>)searchResults.ItemsSource;
                    searchResults.ItemsSource = old.Concat(clouds);
                } else {
                    searchResults.ItemsSource = clouds;
                }
            });
        }

        private void PopularClick(object sender, RoutedEventArgs e) {
            searchResults.ItemsSource = new Cloud[0];
            WebPriorityManager.BeginHighPriorityRequest(new Uri(
                Res.PopularCloudsEndpoint), result => {
                    ShowCloudResult(result.Result);
                    WebPriorityManager.BeginHighPriorityRequest(new Uri(
                        Res.PopularCloudsEndpoint), result2 => {
                            ShowCloudResult(result2.Result, true);
                        }, new KeyValuePair<string, string>("X-Result-Page", "2"));
                });
        }

        private void RecentClick(object sender, RoutedEventArgs e) {
            searchResults.ItemsSource = new Cloud[0];
            WebPriorityManager.BeginHighPriorityRequest(new Uri(
                Res.RecentCloudsEndpoint), result => {
                    ShowCloudResult(result.Result);
                    WebPriorityManager.BeginHighPriorityRequest(new Uri(
                        Res.RecentCloudsEndpoint), result2 => {
                            ShowCloudResult(result2.Result, true);
                        }, new KeyValuePair<string, string>("X-Result-Page", "2"));
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
                    ShowCloudResult(result.Result);
                    WebPriorityManager.BeginHighPriorityRequest(new Uri(
                        Res.SearchCloudsEndpoint.Replace("{query}", SearchQuery.Text)), result2 => {
                            ShowCloudResult(result2.Result, true);
                        }, new KeyValuePair<string, string>("X-Result-Page", "2"));
                });
        }

        private void LogoutClick(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("lastuser");
            settings.Save();
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

        private void CloudListLoaded(object sender, RoutedEventArgs e) {
            CloudList.Pivot = pivotView;
        }

        private void CloudClicked(object sender, CloudTileManager.CloudEventArgs args) {
            Connection.CurrentCloud = args.Cloud;
            NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative));
        }

        private static int StatusIndex(string status) {
            switch ((status ?? "online").ToLower()) {
                case "online":
                    return 0;
                case "away":
                    return 1;
                case "busy":
                    return 2;
                default:
                    return 3;
            }
        }

        private static string StatusString(int index) {
            switch (index) {
                case 0:
                    return "online";
                case 1:
                    return "away";
                case 2:
                    return "busy";
                default:
                    return "offline";
            }
        }

        private void StatusSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (StatusBox != null)
                Connection.ModifyUserProperty("preferred_status", StatusString(StatusBox.SelectedIndex));
        }
    }
}