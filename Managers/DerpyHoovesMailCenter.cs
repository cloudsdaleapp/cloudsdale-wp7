using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if DEBUG
using System.Diagnostics;
#endif
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Models;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;

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

            Connection.Faye.ChannelMessageRecieved += FayeMessageRecieved;
            //if (PresenceAnnouncer == null) PresenceAnnouncer = new Timer(o => {
            //    Thread.CurrentThread.Name = "PresenceAnnouncement";
            //    foreach (var cloud in Cache.Keys) {
            //        Connection.Faye.Publish("/clouds/" + cloud + "/users/" +
            //            Connection.CurrentCloudsdaleUser.id, new object());
            //    }
            //}, null, 5000, 30000);

            Connection.Faye.Subscribe("/users/" + Connection.CurrentCloudsdaleUser.id + "/private");
        }

        static void FayeMessageRecieved(object sender, FayeConnector.FayeConnector.DataReceivedEventArgs args) {
            try {
                var jobj = JObject.Parse(args.Data);
                if (jobj.Root["successful"] != null) return;

                var chansplit = args.Channel.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (chansplit.Length < 2) return;
                if (!Cache.ContainsKey(chansplit[1])) return;
                if (chansplit.Length == 2 || (chansplit.Length == 3 && chansplit[2] == "private")) {
                    if (chansplit[0] == "clouds") {
                        Deployment.Current.Dispatcher.BeginInvoke(
                            () => PonyvilleDirectory.GetCloud(chansplit[1]).UpdateCloud(args.Data));
                    } else if (chansplit[0] == "users") {
                        var user = JsonConvert.DeserializeObject<FayeResult<User>>(args.Data);
                        user.data.CopyTo(Connection.CurrentCloudsdaleUser);
                        if ((Connection.CurrentCloudsdaleUser.suspended_until ?? new DateTime(0)) > DateTime.Now) {
                            Deployment.Current.Dispatcher.BeginInvoke(() => {
                                MessageBox.Show("You are banned until" + Connection.CurrentCloudsdaleUser.suspended_until +
                                    "\n" + Connection.CurrentCloudsdaleUser.reason_for_suspension);
                                throw new ApplicationTerminationException();
                            });
                        }
                    }
                } else {
                    if (chansplit[0] != "clouds") return;
                    switch (chansplit[2]) {
                        case "drops":
                            var drop = JsonConvert.DeserializeObject<FayeDropResponse>(args.Data).data;
                            Cache[chansplit[1]].drops.AddDrop(drop);
                            break;
                        case "users":
                            if (chansplit.Length < 4) break;
                            var data = JObject.Parse(args.Data)["data"];

                            if (data["status"] != null) {
                                if (data["id"] == null)
                                    data["id"] = chansplit[3];
                                Cache[chansplit[1]].users.Heartbeat(data);
                            }
                            break;
                        case "chat":
                            var message =
                                JsonConvert.DeserializeObject<FayeMessageResponse>
                                (args.Data, new JsonSerializerSettings {
                                    ObjectCreationHandling = ObjectCreationHandling.Replace
                                }).data;
                            if (message == null || message.user == null || message.content == null) break;

                            ServerDiff = message.timestamp - DateTime.Now;

                            var cache = Cache[chansplit[1]];
                            lock (cache.Lock) {
                                if (message.client_id == Connection.Faye.ClientId) {
                                    var messages = cache.Messages;
                                    for (var i = 0; i < messages.Count; ++i) {
                                        if (messages[i].content == message.content) {
                                            messages[i].drops = message.drops;
                                            cache.messages.cache.Trigger(i);
                                            break;
                                        }
                                        var foundone = false;
                                        foreach (var sub in messages[i].subs.Where(sub => sub.content == message.content)) {
                                            sub.drops = message.drops;
                                            cache.messages.cache.Trigger(i);
                                            foundone = true;
                                            break;
                                        }
                                        if (foundone) break;
                                    }
                                    break;
                                }
                                cache.messages.Add(message);
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

            Connection.Faye.Subscribe("/clouds/" + id + "/users/**");

            WebPriorityManager.BeginHighPriorityRequest(new Uri(Resources.PreviousMessagesEndpoint.Replace("{cloudid}", id)), e => {
                try {
                    var result = JsonConvert.DeserializeObject<WebMessageResponse>(e.Result);
                    if (!Cache.ContainsKey(id)) return;
                    Cache[id].Users.Init();
                    lock (Cache[id].Lock) {
                        var oldmsgs = Cache[id].Messages.Where(msg => msg.timestamp > result.result.Last().timestamp).ToArray();
                        Cache[id].messages.Clear();
                        foreach (var m in result.result.Where(m => m != null && m.user != null && m.id != null && m.content != null)) {
                            Cache[id].messages.Add(m);
                        }
                        foreach (var m in oldmsgs) {
                            Cache[id].messages.Add(m);
                        }
                    }
                    if (onComplete != null) Deployment.Current.Dispatcher.BeginInvoke(onComplete);
                } catch (WebException) {
                }
            });
        }

        public static void Unsubscribe(string cloud) {
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/chat/messages");
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/drops");
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/users/**");
            if (Cache.ContainsKey(cloud)) Cache.Remove(cloud);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string LastMessage {
            get { return ""; }
        }
    }

    public class FayeResult<T> {
        public string channel;
        public T data { get; set; }
    }
}
