using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if DEBUG
using System.Diagnostics;
#endif
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cloudsdale.Models;
using Cloudsdale.Settings;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;
using Windows.Phone.Speech.Synthesis;

namespace Cloudsdale.Managers {
    /// <summary>
    /// Used to control all incoming data for a cloud
    /// </summary>
    public class DerpyHoovesMailCenter : INotifyPropertyChanged {
        internal static Timer PresenceAnnouncer = null;

        public static TimeSpan ServerDiff = new TimeSpan();

        static DerpyHoovesMailCenter() {
            ServerDiff = new TimeSpan();
        }

        private DerpyHoovesMailCenter(Cloud cloud) {
            users = new PonyTracker(cloud);
            drops = PinkiePieEntertainmentDojo.GetForCloud(cloud.id);
        }

        public static readonly Dictionary<string, bool> ValidPreloadedData = new Dictionary<string, bool>();

        public readonly object Lock = new object();

        public int Unread {
            get { return unread; }
            set {
                unread = value;
                OnPropertyChanged("Unread");
            }
        }

        private static readonly Dictionary<string, DerpyHoovesMailCenter> Cache =
            new Dictionary<string, DerpyHoovesMailCenter>();
        public static void Init() {

            ValidPreloadedData.Clear();

            Connection.Faye.MessageRecieved += FayeMessageRecieved;

            Connection.Faye.Subscribe("/users/" + Connection.CurrentCloudsdaleUser.id + "/private");
            Connection.Faye.Subscribe("/users/" + Connection.CurrentCloudsdaleUser.id + "/bans");
        }

        static void FayeMessageRecieved(JObject jobj) {
            try {
                if (jobj["successful"] != null) return;

                var chansplit = jobj["channel"].ToString().ToLower().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (chansplit.Length < 2) return;
                if (!Cache.ContainsKey(chansplit[1])) return;
                if (chansplit.Length == 2 || (chansplit.Length == 3 && chansplit[0] == "users")) {
                    switch (chansplit[0]) {
                        case "clouds":
                            Deployment.Current.Dispatcher.BeginInvoke(
                                () => PonyvilleDirectory.GetCloud(chansplit[1]).UpdateCloud(jobj));
                            break;
                        case "users":
                            switch (chansplit[2]) {
                                case "private":
                                    var user = jobj["data"].ToObject<User>();
                                    user.CopyTo(Connection.CurrentCloudsdaleUser);
                                    if ((Connection.CurrentCloudsdaleUser.suspended_until ?? new DateTime(0)) > DateTime.Now) {
                                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                                            MessageBox.Show("You are banned until" + Connection.CurrentCloudsdaleUser.suspended_until +
                                                            "\n" + Connection.CurrentCloudsdaleUser.reason_for_suspension);
                                            throw new ApplicationTerminationException();
                                        });
                                    }
                                    break;
                                case "bans":
                                    var ban = jobj["data"].ToObject<Ban>();
                                    var bans = Connection.CurrentCloudsdaleUser.bans.ToList();
                                    var dupe = bans.FirstOrDefault(b => b.id == ban.id);
                                    if (dupe != null) bans.Remove(dupe);
                                    bans.Add(ban);
                                    Connection.CurrentCloudsdaleUser.bans = bans.ToArray();
                                    break;
                            }
                            break;
                    }
                } else {
                    if (chansplit[0] != "clouds") return;
                    switch (chansplit[2]) {
                        case "drops":
                            var drop = jobj["data"].ToObject<Drop>();
                            Cache[chansplit[1]].drops.AddDrop(drop);
                            break;
                        case "users":
                            if (chansplit.Length < 4) break;
                            var data = jobj["data"];

                            if (data["status"] != null) {
                                if (data["id"] == null)
                                    data["id"] = chansplit[3];
                                Cache[chansplit[1]].users.Heartbeat(data);
                            }
                            break;
                        case "chat":
                            var message = jobj["data"].ToObject<Message>();
                            if (message == null || message.user == null || message.content == null) break;

                            ServerDiff = message.timestamp - DateTime.Now;

                            var cache = Cache[chansplit[1]];
                            lock (cache.Lock) {
                                if (message.client_id == Connection.Faye.ClientID) {
                                    break;
                                }
                                cache.messages.AddToEnd(message);
                                var cloud = PonyvilleDirectory.GetCloud(chansplit[1]);

                                if (Connection.CurrentCloud == null || cloud.id != Connection.CurrentCloud.id) {
                                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                                        //var synth = new SpeechSynthesizer();
                                        //synth.SpeakTextAsync(message.content);

                                        var chromeColor = ((SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]).Color;
                                        chromeColor.R = (byte)Math.Max(0x1A, chromeColor.R - 30);
                                        chromeColor.G = (byte)Math.Max(0x1A, chromeColor.G - 30);
                                        chromeColor.B = (byte)Math.Max(0x1A, chromeColor.B - 30);

                                        var linebreak = new Regex(@"((\r|\n)(\n)?)");
                                        var msg = StringParser.ParseLiteral(message.content);
                                        msg = linebreak.Replace(msg, " ");

                                        var toast = new ToastPrompt {
                                            Title = cloud.name + " - " + message.user.name,
                                            TextOrientation = Orientation.Vertical,
                                            Message = msg,
                                            ImageSource = new BitmapImage(message.user.avatar.Mini),
                                            Background = new SolidColorBrush(chromeColor)
                                        };
                                        toast.Tap += (sender, args) => {
                                            var rootVis = (TransitionFrame)Application.Current.RootVisual;
                                            var content = (Page)rootVis.Content;
                                            if (content is Clouds) {
                                                (content as Clouds).NavigateCloud(cloud);
                                            } else if (content is Home) {
                                                Connection.CurrentCloud = cloud;
                                                content.NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative));
                                            } else {
                                                while (content.NavigationService.CanGoBack) {
                                                    content.NavigationService.GoBack();
                                                }

                                                Connection.CurrentCloud = cloud;
                                                content.NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative));
                                            }
                                        };
                                        toast.Show();
                                    });
                                }
                            }
                            cache.Unread++;
                            break;

                    }
                }
            } catch (Exception e) {
#if DEBUG
                Debugger.Break();
#else
                BugSense.BugSenseHandler.Instance.LogError(e);
#endif
            }
        }

        public static DerpyHoovesMailCenter GetCloud(Cloud cloud) {
            return Cache.ContainsKey(cloud.id) ? Cache[cloud.id] : Subscribe(cloud);
        }

        internal readonly SweetAppleAcres messages = new SweetAppleAcres(50);
        private readonly PinkiePieEntertainmentDojo drops;
        private readonly PonyTracker users;
        private readonly GenericBinding<String> textblockbinding =
            new GenericBinding<string>(TextBlock.TextProperty);
        private int unread;
        public void MarkAsRead() {
            Unread = 0;
        }

        public SweetAppleAcres MessageController {
            get { return messages; }
        }

        public ObservableCollection<Message> Messages {
            get { return messages.Cache; }
        }
        public ObservableCollection<Drop> Drops {
            get { return drops.Cache; }
        }
        public PinkiePieEntertainmentDojo DropController {
            get { return drops; }
        }
        public PonyTracker Users {
            get { return users; }
        }

        public static DerpyHoovesMailCenter Subscribe(Cloud cloud) {
            lock (Cache) {
                if (!Cache.ContainsKey(cloud.id)) {
                    Cache[cloud.id] = new DerpyHoovesMailCenter(cloud);
                }
            }
            if (Connection.Faye.IsSubscribed("/clouds/" + cloud.id)) {
                return Cache[cloud.id];
            }
            Connection.Faye.Subscribe("/clouds/" + cloud.id);
            Connection.Faye.Subscribe("/clouds/" + cloud.id + "/chat/messages");
            Connection.Faye.Subscribe("/clouds/" + cloud.id + "/drops");

            WebPriorityManager.BeginMediumPriorityRequest(
                new Uri(Resources.PreviousDropsEndpoint.Replace("{cloudid}", cloud.id)), eventArgs => {
                    try {
                        var result = JsonConvert.DeserializeObject<WebDropResponse>(eventArgs.Result);
                        var drops = result.result;
                        if (!Cache.ContainsKey(cloud.id)) return;
                        Cache[cloud.id].drops.PreLoad(drops);
                    } catch (WebException) {
                    }
                });

            return Cache[cloud.id];
        }

        public static void VerifyCloud(string id, Action onComplete = null) {
            if (ValidPreloadedData.ContainsKey(id) && ValidPreloadedData[id]) {
                if (onComplete != null) Deployment.Current.Dispatcher.BeginInvoke(onComplete);
                return;
            }
            ValidPreloadedData[id] = true;

            Connection.Faye.Subscribe("/clouds/" + id + "/users/*");

            WebPriorityManager.BeginHighPriorityRequest(new Uri(Resources.PreviousMessagesEndpoint.Replace("{cloudid}", id)), e => {
                try {
                    var result = JsonConvert.DeserializeObject<WebMessageResponse>(e.Result);
                    if (!Cache.ContainsKey(id)) return;
                    Cache[id].Users.Init();
                    lock (Cache[id].Lock) {
                        var oldmsgs = Cache[id].Messages.Where(msg => msg.timestamp > result.result.Last().timestamp).ToArray();
                        Cache[id].messages.Clear();
                        foreach (var m in result.result.Where(m => m != null && m.user != null && m.id != null && m.content != null)) {
                            Cache[id].messages.AddToEnd(m);
                        }
                        foreach (var m in oldmsgs) {
                            Cache[id].messages.AddToEnd(m);
                        }
                    }
                    if (onComplete != null) Deployment.Current.Dispatcher.BeginInvoke(onComplete);
                } catch (WebException) {
                }
            });

            if (!Cache.ContainsKey(id)) return;
            Cache[id].Users.Init();
        }

        public static void Unsubscribe(string cloud) {
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/chat/messages");
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/drops");
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/users/**");
            if (Cache.ContainsKey(cloud)) Cache.Remove(cloud);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string LastMessage {
            get { return messages.LastMessage; }
        }
    }

    public class FayeResult<T> {
        public string channel;
        public T data { get; set; }
    }
}
