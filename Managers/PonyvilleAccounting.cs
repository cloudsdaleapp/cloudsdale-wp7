using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Cloudsdale.Models;
using System.IO.IsolatedStorage;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Managers {
    public static class PonyvilleAccounting {
        static PonyvilleAccounting() {
            Load();
        }

        private static readonly ObservableCollection<LoggedInUser> _users = new ObservableCollection<LoggedInUser>();
        public static ReadOnlyObservableCollection<LoggedInUser> Users {
            get { return new ReadOnlyObservableCollection<LoggedInUser>(_users); }
        }

        public static void AddUser(LoggedInUser user) {
            if (!Deployment.Current.Dispatcher.CheckAccess()) {
                Deployment.Current.Dispatcher.BeginInvoke(() => AddUser(user));
                return;
            }
            if (_users.Any(cachedUser => cachedUser.id == user.id)) {
                _users.Remove(_users.First(cachedUser => cachedUser.id == user.id));
            }
            _users.Insert(0, user);

            Save();
        }

        public static void ForgetUser(UserReference user) {
            if (!Deployment.Current.Dispatcher.CheckAccess()) {
                Deployment.Current.Dispatcher.BeginInvoke(() => ForgetUser(user));
                return;
            }

            if (_users.Any(cachedUser => cachedUser.id == user.id)) {
                _users.Remove(_users.First(cachedUser => cachedUser.id == user.id));
            }

            Save();
        }

        public static void Save() {
            var data = _users.Serialize(array: true);
            var storage = IsolatedStorageFile.GetUserStoreForApplication();

            using (var file = storage.OpenFile("accounts.json", FileMode.Create)) {
                file.Write(data, 0, data.Length);
            }
        }

        public static void Load() {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();

            JArray items;
            if (storage.FileExists("accounts.json")) {
                using (var file = storage.OpenFile("accounts.json", FileMode.Open))
                using (var reader = new StreamReader(file, Encoding.UTF8)) {
                    items = JArray.Parse(reader.ReadToEnd());
                }
            } else {
                items = new JArray();
            }

            _users.Clear();
            items.Select(token => token.ToObject<LoggedInUser>()).CopyTo(_users);
        }
    }
}
