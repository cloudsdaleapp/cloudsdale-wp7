using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Cloudsdale.Models;

namespace Cloudsdale.Managers {
    public class CloudUserListManager {
        private readonly Dictionary<string, UserObject> users = new Dictionary<string, UserObject>(); 

        public event EventHandler<UserUpdateEventArgs> UserExpired;

        public void Heartbeat(SimpleUser user) {
            if (users.ContainsKey(user.id)) {
                Reset(user.id);
            } else {
                users[user.id] = new UserObject {
                    id = user,
                    update = new Timer(o => {
                        users[user.id].Destroy();
                        users.Remove(user.id);
                        if (UserExpired != null) {
                            UserExpired(this, new UserUpdateEventArgs {
                                Message = "HEARTBEAT_TIMEOUT", User = user
                            });
                        }
                    }, null, 45000, Timeout.Infinite)
                };
            }
        }

        private void Reset(string name) {
            users[name].update.Change(45000, Timeout.Infinite);
        }

        private struct UserObject {
            internal SimpleUser id;
            internal Timer update;
            internal void Destroy() {
                update.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public SimpleUser[] Users {
            get { return (from user in users.Values select user.id).ToArray(); }
        }
    }

    public class UserUpdateEventArgs : EventArgs {
        public string Message { get; internal set; }
        public SimpleUser User { get; internal set; }
    }
}
