using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cloudsdale.Avatars;
using Cloudsdale.Controls;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using NdefLibrary.Ndef;
using Newtonsoft.Json.Linq;
using Windows.Networking.Proximity;
using Windows.System;
using Res = Cloudsdale.Resources;

namespace Cloudsdale {
    public partial class Home {
        public static bool comingfromhome = false;
        public static bool comingfromlogin = true;

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private bool initializedFont;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        public static readonly string[] Fonts = new[] {
            "Calibri", "Comic Sans MS", "Courier New",
            "Segoe WP", "Tahoma", "Times New Roman", "Verdana"
        };

        public static readonly ObservableCollection<Cloud> ExploreClouds = new ObservableCollection<Cloud>();
        public static readonly Uri CloudsdaleApiBase = new Uri("http://www.cloudsdale.org/v1/");

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

            FontPicker.SetValue(ListPicker.ItemCountThresholdProperty, 12);
            foreach (var font in Fonts) {
                var fonttext = font;
                FontPicker.Items.Add(new ListPickerItem { Content = fonttext, FontFamily = new FontFamily(font) });
            }
            initializedFont = true;
            var index = Array.IndexOf(Fonts, ((App)Application.Current).ChatFont.Source);
            FontPicker.SelectedIndex = index < 0 ? 0 : index;

            ThemePicker.SetValue(ListPicker.ItemCountThresholdProperty, 8);
            ThemePicker.SelectedIndex = Math.Max(Array.IndexOf(App.ThemeColors,
                ((SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]).Color), 0);

            if (Connection.LaunchedUri != null) {
                var cloudUrl = new Uri(CloudsdaleApiBase, "clouds" + Connection.LaunchedUri.AbsolutePath);
                Connection.LaunchedUri = null;
                WebPriorityManager.BeginHighPriorityRequest(cloudUrl, args => {
                    try {
                        var cloud = PonyvilleDirectory.RegisterCloud(JObject.Parse(args.Result)["result"].ToObject<Cloud>());
                        Connection.CurrentCloud = cloud;
                        Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative)));
                    } catch {
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Couldn't open cloud! (does it exist?)"));
                    }
                });
            }

            ExplorePanel.DataContext = Connection.CurrentCloudsdaleUser;

            SetupNdef();
        }

        void SetupNdef() {
            //var device = ProximityDevice.GetDefault();
            //if (device == null) return;
            //device.SubscribeForMessage("NDEF", (sender, message) => {
            //    var data = NdefMessage.FromByteArray(message.Data.ToArray());
            //    foreach (var record in data) {
            //        Uri uri;
            //        try {
            //            var poster = new NdefSpRecord(record);
            //            uri = new Uri(poster.Uri);
            //        } catch {
            //            continue;
            //        }
            //        if (uri.Scheme != "cloudsdale") continue;
            //        Launcher.LaunchUriAsync(uri);
            //    }
            //});
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

        private void AboutClick(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/Information/About.xaml", UriKind.Relative));
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
            Connection.Faye.Disconnect();

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

            if (cloud.IsBannedFrom) {
                var ban = cloud.ApplicableBan;
                MessageBox.Show("You have been banned from " + cloud.name + " for the reason \"" + ban.reason +
                                "\" until " + ban.due, "You are banned!", MessageBoxButton.OK);
                return;
            }

            Connection.CurrentCloud = cloud;
            NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative));
        }

        private void CloudListLoaded(object sender, RoutedEventArgs e) {
            CloudList.Pivot = pivotView;
        }

        private void CloudClicked(object sender, CloudTileManager.CloudEventArgs args) {
            var cloud = args.Cloud;

            if (cloud.IsBannedFrom) {
                var ban = cloud.ApplicableBan;
                MessageBox.Show("You have been banned from " + cloud.name + " for the reason \"" + ban.reason +
                                "\" until " + ban.due, "You are banned!", MessageBoxButton.OK);
                return;
            }

            Connection.CurrentCloud = cloud;
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
        private void FontPickerSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!initializedFont) return;
            var font = new FontFamily(Fonts[FontPicker.SelectedIndex]);
            ((App)Application.Current).ChatFont = font;
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["chatfont"] = font.Source;
            settings.Save();
        }

        private void ThemePickerSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ThemePicker == null) return;

            var settings = IsolatedStorageSettings.ApplicationSettings;
            var newcolor = App.ThemeColors[ThemePicker.SelectedIndex];

            if (settings.Contains("theme") && (Color)settings["theme"] == newcolor) return;

            ((SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]).Color = newcolor;
            settings["theme"] = newcolor;
            settings.Save();

            LayoutRoot.InvalidateArrange();
        }

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
            ChangeAvatar.target = CurrentUser;
            NavigationService.Navigate(new Uri("/Avatars/ChangeAvatar.xaml", UriKind.Relative));
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

        private void CreateCloudClick(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/CreateCloud.xaml", UriKind.Relative));
        }

    }
}