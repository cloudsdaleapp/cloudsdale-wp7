using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cloudsdale.Models {
    public struct User {
        public string Name;
        public DateTime MemberSince;
        public DateTime SuspendedUntil;
        public string ReasonForSuspension;
        public string Id;
        public UserAvatars Avatars;

        public User(IDictionary<string, object> dictionary) {
            Name = dictionary["name"] as string ?? "";
            MemberSince = ParseTime(dictionary["member_since"] as string);
            SuspendedUntil = ParseTime(dictionary["suspended_until"] as string);
            ReasonForSuspension = dictionary["reason_for_suspension"] as string ?? "";
            Id = dictionary["id"] as string ?? "";
            var avatarkeys = dictionary["avatar"] as Dictionary<string, object> ?? new Dictionary<string, object>();
            Avatars = new UserAvatars(avatarkeys);

            return;
        }

        private static DateTime ParseTime(string input) {
            DateTime time;
            if (!DateTime.TryParse(input, out time)) {
                time = new DateTime(0);
            }
            return time;
        }

    }

    public struct UserAvatars {
        public UserAvatars(IDictionary<string, object> dictionary) {
            if (dictionary.ContainsKey("normal") && dictionary["normal"] is string) {
                Normal = new Uri(dictionary["normal"] as string);
            } else {
                Normal = new Uri(Resources.fallbackBaseUrl + "avatar_user.png");
            }
            if (dictionary.ContainsKey("mini") && dictionary["mini"] is string) {
                Mini = new Uri(dictionary["mini"] as string);
            } else {
                Mini = new Uri(Resources.fallbackBaseUrl + "avatar_mini_user.png");
            }
            if (dictionary.ContainsKey("thumb") && dictionary["thumb"] is string) {
                Thumb = new Uri(dictionary["thumb"] as string);
            } else {
                Thumb = new Uri(Resources.fallbackBaseUrl + "avatar_thumb_user.png");
            }
            if (dictionary.ContainsKey("preview") && dictionary["preview"] is string) {
                Preview = new Uri(dictionary["preview"] as string);
            } else {
                Preview = new Uri(Resources.fallbackBaseUrl + "avatar_preview_user.png");
            }
            if (dictionary.ContainsKey("chat") && dictionary["chat"] is string) {
                Chat = new Uri(dictionary["chat"] as string);
            } else {
                Chat = new Uri(Resources.fallbackBaseUrl + "avatar_chat_user.png");
            }
        }

        public Uri Normal;
        public Uri Mini;
        public Uri Thumb;
        public Uri Preview;
        public Uri Chat;
    }
}
