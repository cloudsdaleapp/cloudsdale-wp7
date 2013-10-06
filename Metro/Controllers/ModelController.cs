using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudsdaleLib.Models;
using CloudsdaleLib.Providers;

namespace Cloudsdale_Metro.Controllers {
    public class ModelController : IUserProvider, ICloudProvider {
        private readonly Dictionary<string, WeakReference<User>> users = new Dictionary<string, WeakReference<User>>();
        private readonly Dictionary<string, WeakReference<Cloud>> clouds = new Dictionary<string, WeakReference<Cloud>>();
        private SessionController sessionController { get { return App.Connection.SessionController; } }

        public async Task<User> GetUserAsync(string id) {
            if (id == sessionController.CurrentSession.Id) {
                return sessionController.CurrentSession;
            }

            var user = GetUserRef(id);
            if (user == null) {
                user = new User(id);
                await user.ForceValidate();
                users[id] = MakeUserRef(user);
            } else {
                await user.Validate();
            }
            return user;
        }

        public User GetUser(string id) {
            if (id == sessionController.CurrentSession.Id) {
                return sessionController.CurrentSession;
            }

            var user = GetUserRef(id);
            if (user == null) {
                user = new User(id);
                user.ForceValidate();
                users[id] = MakeUserRef(user);
            } else {
                user.Validate();
            }
            return user;
        }

        public async Task<User> UpdateUserAsync(User data) {
            if (data.Id == sessionController.CurrentSession.Id) {
                var session = sessionController.CurrentSession;
                data.CopyTo(session);
                return session;
            }

            var user = GetUserRef(data.Id);
            if (user == null) {
                user = data;
                await user.ForceValidate();
                users[user.Id] = MakeUserRef(user);
            } else {
                data.CopyTo(user);
            }
            return user;
        }

        public User UpdateUser(User data) {
            if (data.Id == sessionController.CurrentSession.Id) {
                var session = sessionController.CurrentSession;
                data.CopyTo(session);
                return session;
            }

            var user = GetUserRef(data.Id);
            if (user == null) {
                user = data;
                user.ForceValidate();
                users[user.Id] = MakeUserRef(user);
            } else {
                data.CopyTo(user);
            }
            return user;
        }

        public async Task<Cloud> UpdateCloudAsync(Cloud data) {
            var cloud = GetCloudRef(data.Id);
            if (cloud == null) {
                cloud = data;
                await cloud.ForceValidate();
                clouds[data.Id] = MakeCloudRef(cloud);
            } else {
                data.CopyTo(cloud);
            }

            return cloud;
        }

        public Cloud UpdateCloud(Cloud data) {
            var cloud = GetCloudRef(data.Id);
            if (cloud == null) {
                cloud = data;
                cloud.ForceValidate();
                clouds[data.Id] = MakeCloudRef(cloud);
            } else {
                data.CopyTo(cloud);
            }

            return cloud;
        }

        public Cloud GetCloud(string cloudId) {
            var cloud = GetCloudRef(cloudId);
            if (cloud == null) {
                cloud = new Cloud(cloudId);
                cloud.ForceValidate();
                clouds[cloud.Id] = MakeCloudRef(cloud);
            } else {
                cloud.Validate();
            }

            return cloud;
        }

        private User GetUserRef(string id) {
            if (!users.ContainsKey(id)) return null;
            var weakRef = users[id];
            User user;
            if (!weakRef.TryGetTarget(out user)) {
                return null;
            }
            return user;
        }

        private WeakReference<User> MakeUserRef(User user) {
            return new WeakReference<User>(user);
        }

        private Cloud GetCloudRef(string id) {
            if (!clouds.ContainsKey(id)) return null;
            var weakRef = clouds[id];
            Cloud cloud;
            if (!weakRef.TryGetTarget(out cloud)) {
                return null;
            }
            return cloud;
        }

        private WeakReference<Cloud> MakeCloudRef(Cloud cloud) {
            return new WeakReference<Cloud>(cloud);
        } 
    }
}
