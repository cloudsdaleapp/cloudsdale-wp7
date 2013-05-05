using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Cloudsdale.Models;
using Cloudsdale.Managers;

namespace Cloudsdale.Managers {
    public static class PonyvilleAccounting {
        private static readonly ObservableCollection<ListUser> _users = new ObservableCollection<ListUser>();
        public static ReadOnlyObservableCollection<ListUser> Users {
            get { return new ReadOnlyObservableCollection<ListUser>(_users); }
        }

        public static void AddUser(ListUser user) {
            if (_users.Any(cachedUser => cachedUser.id == user.id)) {
                _users[_users.IndexOf(cachedUser => cachedUser.id == user.id)] = user.AsListUser;
            } else {
                _users.Add(user.AsListUser);
            }
        }

        public static void Save() {
            var data = 
        }
    }
}
