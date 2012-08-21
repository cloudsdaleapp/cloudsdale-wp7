using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Models;
using Newtonsoft.Json;
using System.Threading;

namespace Cloudsdale.Managers {
    /// <summary>
    /// Used to control all incoming data for a cloud
    /// </summary>
    public class DerpyHoovesMailCenter {
        internal static Timer PresenceAnnouncer = null;

        private DerpyHoovesMailCenter(string cid) {
            drops = PinkiePieEntertainmentDojo.GetForCloud(cid);
        }

        public readonly object _lock = new object();

        private static readonly Dictionary<string, DerpyHoovesMailCenter> Cache =
            new Dictionary<string, DerpyHoovesMailCenter>();
        public static void Init() {
            Connection.Faye.ChannelMessageRecieved += FayeMessageRecieved;
            if (PresenceAnnouncer == null) PresenceAnnouncer = new Timer(o => {
                Thread.CurrentThread.Name = "PresenceAnnouncement";
                foreach (var cloud in Cache.Keys) {
                    var user = Connection.CurrentCloudsdaleUser.AsListUser;
                    var request =
                        JsonConvert.SerializeObject(new PresenceObject {
                            channel = "/clouds/" + cloud + "/users",
                            data = user,
                        }).Replace(";", "");
                    Connection.Faye.SendRaw(request);
                }
            }, null, 5000, 30000);
        }

        static void FayeMessageRecieved(object sender, FayeConnector.FayeConnector.DataReceivedEventArgs args) {
            try {
                var chansplit = args.Channel.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (chansplit.Length < 2) return;
                if (chansplit[0] != "clouds") return;
                if (!Cache.ContainsKey(chansplit[1])) return;
                if (chansplit.Length == 2) {
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () => PonyvilleDirectory.GetCloud(chansplit[1]).UpdateCloud(args.Data));
                } else {
                    switch (chansplit[2]) {
                        case "drops":
                            var drop = JsonConvert.DeserializeObject<FayeDropResponse>(args.Data).data;
                            Cache[chansplit[1]].drops.AddDrop(drop);
                            break;
                        case "users":
                            var user = JsonConvert.DeserializeObject<FayeResult<ListUser>>(args.Data);
                            if (user.data == null) break;
                            Deployment.Current.Dispatcher.BeginInvoke(
                                () => Cache[chansplit[1]].users.Heartbeat(user.data));
                            break;
                        case "chat":
                            var message =
                                JsonConvert.DeserializeObject<FayeMessageResponse>
                                (args.Data, new JsonSerializerSettings {
                                    ObjectCreationHandling = ObjectCreationHandling.Replace
                                }).data;
                            if (message == null || message.user == null || message.content == null) break;
                            var cache = Cache[chansplit[1]];
                            lock (cache._lock)
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

        public static DerpyHoovesMailCenter GetCloud(string name) {
            return Cache.ContainsKey(name) ? Cache[name] : Subscribe(name);
        }

        private readonly SweetAppleAcres messages = new SweetAppleAcres(50);
        private readonly PinkiePieEntertainmentDojo drops;
        private readonly PonyTracker users = new PonyTracker();
        private readonly GenericBinding<String> textblockbinding = new GenericBinding<string>(TextBlock.TextProperty);
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

        public static DerpyHoovesMailCenter Subscribe(string cloud) {
            lock (Cache) {
                if (!Cache.ContainsKey(cloud)) {
                    Cache[cloud] = new DerpyHoovesMailCenter(cloud);
                }
            }
            if (Connection.Faye.IsSubscribed("/clouds/" + cloud)) {
                return Cache[cloud];
            }
            Connection.Faye.Subscribe("/clouds/" + cloud);
            Connection.Faye.Subscribe("/clouds/" + cloud + "/users");
            Connection.Faye.Subscribe("/clouds/" + cloud + "/chat/messages");
            Connection.Faye.Subscribe("/clouds/" + cloud + "/drops");
            var wc = new WebClient();
            DownloadStringCompletedEventHandler[] dlm = {(sender, args) => { }};
            dlm[0] = (sender, args) => {
                var ms = JsonConvert.DeserializeObject<WebMessageResponse>(args.Result, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace }).result;
                lock (Cache[cloud]._lock) {
                    foreach (var m in ms) {
                        if (m == null || m.user == null || m.id == null || m.content == null) {
                            Debugger.Break();
                            continue;
                        }
                        Cache[cloud].messages.Add(m);
                    }
                }
                wc.DownloadStringCompleted -= dlm[0];
                wc.DownloadStringCompleted += (o, eventArgs) => {
                    var result = JsonConvert.DeserializeObject<WebDropResponse>(eventArgs.Result);
                    var drops = result.result;
                    Cache[cloud].drops.PreLoad(drops);
                };
                wc.DownloadStringAsync(new Uri(Resources.PreviousDropsEndpoint.Replace("{cloudid}", cloud)));
            };
            wc.DownloadStringCompleted += dlm[0];
            wc.DownloadStringAsync(new Uri(Resources.PreviousMessagesEndpoint.Replace("{cloudid}", cloud)));

            return Cache[cloud];
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
