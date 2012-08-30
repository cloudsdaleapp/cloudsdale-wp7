using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if DEBUG
using System.Diagnostics;
#endif
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
    public class DerpyHoovesMailCenter {
        internal static Timer PresenceAnnouncer = null;

        public static TimeSpan ServerDiff = new TimeSpan();

        private DerpyHoovesMailCenter(Cloud cloud) {
            users = new PonyTracker(cloud);
            drops = PinkiePieEntertainmentDojo.GetForCloud(cloud.id);
        }

        public static readonly Dictionary<string, bool> ValidPreloadedData = new Dictionary<string, bool>();

        public readonly object Lock = new object();

        private static readonly Dictionary<string, DerpyHoovesMailCenter> Cache =
            new Dictionary<string, DerpyHoovesMailCenter>();
        public static void Init() {

            ValidPreloadedData.Clear();

            Connection.Faye.ChannelMessageRecieved += FayeMessageRecieved;
            if (PresenceAnnouncer == null) PresenceAnnouncer = new Timer(o => {
                Thread.CurrentThread.Name = "PresenceAnnouncement";
                foreach (var cloud in Cache.Keys) {
                    Connection.Faye.Publish("/clouds/" + cloud + "/users/" +
                        Connection.CurrentCloudsdaleUser.id, new object());
                }
            }, null, 5000, 30000);

            Connection.Faye.Subscribe("/users/" + Connection.CurrentCloudsdaleUser.id + "/private");
        }

        static void FayeMessageRecieved(object sender, FayeConnector.FayeConnector.DataReceivedEventArgs args) {
            try {
                var jobj = JObject.Parse(args.Data);
                if (jobj.Root["successful"] != null) return;

                var chansplit = args.Channel.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (chansplit.Length < 2) return;
                if (!Cache.ContainsKey(chansplit[1])) return;
                if (chansplit.Length == 2) {
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
                            var user = JsonConvert.DeserializeObject<FayeResult<ListUser>>(args.Data);
                            UserReference uref;
                            if (user.data.id == null) {
                                uref = new UserReference {
                                    id = chansplit[3]
                                };
                            } else {
                                uref = user.data;
                            }
                            Deployment.Current.Dispatcher.BeginInvoke(
                                () => Cache[chansplit[1]].users.Heartbeat(uref));
                            break;
                        case "chat":
                            var message =
                                JsonConvert.DeserializeObject<FayeMessageResponse>
                                (args.Data, new JsonSerializerSettings {
                                    ObjectCreationHandling = ObjectCreationHandling.Replace
                                }).data;
                            if (message == null || message.user == null || message.content == null) break;

                            ServerDiff = message.timestamp - DateTime.Now;

                            if (message.client_id == Connection.Faye.ClientId) {
#if DEBUG
                                Debug.WriteLine("Message comes from this client. Skipping...");
#endif
                                break;
                            }
                            var cache = Cache[chansplit[1]];
                            lock (cache.Lock)
                                cache.messages.Add(message);
                            cache.unread++;
                            cache.DoUpdates();
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
            unread = 0;
            DoUpdates();
        }
        private void DoUpdates() {
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                textblockbinding.Value = (unread > 0) ? unread.ToString() : "";
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(
                    () => textblockbinding.Value = (unread > 0) ? unread.ToString() : "");
            }
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
        public ObservableCollection<CensusUser> Users {
            get { return users.Users; }
        }
        public void BindMsgCount(Controls.CountDisplay display) {
            display.Binding = textblockbinding;
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
                        Cache[cloud.id].drops.PreLoad(drops);
                    } catch (WebException) {
                    }
                });

            return Cache[cloud.id];
        }

        public static void VerifyCloud(string id) {
            if (ValidPreloadedData.ContainsKey(id) && ValidPreloadedData[id]) {
                return;
            }
            ValidPreloadedData[id] = true;

            Connection.Faye.Subscribe("/clouds/" + id + "/users/**");

            WebPriorityManager.BeginHighPriorityRequest(new Uri(Resources.PreviousMessagesEndpoint.Replace("{cloudid}", id)), e => {
                try {
                    var result = JsonConvert.DeserializeObject<WebMessageResponse>(e.Result);
                    lock (Cache[id].Lock) {
                        foreach (var m in result.result) {
                            if (m == null || m.user == null || m.id == null || m.content == null) {
                                continue;
                            }
                            Cache[id].messages.Add(m);
                        }
                    }
                } catch (WebException) {
                }
            });
        }

        public static void Unsubscribe(string cloud) {
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/chat/messages");
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/drops");
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/users");
            if (Cache.ContainsKey(cloud)) Cache.Remove(cloud);
        }

        public class PresenceObject {
            public string channel;
            public PresenceUser data;
            public string clientId = Connection.Faye.ClientId;
        }

        public class PresenceUser {
            public string name;
            public string id;
            public PresenceAvatar avatar;

            public static implicit operator PresenceUser(ListUser u) {
                return new PresenceUser {
                    name = u.name,
                    id = u.id,
                    avatar = u.avatar
                };
            }
        }

        public class PresenceAvatar {
            public string chat;
            public string mini;
            public string normal;
            public string preview;
            public string thumb;

            public static implicit operator PresenceAvatar(Avatar a) {
                return new PresenceAvatar {
                    chat = a.Chat.ToString(),
                    mini = a.Mini.ToString(),
                    normal = a.Normal.ToString(),
                    preview = a.Preview.ToString(),
                    thumb = a.Thumb.ToString(),
                };
            }
        }
    }

    public class FayeResult<T> {
        public string channel;
        public T data { get; set; }
    }
}
