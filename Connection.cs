using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Wp7Faye;

namespace Cloudsdale {
    /// <summary>
    /// Static class to aid in the static connection data that should be maintained for all things done with respect to the server
    /// </summary>
    public static class Connection {
        public static Cloud CurrentCloud;
        public static string FacebookUid;
        public static int LoginType;
        public static LoginResponse LoginResult;
        public static string CloudsdaleClientId;
        public static LoggedInUser CurrentCloudsdaleUser;
        public static bool TransProxWorkaround;
        public static readonly LoginState LoginState = new LoginState();

        public static MessageHandler Faye;

        public static void Connect(Page page = null, Dispatcher dispatcher = null, bool pulluserclouds = false) {
            LoginState.Message = "Logging in...";
            switch (LoginType) {
                case 0:
                    break;
                case 1:
                    // Should never be hit, but just in case
                    if (page == null) {
                        // ReSharper disable PossibleNullReferenceException
                        Deployment.Current.Dispatcher.BeginInvoke(
                            () => (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(
                                new Uri("/MainPage.xaml", UriKind.Relative)));
                        // ReSharper restore PossibleNullReferenceException
                    }
                    FacebookAuth.FBOANegotiator.FacebookLogin(page);
                    return;
                default:
                    return;
            }

            if (pulluserclouds) {
                PullUserClouds(() => {
                    if ((CurrentCloudsdaleUser.needs_name_change ?? false)
                        || string.IsNullOrWhiteSpace(CurrentCloudsdaleUser.name)) {
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            var settings = IsolatedStorageSettings.ApplicationSettings;
                            settings.Remove("lastuser");
                            settings.Save();
                            MainPage.reconstruction = true;
                            ((PhoneApplicationFrame)Application.Current.RootVisual)
                                .Navigate(new Uri("/Account/SetName.xaml", UriKind.Relative));
                        });
                        return;
                    }
                    if (!(CurrentCloudsdaleUser.has_read_tnc ?? false)) {
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            var settings = IsolatedStorageSettings.ApplicationSettings;
                            settings.Remove("lastuser");
                            settings.Save();
                            MainPage.reconstruction = true;
                            ((PhoneApplicationFrame)Application.Current.RootVisual)
                                .Navigate(new Uri("/Account/TermsAndConditions.xaml", UriKind.Relative));
                        });
                        return;
                    }
                    if ((CurrentCloudsdaleUser.suspended_until ?? new DateTime(0)) > DateTime.Now) {
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            MessageBox.Show("You are banned until" + CurrentCloudsdaleUser.suspended_until +
                                "\n" + CurrentCloudsdaleUser.reason_for_suspension);
                            Deployment.Current.Dispatcher.BeginInvoke(() => {
                                var settings = IsolatedStorageSettings.ApplicationSettings;
                                settings.Remove("lastuser");
                                settings.Save();
                                MainPage.reconstruction = true;
                                ((PhoneApplicationFrame)Application.Current.RootVisual)
                                    .Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                            });
                        });
                        return;
                    }

                    CurrentCloudsdaleUser.PropertyChanged += (sender, args) => {
                        if (args.PropertyName != "bans") return;

                        foreach (var diff in BanDifferentiation.DifferentiateBans
                            (CurrentCloudsdaleUser.old_bans, CurrentCloudsdaleUser.bans)) {
                            if (diff.isnew && diff.active == true) {
                                MessageBox.Show("You have been banned from " +
                                                PonyvilleDirectory.GetCloud(diff.cloud).name +
                                                " for \"" + diff.reason + "\" until " + diff.due,
                                                "Banned!", MessageBoxButton.OK);
                                if (CurrentCloud.id == diff.cloud) {
                                    while (((TransitionFrame)Application.Current.RootVisual).CanGoBack) {
                                        ((TransitionFrame)Application.Current.RootVisual).GoBack();
                                    }
                                }
                            } else if ((diff.due != null && diff.due < DateTime.Now) ||
                                (diff.active == false || diff.revoked == true)) {
                                MessageBox.Show("You are no longer banned from " +
                                                PonyvilleDirectory.GetCloud(diff.cloud).name +
                                                "!", "No longer banned!", MessageBoxButton.OK);
                            }
                        }
                    };

                    FinishConnecting(page, dispatcher);
                });
            } else {
                SaveUser();
                FinishConnecting(page, dispatcher);
            }
        }

        public static void FinishConnecting(Page page = null, Dispatcher dispatcher = null) {
            LoginState.Message = "Connecting...";
            Faye = Wp7Faye.Faye.Connect(Resources.pushUrl);
            Faye.ConnectTimeout += () => FinishConnectingLongPoll(page, dispatcher);

            Faye.Timeout = 10000;
            Faye.MessageExt = JObject.FromObject(new { CurrentCloudsdaleUser.auth_token });
            Faye.HandshakeResponse += response => {

                CurrentCloudsdaleUser.clouds = (from cloud in CurrentCloudsdaleUser.clouds
                                                select PonyvilleDirectory.RegisterCloud(cloud)).ToArray();

                if (dispatcher == null)
                    foreach (var cloud in CurrentCloudsdaleUser.clouds) {
                        DerpyHoovesMailCenter.Subscribe(cloud);
                    }

                DerpyHoovesMailCenter.Init();

                if (page == null) {
                    if (dispatcher != null) {
                        dispatcher.BeginInvoke(() => {
                            var phoneApplicationFrame = Application.Current.RootVisual as PhoneApplicationFrame;
                            if (phoneApplicationFrame != null)
                                phoneApplicationFrame.Navigate(new Uri("/Home.xaml", UriKind.Relative));
                        });
                    }
                } else {
                    page.Dispatcher.BeginInvoke(
                        () => page.NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative)));
                }
            };

            Faye.Connect();
        }

        public static void FinishConnectingLongPoll(Page page, Dispatcher dispatcher) {
            if (Resources.DevMessages == "true") LoginState.Message = "Falling back to long polling...";
            Faye = Wp7Faye.Faye.Connect(Resources.longPollingUrl);

            Faye.Timeout = 10000;
            Faye.MessageExt = JObject.FromObject(new { CurrentCloudsdaleUser.auth_token });
            Faye.HandshakeResponse += response => {

                CurrentCloudsdaleUser.clouds = (from cloud in CurrentCloudsdaleUser.clouds
                                                select PonyvilleDirectory.RegisterCloud(cloud)).ToArray();

                if (dispatcher == null)
                    foreach (var cloud in CurrentCloudsdaleUser.clouds) {
                        DerpyHoovesMailCenter.Subscribe(cloud);
                    }

                DerpyHoovesMailCenter.Init();

                if (page == null) {
                    if (dispatcher != null) {
                        dispatcher.BeginInvoke(() => {
                            var phoneApplicationFrame = Application.Current.RootVisual as PhoneApplicationFrame;
                            if (phoneApplicationFrame != null)
                                phoneApplicationFrame.Navigate(new Uri("/Home.xaml", UriKind.Relative));
                        });
                    }
                } else {
                    page.Dispatcher.BeginInvoke(
                        () => page.NavigationService.Navigate(new Uri("/Home.xaml", UriKind.Relative)));
                }
            };

            Faye.Connect();
        }

        public static void PullUserClouds(Action complete) {
            var token = BCrypt.Net.BCrypt.HashPassword(CurrentCloudsdaleUser.id + "cloudsdale", Resources.InternalToken);
            var oauth = string.Format(Resources.OAuthFormat, "cloudsdale", token, CurrentCloudsdaleUser.id);
            var data = Encoding.UTF8.GetBytes(oauth);
            var request = WebRequest.CreateHttp(Resources.loginUrl);
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["Content-Length"] = data.Length.ToString();
            request.BeginGetRequestStream(ar => {
                using (var stream = request.EndGetRequestStream(ar)) {
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                }
                request.BeginGetResponse(ac => {
                    try {
                        string json;
                        using (var response = request.EndGetResponse(ac))
                        using (var responseStream = response.GetResponseStream())
                        using (var responseReader = new StreamReader(responseStream)) {
                            json = responseReader.ReadToEnd();
                        }
                        LoginType = 0;
                        var settings = new JsonSerializerSettings {
                            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                            Error = (sender, args) => Deployment.Current.Dispatcher.BeginInvoke(() => {
                                MessageBox.Show("Error receiving data from the server");
                                var isettings = IsolatedStorageSettings.ApplicationSettings;
                                isettings.Remove("lastuser");
                                isettings.Save();
                                MainPage.reconstruction = true;
                                ((PhoneApplicationFrame)Application.Current.RootVisual)
                                    .Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                            })
                        };
                        LoginResult = JsonConvert.DeserializeObject<LoginResponse>(json, settings);
                        CloudsdaleClientId = LoginResult.result.client_id;
                        CurrentCloudsdaleUser = LoginResult.result.user;
                        SaveUser();
                        complete();
                    } catch {
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            if (MessageBox.Show("An error occured logging in. Retry?", "",
                                MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                                PullUserClouds(complete);
                                return;
                            }
                            var settings = IsolatedStorageSettings.ApplicationSettings;
                            settings.Remove("lastuser");
                            settings.Save();
                            MainPage.reconstruction = true;
                            ((PhoneApplicationFrame)Application.Current.RootVisual)
                                .Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                        });
                    }
                }, null);
            }, null);
        }

        public static void SendMessage(string cloud, string message, Action<JObject> callback) {
            var dataObject = new JObject();
            dataObject["content"] = message;
            dataObject["client_id"] = Faye.ClientID;
            dataObject["device"] = "mobile";
            var data = Encoding.UTF8.GetBytes(dataObject.ToString());
            var request = WebRequest.CreateHttp(Resources.SendEndpoint.Replace("{cloudid}", cloud));
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["Content-Length"] = data.Length.ToString(CultureInfo.InvariantCulture);
            request.Headers["X-Auth-Token"] = CurrentCloudsdaleUser.auth_token;
            request.BeginGetRequestStream(ar => {
                var reqs = request.EndGetRequestStream(ar);
                reqs.Write(data, 0, data.Length);
                reqs.Close();
                request.BeginGetResponse(a => {
                    string responseData;
                    using (var response = request.EndGetResponse(a))
                    using (var responseStream = response.GetResponseStream())
                    using (var responseReader = new StreamReader(responseStream, Encoding.UTF8)) {
                        responseData = responseReader.ReadToEnd();
                    }

                    if (callback != null) callback(JObject.Parse(responseData));
                }, null);
            }, null);
        }

        public static void JoinCloud(string id) {
            var jObj = new JObject();
            var dataString = jObj.ToString();
            var data = Encoding.UTF8.GetBytes(dataString);
            var request = WebRequest.CreateHttp(Resources.JoinCloudEndpoint.
                Replace("{cloudid}", id).
                Replace("{userid}", CurrentCloudsdaleUser.id));
            request.Accept = "application/json";
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Headers["Content-Length"] = data.Length.ToString();
            request.Headers["X-Auth-Token"] = CurrentCloudsdaleUser.auth_token;
            request.BeginGetRequestStream(ar => {
                var requestStream = request.EndGetRequestStream(ar);
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                request.BeginGetResponse(a => {
                    try {
                        var response = request.EndGetResponse(a);
                        string message;
                        using (var responseStream = response.GetResponseStream())
                        using (var streamReader = new StreamReader(responseStream)) {
                            message = streamReader.ReadToEnd();
                        }
                        var user = JsonConvert.DeserializeObject<WebResponse<LoggedInUser>>(message).result;
                        user.CopyTo(CurrentCloudsdaleUser);
                    } catch (WebException ex) {
#if DEBUG
                        Debugger.Break();
#endif
                    }
                }, null);
            }, null);
        }

        public static void LeaveCloud(string id) {
            Faye.Unsubscribe("/clouds/" + id + "/chat/messages");
            Faye.Unsubscribe("/clouds/" + id + "/users/**");

            var jObj = new JObject();
            var dataString = jObj.ToString();
            var data = Encoding.UTF8.GetBytes(dataString);
            var request = WebRequest.CreateHttp(Resources.LeaveCloudEndpoint.
                Replace("{cloudid}", id).
                Replace("{userid}", CurrentCloudsdaleUser.id));
            request.Accept = "application/json";
            request.Method = "DELETE";
            request.ContentType = "application/json";
            request.Headers["Content-Length"] = data.Length.ToString();
            request.Headers["X-Auth-Token"] = CurrentCloudsdaleUser.auth_token;
            request.BeginGetRequestStream(ar => {
                var requestStream = request.EndGetRequestStream(ar);
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                request.BeginGetResponse(a => {
                    try {
                        var response = request.EndGetResponse(a);
                        string message;
                        using (var responseStream = response.GetResponseStream())
                        using (var streamReader = new StreamReader(responseStream)) {
                            message = streamReader.ReadToEnd();
                        }
                        var user = JsonConvert.DeserializeObject<WebResponse<LoggedInUser>>(message).result;
                        user.CopyTo(CurrentCloudsdaleUser);
                    } catch (WebException ex) {
#if DEBUG
                        Debugger.Break();
#endif
                    }
                }, null);
            }, null);
        }

        public static void SaveUser() {
            var serial = JsonConvert.SerializeObject(new SavedUser { id = CloudsdaleClientId, user = CurrentCloudsdaleUser },
                new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                });
            Deployment.Current.Dispatcher.BeginInvoke(() => SaveUserInternal(serial));
        }

        private static void SaveUserInternal(string serial) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings["lastuser"] = serial;
            settings.Save();
        }

        public static bool IsMemberOfCloud(string cloud) {
            foreach (var c in CurrentCloudsdaleUser.clouds) {
                if (c.id == cloud) return true;
            }
            return false;
        }

        public static void ModifyUserProperty(string property, JToken value, Action success = null, Action<WebException> onError = null) {
            var properties = new JObject();
            properties[property] = value;
            ModifyUserProperty(properties, success, onError);
        }

        public static void ModifyUserProperty(JToken properties, Action success, Action<WebException> onError) {
            var requestData = new JObject();
            requestData["user"] = properties;
            var bytes = Encoding.UTF8.GetBytes(requestData.ToString());

            var request = WebRequest.CreateHttp("http://www.cloudsdale.org/v1/users/" + CurrentCloudsdaleUser.id);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers["Content-Length"] = bytes.Length.ToString();
            request.Headers["X-Auth-Token"] = CurrentCloudsdaleUser.auth_token;

            request.BeginGetRequestStream(a => {
                using (var requestStream = request.EndGetRequestStream(a)) {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
                request.BeginGetResponse(ai => {
                    try {
                        using (request.EndGetResponse(ai)) {
                            if (success != null) Deployment.Current.Dispatcher.BeginInvoke(success);
                        }
                    } catch (WebException ex) {
                        if (onError != null) onError(ex);
                    }
                }, null);
            }, null);
        }
    }

    public class CloudGetResponse {
        public Cloud[] result;
    }

    public class WebResponse<T> {
        public T result;
    }

    public class LoginState : INotifyPropertyChanged {

        private string _message;
        public string Message {
            get { return _message; }
            set {
                _message = value;
                OnPropertyChanged("Message");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                if (!Deployment.Current.Dispatcher.CheckAccess()) {
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () => handler(this, new PropertyChangedEventArgs(propertyName)));
                } else {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
        }
    }
}
