using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Cloudsdale.Models;
using System.Windows;

namespace Cloudsdale.Managers {
    public class PonyTracker {
        private readonly Dictionary<string, UserObject> users = new Dictionary<string, UserObject>();
        private readonly ObservableCollection<CensusUser> userlist = new ObservableCollection<CensusUser>();

        internal PonyTracker() {
        }

        public void Heartbeat(UserReference use) {
            var user = PonyvilleCensus.Heartbeat(use);
            if (users.ContainsKey(user.id)) {
                Reset(user.id);
            } else {
                userlist.Add((users[user.id] = new UserObject {
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

    public class UserUpdateEventArgs : EventArgs {
        public string Message { get; internal set; }
        public SimpleUser User { get; internal set; }
    }
}
