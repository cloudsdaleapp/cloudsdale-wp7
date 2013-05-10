using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;

namespace Cloudsdale {
    public partial class MainPage {
        public static bool reconstruction = false;

        private bool? isSavedLogin;

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
            ContentPanel.Visibility = Visibility.Visible;

            isSavedLogin = false;
        }

        private void LoginClick(object sender, RoutedEventArgs e) {
            reconstruction = true;
            Home.comingfromhome = false;
            LockButtons();

            if (isSavedLogin == true) {
                Connection.CurrentCloudsdaleUser = PonyvilleAccounting.Users.First(account => account.email == UserBox.Text);
                Connection.LoginType = 0;

                NavigationService.Navigate(new Uri("/Connecting.xaml", UriKind.Relative));
                Connection.Connect((Page)((PhoneApplicationFrame)Application.Current.RootVisual).Content, pulluserclouds: true);

                return;
            }

            EmailLogin();
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["email"] = UserBox.Text;
            settings.Save();
        }

        private void CreateClick(object sender, RoutedEventArgs e) {
            reconstruction = true;
            Home.comingfromhome = false;
            NavigationService.Navigate(new Uri("/Account/Register.xaml", UriKind.Relative));
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
                    UnlockButtons();
                    return;
                } catch (Exception ex) {
                    Debug.WriteLine(ex);
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Unkown error connecting to the server"));
                    UnlockButtons();
                    return;
                }
                Connection.LoginType = 0;
                var settings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    Error = (sender, args) => {
                        if (args.ErrorContext.OriginalObject is Ban) {
                            var property = args.ErrorContext.OriginalObject.GetType().GetField((string)args.ErrorContext.Member);
                            if (property.FieldType == typeof(DateTime?)) {
                                property.SetValue(args.ErrorContext.OriginalObject, (DateTime?)DateTime.Now.AddDays(2));
                                args.ErrorContext.Handled = true;
                                return;
                            }
                        }
                        Dispatcher.BeginInvoke(() => {
                            MessageBox.Show("Error receiving data from the server");
                            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                        });
                    }
                };
                Connection.LoginResult = JsonConvert.DeserializeObject<LoginResponse>(responseData, settings);
                Connection.CloudsdaleClientId = Connection.LoginResult.result.client_id;
                Connection.CurrentCloudsdaleUser = Connection.LoginResult.result.user;
                Dispatcher.BeginInvoke(() => {
                    NavigationService.Navigate(new Uri("/Connecting.xaml", UriKind.Relative));
                    Connection.Connect((Page)((PhoneApplicationFrame)Application.Current.RootVisual).Content, pulluserclouds: true);
                });
            }, null);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            UnlockButtons();

            AccountsBox.ItemsSource = PonyvilleAccounting.Users;
            isSavedLogin = false;

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

            reconstruction = false;

            base.OnNavigatedTo(e);
        }

        private void AccountsBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            isSavedLogin = false;

            var user = (LoggedInUser)((ListBox)sender).SelectedItem;
            if (user == null) return;
            UserBox.Text = user.email;
            PassBox.Password = "DUMMY_PASS";

            isSavedLogin = null;
        }

        public void LockButtons() {
            if (!Dispatcher.CheckAccess()) {
                Dispatcher.BeginInvoke(LockButtons);
                return;
            }

            UserBox.IsEnabled = false;
            PassBox.IsEnabled = false;
            emailbtn.IsEnabled = false;
            createbtn.IsEnabled = false;
            AccountsBox.IsEnabled = false;
        }

        public void UnlockButtons() {
            if (!Dispatcher.CheckAccess()) {
                Dispatcher.BeginInvoke(UnlockButtons);
                return;
            }

            UserBox.IsEnabled = true;
            PassBox.IsEnabled = true;
            emailbtn.IsEnabled = true;
            createbtn.IsEnabled = true;
            AccountsBox.IsEnabled = true;
        }

        private void UserBoxTextChanged(object sender, TextChangedEventArgs e) {
            switch (isSavedLogin) {
                case null:
                    Dispatcher.BeginInvoke(() => isSavedLogin = true);
                    return;
                case false:
                    return;
            }
            isSavedLogin = false;
            UserBox.Text = "";
            PassBox.Password = "";
            AccountsBox.SelectedIndex = -1;
        }

        private void PassBoxPasswordChanged(object sender, RoutedEventArgs e) {
            if (isSavedLogin != true) return;
            isSavedLogin = false;
            UserBox.Text = "";
            PassBox.Password = "";
            AccountsBox.SelectedIndex = -1;
        }

        private void ForgetClick(object sender, RoutedEventArgs e) {
            isSavedLogin = false;
            UserBox.Text = "";
            PassBox.Password = "";
            AccountsBox.SelectedIndex = -1;

            PonyvilleAccounting.ForgetUser((UserReference)((FrameworkElement)sender).DataContext);
        }

        private void AboutClick(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/Information/About.xaml", UriKind.Relative));
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