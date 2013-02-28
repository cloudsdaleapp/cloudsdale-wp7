using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using Cloudsdale.Models;
using System.Windows;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Managers {
    public class PonyTracker : INotifyPropertyChanged {
        private readonly Dictionary<string, Status> userStatus = new Dictionary<string, Status>();
        private readonly Cloud cloud;

        internal PonyTracker(Cloud cloud) {
            this.cloud = cloud;
        }

        internal void Init() {
            WebPriorityManager.BeginHighPriorityRequest(new Uri("http://www.cloudsdale.org/v1/clouds/:id/users.json"
                .Replace(":id", cloud.id)), response => {
                    var result = JObject.Parse(response.Result);
                    foreach (var user in result["result"].Where(token => (string)token["status"] != "offline")) {
                        PonyvilleCensus.Heartbeat(user.ToObject<SimpleUser>());
                        Heartbeat(user, false);
                    }

                    Deployment.Current.Dispatcher.BeginInvoke(() => OnPropertyChanged("Users"));
                });
        }

        public void Heartbeat(JToken user, bool update = true) {
            var uid = (string)user["id"];
            var status = user["status"].ToObject<Status>();

            if (uid == Connection.CurrentCloudsdaleUser.id) {
                Connection.CurrentCloudsdaleUser.status = (string) user["status"];
                Connection.CurrentCloudsdaleUser.OnPropertyChanged("status");
            }

            if (userStatus.ContainsKey(uid) && status == userStatus[uid]) {
                return;
            }

            userStatus[uid] = status;
            Deployment.Current.Dispatcher.BeginInvoke(() => PonyvilleCensus.GetUser(uid).OnPropertyChanged("Status"));
            if (update) {
                Deployment.Current.Dispatcher.BeginInvoke(() => OnPropertyChanged("Users"));
            }
        }

        public IEnumerable<CensusUser> Users {
            get {
                var users =
                    userStatus.Where(kvp => kvp.Value != Status.Offline).Select(kvp => PonyvilleCensus.GetUser(kvp.Key)).ToArray();
                Array.Sort(users, new UserListSorter(cloud));
                return users;
            }
        }

        public Status GetStatus(string id) {
            return userStatus.ContainsKey(id) ? userStatus[id] : Status.Offline;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UserListSorter : IComparer<CensusUser> {
        private readonly Cloud cloud;

        public UserListSorter(Cloud cloud) {
            this.cloud = cloud;
        }

        #region Implementation of IComparer<ListUser>

        public int Compare(CensusUser x, CensusUser y) {
            if (x.id == y.id) return 0;

            var xisowner = x.id == cloud.Owner;
            var yisowner = y.id == cloud.Owner;
            if (xisowner) return -1;
            if (yisowner) return 1;

            var xismod = cloud.Moderators.Contains(x.id);
            var yismod = cloud.Moderators.Contains(y.id);

            return xismod ? (yismod ? 0 : -1) : 1;
        }

        #endregion
    }

    public class UserUpdateEventArgs : EventArgs {
        public string Message { get; internal set; }
        public SimpleUser User { get; internal set; }
    }
}
