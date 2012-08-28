using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Cloudsdale.Models;
using System.Windows;
using System.Linq;

namespace Cloudsdale.Managers {
    public class PonyTracker {
        private readonly Dictionary<string, UserObject> users = new Dictionary<string, UserObject>();
        private readonly ObservableCollection<CensusUser> userlist = new ObservableCollection<CensusUser>();
        private readonly Cloud cloud;

        internal PonyTracker(Cloud cloud) {
            this.cloud = cloud;
        }

        public void Heartbeat(UserReference use) {
            var user = PonyvilleCensus.Heartbeat(use);
            if (users.ContainsKey(user.id)) {
                Reset(user.id);
            } else {
                var comp = new UserListSorter(cloud);
                var i = 0;
                while (i < userlist.Count && comp.Compare(user, userlist[i]) > -1) ++i;

                userlist.Insert(i, (users[user.id] = new UserObject {
                    id = user,
                    update = new Timer(o => {
                        users[user.id].Destroy();
                        users.Remove(user.id);
                        Deployment.Current.Dispatcher.BeginInvoke(() => userlist.Remove(user));
                    }, null, 45000, Timeout.Infinite)
                }).id);
            }
        }

        private void Reset(string name) {
            users[name].update.Change(45000, Timeout.Infinite);
        }

        private struct UserObject {
            internal CensusUser id;
            internal Timer update;
            internal void Destroy() {
                update.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public ObservableCollection<CensusUser> Users {
            get { return userlist; }
        }
    }

    public class UserListSorter : IComparer<ListUser> {
        private readonly Cloud cloud;

        public UserListSorter(Cloud cloud) {
            this.cloud = cloud;
        }

        #region Implementation of IComparer<ListUser>

        public int Compare(ListUser x, ListUser y) {
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
