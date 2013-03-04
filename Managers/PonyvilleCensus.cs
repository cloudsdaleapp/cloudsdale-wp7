using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using Cloudsdale.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Managers {
    public class PonyvilleCensus {
        private static readonly UserStore Cache;

        public List<CensusUser> Users {
            get {
                return (from reference in Cache.Values select reference.Target as CensusUser).ToList();
            }
        }

        static PonyvilleCensus() {
            Cache = new UserStore();
        }

        public static CensusUser GetUser(string id) {
            lock (Cache) {
                if (Cache.ContainsKey(id)) {
                    var user = Cache[id].Target as CensusUser;
                    if (user != null) {
                        return user;
                    }
                }
                var newuser = new CensusUser(id);
                Cache[id] = new WeakReference(newuser);
                return newuser;
            }
        }

        public static void Save() {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!storage.DirectoryExists("users")) {
                storage.CreateDirectory("users");
            }
            foreach (var user in Cache.Select(userRef => userRef.Value.Target).OfType<CensusUser>()) {
                SaveUser(user, storage);
            }
        }

        internal static void SaveUser(CensusUser user, IsolatedStorageFile storage) {
            var obj = new JObject();
            obj["user"] = JObject.FromObject(user);
            obj["clouds"] = new JArray(from cloud in user.ExtClouds
                                       select JObject.FromObject(cloud));

            if (storage.FileExists("users\\" + user.id)) {
                storage.DeleteFile("users\\" + user.id);
            }

            var data = obj.ToString();
            using (var file = storage.OpenFile("users\\" + user.id, FileMode.OpenOrCreate, FileAccess.Write))
            using (var writer = new StreamWriter(file, Encoding.UTF8)) {
                writer.WriteLine(data);
            }
        }

        // ReSharper disable RedundantCheckBeforeAssignment
        // The check is hardly redundant. I don't want to go calling property updates if nothing is even changing lol.
        public static CensusUser Heartbeat(UserReference user) {
            CensusUser cacheUser;
            lock (Cache) {
                cacheUser = GetUser(user.id);
            }
            if (cacheUser.id == null) cacheUser.id = user.id;
            if (user is ListUser) {
                var luser = user as ListUser;
                if (luser.name != null && cacheUser.name != luser.name)
                    cacheUser.name = luser.name;

                if (luser.avatar != null) {
                    if (luser.avatar.Chat != null && cacheUser.avatar.Chat != luser.avatar.Chat)
                        cacheUser.avatar.Chat = luser.avatar.Chat;

                    if (luser.avatar.Mini != null && cacheUser.avatar.Mini != luser.avatar.Mini)
                        cacheUser.avatar.Mini = luser.avatar.Mini;

                    if (luser.avatar.Normal != null && cacheUser.avatar.Normal != luser.avatar.Normal)
                        cacheUser.avatar.Normal = luser.avatar.Normal;

                    if (luser.avatar.Preview != null && cacheUser.avatar.Preview != luser.avatar.Preview)
                        cacheUser.avatar.Preview = luser.avatar.Preview;

                    if (luser.avatar.Thumb != null && cacheUser.avatar.Thumb != luser.avatar.Thumb)
                        cacheUser.avatar.Thumb = luser.avatar.Thumb;
                }
            }
            if (user is SimpleUser) {
                var suser = user as SimpleUser;
                if (suser.role != null && cacheUser.role != suser.role) {
                    cacheUser.Role = suser.role;
                }
            }
            return cacheUser;
        }
        // ReSharper restore RedundantCheckBeforeAssignment
    }

    public sealed class CensusUser : User {

        [JsonConstructor]
        public CensusUser() {
        }

        public CensusUser(string id) {
            this.id = id;
            avatar = new CensusAvatar();
            name = "(Identifying)";

            new Thread(() => {
                var storage = IsolatedStorageFile.GetUserStoreForApplication();
                if (!storage.FileExists("users\\" + id)) return;
                string data;
                using (var file = storage.OpenFile("users\\" + id, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(file, Encoding.UTF8)) {
                    data = reader.ReadToEnd();
                }
                try {
                    var jObj = JObject.Parse(data);
                    var user = jObj["user"].ToObject<CensusUser>();
                    var clouds = (JArray)jObj["clouds"];

                    if (!_extClouds.Any()) {
                        _extClouds = from jcloud in clouds select PonyvilleDirectory.RegisterCloud(jcloud.ToObject<Cloud>());
                    }
                    if (name == "(Identifying)") {
                        name = user.name;
                    }
                    if (string.IsNullOrWhiteSpace(role)) {
                        Role = user.role;
                    }
                    if (avatar == null) {
                        avatar = user.avatar;
                    } else {
                        avatar.Normal = user.avatar.Normal;
                        avatar.Mini = user.avatar.Mini;
                        avatar.Chat = user.avatar.Chat;
                        avatar.Preview = user.avatar.Preview;
                        avatar.Thumb = user.avatar.Thumb;
                    }


                    if (member_since == null) {
                        member_since = user.member_since;
                    }
                    if (prosecutions == null) {
                        prosecutions = user.prosecutions;
                    }

                } catch (JsonException e) {
                }
            }).Start();

            if (Connection.CurrentCloudsdaleUser != null)
                if (id == Connection.CurrentCloudsdaleUser.id) {
                    Connection.CurrentCloudsdaleUser.CopyTo(this);
                }

            WebPriorityManager.BeginLowPriorityRequest(
                new Uri(Resources.getUserEndpoint.Replace("{0}", id)),
                args => {
                    var data = JsonConvert.DeserializeObject<GetUserResult>(args.Result);
                    data.result.CopyTo(this);
                });
        }

        ~CensusUser() {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!storage.DirectoryExists("users")) {
                storage.CreateDirectory("users");
            }
            PonyvilleCensus.SaveUser(this, storage);
        }

        [JsonIgnore]
        public Status Status {
            get { return Connection.CurrentCloud.Controller.Users.GetStatus(id); }
        }

        public override string name {
            get {
                return base.name;
            }
            set {
                base.name = value;
                OnPropertyChanged("name");
            }
        }

        public string Role {
            set {
                role = value;
                OnPropertyChanged("RoleTag");
                OnPropertyChanged("RoleBrush");
            }
        }

        public override string skype_name {
            get {
                return base.skype_name;
            }
            set {
                base.skype_name = value;
                OnPropertyChanged("skype_name");
            }
        }

        private IEnumerable<Cloud> _extClouds = new Cloud[0];
        [JsonIgnore]
        public IEnumerable<Cloud> ExtClouds {
            get { return from cloud in _extClouds where !(cloud.hidden ?? false) select cloud; }
        }

        public void Ban(string reason, DateTime due, string cloudid) {
            var data = new JObject();
            data["offender_id"] = id;
            data["ban"] = new JObject();
            data["ban"]["reason"] = reason;
            data["ban"]["due"] = due;

            var bytes = Encoding.UTF8.GetBytes(data.ToString());

            var request = WebRequest.CreateHttp("http://www.cloudsdale.org/v1/clouds/" + cloudid + "/bans");
            request.Accept = "application/json";
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers["X-Auth-Token"] = Connection.CurrentCloudsdaleUser.auth_token;

            request.BeginGetRequestStream(requestCallback => {
                using (var requestStream = request.EndGetRequestStream(requestCallback)) {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }

                request.BeginGetResponse(responseCallback => {
                    using (var response = request.EndGetResponse(responseCallback))
                    using (var responseStream = response.GetResponseStream()) {
                        responseStream.Close();
                    }
                }, null);
            }, null);
        }
    }

    public sealed class CensusAvatar : Avatar, INotifyPropertyChanged {
        private const string DefaultAvatarUrlChat = "http://assets.cloudsdale.org/assets/fallback/avatar_chat_user.png";
        private const string DefaultAvatarUrlMini = "http://assets.cloudsdale.org/assets/fallback/avatar_mini_user.png";
        private const string DefaultAvatarUrlNormal = "http://assets.cloudsdale.org/assets/fallback/avatar_user.png";
        private const string DefaultAvatarUrlPreview = "http://assets.cloudsdale.org/assets/fallback/avatar_preview_user.png";
        private const string DefaultAvatarUrlThumb = "http://assets.cloudsdale.org/assets/fallback/avatar_thumb_user.png";

        public CensusAvatar() {
            Chat = new Uri(DefaultAvatarUrlChat);
            Mini = new Uri(DefaultAvatarUrlMini);
            Normal = new Uri(DefaultAvatarUrlNormal);
            Preview = new Uri(DefaultAvatarUrlPreview);
            Thumb = new Uri(DefaultAvatarUrlThumb);
        }

        public override Uri Chat {
            get {
                return base.Chat;
            }
            set {
                base.Chat = value;
                OnPropertyChanged("Chat");
            }
        }

        public override Uri Mini {
            get {
                return base.Mini;
            }
            set {
                base.Mini = value;
                OnPropertyChanged("Mini");
            }
        }

        public override Uri Normal {
            get {
                return base.Normal;
            }
            set {
                base.Normal = value;
                OnPropertyChanged("Normal");
            }
        }

        public override Uri Preview {
            get {
                return base.Preview;
            }
            set {
                base.Preview = value;
                OnPropertyChanged("Preview");
            }
        }

        public override Uri Thumb {
            get {
                return base.Thumb;
            }
            set {
                base.Thumb = value;
                OnPropertyChanged("Thumb");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) {
            if (Deployment.Current.CheckAccess()) {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    var handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
    }

    public class UserStore : Dictionary<string, WeakReference> {

    }
}
