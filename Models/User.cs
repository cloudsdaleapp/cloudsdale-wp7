using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cloudsdale.Models {

    [DataContract]
    [KnownType(typeof(Avatar))]
    public class ListUser : UserReference {
        [DataMember]
        public virtual string name { get; set; }
        [DataMember]
        public virtual Avatar avatar { get; set; }

        public ListUser AsListUser {
            get { return new ListUser { id = id, name = name, avatar = avatar }; }
        }
    }

    [DataContract]
    public class SimpleUser : ListUser {
        [DataMember]
        public string role;

        public string RoleTag {
            get {
                switch (role) {
                    case "creator":
                        return "founder";
                    case "admin":
                    case "moderator":
                        return role;
                }
                return "";
            }
        }

        public Color RoleColor {
            get {
                switch (role) {
                    case "creator":
                        return Color.FromArgb(0xFF, 0xFF, 0x1F, 0x1F);
                    case "admin":
                        return Color.FromArgb(0xFF, 0x1F, 0x7F, 0x1F);
                    case "moderator":
                        return Color.FromArgb(0xFF, 0xFF, 0xAF, 0x1F);
                }
                return default(Color);
            }
        }

        public Brush RoleBrush {
            get { return new SolidColorBrush(RoleColor); }
        }
    }

    [DataContract]
    [KnownType(typeof(Prosecution))]
    public class User : SimpleUser {
        [DataMember]
        public string time_zone;
        [DataMember]
        public DateTime? member_since;
        [DataMember]
        public DateTime? suspended_until;
        [DataMember]
        public string reason_for_suspension;

        [DataMember]
        public bool? is_registered;
        [DataMember]
        public bool? is_transient;
        [DataMember]
        public bool? is_banned;
        [DataMember]
        public bool? is_member_of_a_cloud;
        [DataMember]
        public bool? has_an_avatar;
        [DataMember]
        public bool? has_read_tnc;
        [DataMember]
        public Prosecution[] prosecutions;

        public void CopyTo(User user) {
            if (id != null)
                user.id = id;
            if (name != null)
                user.name = name;
            if (avatar != null) {
                if (avatar.Chat != null)
                    user.avatar.Chat = avatar.Chat;
                if (avatar.Mini != null)
                    user.avatar.Mini = avatar.Mini;
                if (avatar.Normal != null)
                    user.avatar.Normal = avatar.Normal;
                if (avatar.Preview != null)
                    user.avatar.Preview = avatar.Preview;
                if (avatar.Thumb != null)
                    user.avatar.Thumb = avatar.Thumb;
            }
            if (role != null)
                user.role = role;
            if (time_zone != null)
                user.time_zone = time_zone;
            if (member_since != null)
                user.member_since = member_since;
            if (suspended_until != null)
                user.suspended_until = suspended_until;
            if (reason_for_suspension != null)
                user.reason_for_suspension = reason_for_suspension;
            if (is_registered != null)
                user.is_registered = is_registered;
            if (is_transient != null)
                user.is_transient = is_transient;
            if (is_banned != null)
                user.is_banned = is_banned;
            if (is_member_of_a_cloud != null)
                user.is_member_of_a_cloud = is_member_of_a_cloud;
            if (has_an_avatar != null)
                user.has_an_avatar = has_an_avatar;
            if (has_read_tnc != null)
                user.has_read_tnc = has_read_tnc;
            if (prosecutions != null)
                user.prosecutions = prosecutions;
        }
    }

    [DataContract]
    [KnownType(typeof(Cloud))]
    public class LoggedInUser : User {
        [DataMember]
        public string auth_token;
        [DataMember]
        public string email;
        [DataMember]
        public bool? needs_to_confirm_registration;
        [DataMember]
        public bool? needs_name_change;
        [DataMember]
        public Cloud[] clouds;
    }
}
