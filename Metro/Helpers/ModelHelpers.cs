using System.Collections.Generic;
using System.Linq;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Controllers;

namespace Cloudsdale_Metro.Helpers {
    public static class ModelHelpers {
        public static CloudController GetController(this Cloud cloud) {
            return null;
        }

        public static KeyValuePair<string, T> KeyOf<T>(this string key, T value) {
            return new KeyValuePair<string, T>(key, value);
        }

        public static Session GetSession(this ConnectionController controller) {
            return controller.SessionController.CurrentSession;
        }

        public static bool IsModerator(this User user) {
            return App.Connection.MessageController.CurrentCloud.Cloud.ModeratorIds.Contains(user.Id);
        }

        public static bool IsOwner(this User user) {
            return App.Connection.MessageController.CurrentCloud.Cloud.OwnerId == user.Id;
        }
    }
}
