using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Controls;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json.Linq;
using Res = Cloudsdale.Resources;

namespace Cloudsdale {
    public partial class Home {
        public static bool comingfromhome = false;
        public static bool comingfromlogin = true;

        public static readonly ObservableCollection<Cloud> ExploreClouds = new ObservableCollection<Cloud>();

        #region Page stuffs
        public Home() {
            comingfromhome = true;

            InitializeComponent();

            UserInfoPane.DataContext = PonyvilleCensus.Heartbeat(CurrentUser);

            searchResults.ItemsSource = ExploreClouds;

            PopularClick(null, null);

            if (IsolatedStorageSettings.ApplicationSettings.Contains("ux.allowaltcodes")) {
                allowaltcodes.IsChecked = true;
                //recursivealtcodes.Visibility = Visibility.Visible;
            }
            recursivealtcodes.IsChecked =
                IsolatedStorageSettings.ApplicationSettings.Contains("ux.recursivealtcodeentry");

            StatusBox.SelectedIndex = StatusIndex(Connection.CurrentCloudsdaleUser.status);
            Connection.CurrentCloudsdaleUser.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "status") {
                    Dispatcher.BeginInvoke(() => {
                        StatusBox.SelectionChanged -= StatusSelectionChanged;
                        StatusBox.SelectedIndex = StatusIndex(Connection.CurrentCloudsdaleUser.status);
                        StatusBox.SelectionChanged += StatusSelectionChanged;
                    });
                }
            };

            if (Connection.CurrentCloudsdaleUser.needs_to_confirm_registration ?? false) {
                Connection.ModifyUserProperty("confirm_registration", true);
                Connection.CurrentCloudsdaleUser.needs_to_confirm_registration = false;
            }

            SettingsPanel.DataContext = CurrentUser;
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
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            if (pivotView.SelectedIndex != 0) {
                e.Cancel = true;
                pivotView.SelectedIndex = 0;
            }
        }

        public LoggedInUser CurrentUser {
            get { return Connection.CurrentCloudsdaleUser; }
        }
        #endregion

        #region Explore
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
        #endregion

        #region Logout
        private void LogoutClick(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("lastuser");
            settings.Save();
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
        #endregion

        #region Alt Codes
        private void AllowaltcodesChecked(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["ux.allowaltcodes"] = true;
            settings.Save();
            //recursivealtcodes.Visibility = Visibility.Visible;
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
        #endregion

        #region Clouds
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
        #endregion

        #region User Status
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
        #endregion

        #region Settings codez
        private void ChangeUsernameAccess(bool enabled) {
            Username.IsEnabled = enabled;
            UsernameBtn.IsEnabled = enabled;
        }
        private void ChangeEmailAccess(bool enabled) {
            Email.IsEnabled = enabled;
            EmailBtn.IsEnabled = enabled;
        }
        private void ChangeSkypeAccess(bool enabled) {
            Skype.IsEnabled = enabled;
            SkypeBtn.IsEnabled = enabled;
        }
        private void ChangePasswordAccess(bool enabled) {
            NewPassword.IsEnabled = enabled;
            NewPasswordBtn.IsEnabled = enabled;
        }

        private void ChangeNameClick(object sender, RoutedEventArgs e) {
            ChangeUsernameAccess(false);
            Connection.ModifyUserProperty("name", Username.Text, () => ChangeUsernameAccess(true),
                ex => ProcessError(ex, ChangeUsernameAccess));
        }

        private void ChangeEmailClick(object sender, RoutedEventArgs e) {
            ChangeEmailAccess(false);
            Connection.ModifyUserProperty("email", Email.Text, () => ChangeEmailAccess(true),
                ex => ProcessError(ex, ChangeEmailAccess));
        }

        private void ChangeSkypeClick(object sender, RoutedEventArgs e) {
            ChangeSkypeAccess(false);
            Connection.ModifyUserProperty("skype_name", Skype.Text, () => ChangeSkypeAccess(true),
                ex => ProcessError(ex, ChangeSkypeAccess));
        }

        private void ChangePasswordClick(object sender, RoutedEventArgs e) {
            ChangePasswordAccess(false);
            Connection.ModifyUserProperty("password", NewPassword.Password, () => ChangePasswordAccess(true),
                ex => ProcessError(ex, ChangePasswordAccess));
        }

        private void ChangeAvatarClick(object sender, RoutedEventArgs e) {
            //MessageBox.Show("We're sorry, you can't change your avatar yet in this version of the app. " +
            //                "You need to go to the website to change it.");
            var picChooser = new PhotoChooserTask();
            picChooser.Completed += (o, result) => Connection.CurrentCloudsdaleUser.UploadAvatar(result);
            picChooser.Show();
        }

        private void ProcessError(WebException exception, Action<bool> resetFields) {
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
                        ("\n - " + error["ref_node"].ToString().UppercaseFirst() 
                        + (string.IsNullOrWhiteSpace("" + data["result"][error["ref_node"].ToString()]) ? ": " 
                        : " \"" + data["result"][error["ref_node"].ToString()] + "\" ") + error["message"]));
                    MessageBox.Show(message, data["flash"]["title"].ToString(),
                                    MessageBoxButton.OK);
                } else {
                    MessageBox.Show("An unknown error occurred trying to update your profile.");
                }
                resetFields(true);
            });
        }
        #endregion
    }
}