using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Models;
using Newtonsoft.Json;
using System.Threading;

namespace Cloudsdale.Managers {
    public class MessageCacheController {
        internal static Timer PresenceAnnouncer = new Timer(o => {
            Thread.CurrentThread.Name = "PresenceAnnouncement";
            foreach (var cloud in Cache.Keys) {
                var user = Connection.CurrentCloudsdaleUser.AsListUser;
                var request = JsonConvert.SerializeObject(new PresenceObject { channel = "/clouds/" + cloud + "/users", data = user });
                Connection.Faye.SendRaw(request);
            }
        }, null, 5000, 30000);

        public class PresenceObject {
            public string channel;
            public PresenceUser data;
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
            public string Chat;
            public string Mini;
            public string Normal;
            public string Preview;
            public string Thumb;

            public static implicit operator PresenceAvatar(Avatar a) {
                return new PresenceAvatar {
                    Chat = a.Chat.ToString(),
                    Mini = a.Mini.ToString(),
                    Normal = a.Normal.ToString(),
                    Preview = a.Preview.ToString(),
                    Thumb = a.Thumb.ToString()
                };
            }
        }

        private MessageCacheController() {
        }
        private static readonly Dictionary<string, MessageCacheController> Cache =
            new Dictionary<string, MessageCacheController>();
        public static void Init() {
            Connection.Faye.ChannelMessageRecieved += FayeMessageRecieved;
            if (PresenceAnnouncer == null) PresenceAnnouncer = new Timer(o => {
                Thread.CurrentThread.Name = "PresenceAnnouncement";
                foreach (var cloud in Cache.Keys) {
                    var user = Connection.CurrentCloudsdaleUser.AsListUser;
                    var request = JsonConvert.SerializeObject(new PresenceObject { channel = "/clouds/" + cloud + "/users", data = user });
                    Connection.Faye.SendRaw(request);
                }
            }, null, 5000, 30000);
        }

        static void FayeMessageRecieved(object sender, FayeConnector.FayeConnector.DataReceivedEventArgs args) {
            try {
                var chansplit = args.Channel.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (chansplit.Length < 3) return;
                if (chansplit[0] != "clouds") return;
                if (!Cache.ContainsKey(chansplit[1])) return;
                switch (chansplit[2]) {
                    case "drops":
                        var drop = JsonConvert.DeserializeObject<FayeDropResponse>(args.Data).data;
                        Cache[chansplit[1]].drops.Add(drop);
                        break;
                    case "users":
                        var user = JsonConvert.DeserializeObject<FayeResult<ListUser>>(args.Data);
                        if (user.data == null) break;
                        Deployment.Current.Dispatcher.BeginInvoke(() => Cache[chansplit[1]].users.Heartbeat(user.data));
                        break;
                    case "chat":
                        var message = JsonConvert.DeserializeObject<FayeMessageResponse>(args.Data).data;
                        if (message == null || message.user == null || message.content == null) break;
#if DEBUG
                        Debug.WriteLine("[{2}] {0}: {1}", message.user.name, message.content, chansplit[1]);
#endif
                        var cache = Cache[chansplit[1]];
                        if (cache.messages.Count > 0 && cache.messages.LastItem.user.id == message.user.id) {
                            cache.messages.LastItem.content += "\n" + message.content;
                            cache.messages.ForceUpdate();
                        } else {
                            cache.messages.Add(message);
                        }
                        cache.unread++;
                        cache.DoUpdates();
                        break;

                }
            } catch (Exception e) {
#if DEBUG
                Debugger.Break();
#else
                BugSense.BugSenseHandler.Instance.LogError(e);
#endif
            }
        }

        public static MessageCacheController GetCloud(string name) {
            return Cache[name];
        }

        private readonly LinearCache<Message> messages = new LinearCache<Message>(75);
        private readonly LinearCache<Drop> drops = new LinearCache<Drop>(25, true);
        private readonly CloudUserListManager users = new CloudUserListManager();
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
        public ObservableCollection<ListUser> Users {
            get { return users.Users; }
        }
        public void BindMsgCount(Controls.CountDisplay display) {
            display.Binding = textblockbinding;
        }

        public static MessageCacheController Subscribe(string cloud) {
            if (!Cache.ContainsKey(cloud)) {
                Cache[cloud] = new MessageCacheController();
            }
            Connection.Faye.Subscribe("/clouds/" + cloud + "/users");
            Connection.Faye.Subscribe("/clouds/" + cloud + "/chat/messages");
            Connection.Faye.Subscribe("/clouds/" + cloud + "/drops");
            var wc = new WebClient();
            DownloadStringCompletedEventHandler dlm = (sender, args) => { };
            dlm = (sender, args) => {
                Cache[cloud].messages.Clear();
                var ms = JsonConvert.DeserializeObject<WebMessageResponse>(args.Result).result;
                var last = new Message { user = new SimpleUser { id = "" } };
                var cache = Cache[cloud];
                foreach (var m in ms) {
                    if (m == null || m.user == null || m.content == null)
                        continue;
                    if (cache.messages.Count > 0 && last.user.id == m.user.id) {
                        last.content += ("\n" + m.content);
                    } else {
                        cache.messages.Add(m);
                        last = m;
                    }
                }
                wc.DownloadStringCompleted -= dlm;
                wc.DownloadStringCompleted += (o, eventArgs) => {
                    var result = JsonConvert.DeserializeObject<WebDropResponse>(eventArgs.Result);
                    var drops = result.result;
                    Array.Reverse(drops);
                    Cache[cloud].drops.Clear();
                    foreach (var d in drops) {
                        Cache[cloud].drops.Add(d);
                    }
                };
                wc.DownloadStringAsync(new Uri(Resources.PreviousDropsEndpoint.Replace("{cloudid}", cloud)));
            };
            wc.DownloadStringCompleted += dlm;
            wc.DownloadStringAsync(new Uri(Resources.PreviousMessagesEndpoint.Replace("{cloudid}", cloud)));

            return Cache[cloud];
        }

        public static void Unsubscribe(string cloud) {
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/chat/messages");
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/drops");
            Connection.Faye.Unsubscribe("/clouds/" + cloud + "/users");
            if (Cache.ContainsKey(cloud)) Cache.Remove(cloud);
        }
    }

    public class LinearCache<T> {
        private int capacity;
        private readonly ObColWmt cache;
        private readonly bool reverse;

        public LinearCache(int capacity, bool reverse = false) {
            this.capacity = capacity;
            this.reverse = reverse;
            cache = new ObColWmt();
        }

        public int Capacity {
            get { return capacity; }
            set {
                while (cache.Count > value) {
                    cache.RemoveAt(0);
                }
                capacity = value;
            }
        }

        public int Count {
            get { return cache.Count; }
        }

        public T LastItem {
            get { return cache[cache.Count - 1]; }
            set { cache[cache.Count - 1] = value; }
        }

        public void Clear() {
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                cache.Clear();
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(() => cache.Clear());
            }
        }

        public void RemoveFirst() {
            cache.RemoveAt(0);
        }

        public void Add(T item) {
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                if (reverse) {
                    if (cache.Count >= capacity) {
                        cache.RemoveAt(cache.Count - 1);
                    }
                    cache.Insert(0, item);
                } else {
                    if (cache.Count >= capacity) {
                        cache.RemoveAt(0);
                    }
                    cache.Add(item);
                }
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    if (reverse) {
                        if (cache.Count >= capacity) {
                            cache.RemoveAt(cache.Count - 1);
                        }
                        cache.Insert(0, item);
                    } else {
                        if (cache.Count >= capacity) {
                            cache.RemoveAt(0);
                        }
                        cache.Add(item);
                    }
                });
            }
        }

        public void ForceUpdate() {
            cache.Trigger();
        }

        private class ObColWmt : ObservableCollection<T> {
            internal void Trigger() {
                if (Deployment.Current.Dispatcher.CheckAccess()) {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, this[Count - 1], this[Count - 1], Count - 1));
                } else {
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () => OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace, this[Count - 1], this[Count - 1], Count - 1)));
                }
            }
        }

        public ObservableCollection<T> Cache {
            get { return cache; }
        }
    }

    public class FayeResult<T> {
        public string channel;
        public T data { get; set; }
    }
}
