﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cloudsdale.Models {

    public class ListUser : UserReference {
        public string name { get; set; }
        public Avatar avatar { get; set; }
        public ListUser AsListUser {
            get { return new ListUser {id = id, name = name, avatar = avatar}; }
        }
    }

    public class SimpleUser : ListUser {
        public string role;
        
        public string RoleTag {
            get {
                switch (role) {
                    case "founder":
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
                    case "founder":
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

    public class User : SimpleUser {
        public string time_zone;
        public DateTime member_since;
        public DateTime suspended_until;
        public string reason_for_suspension;

        public bool is_registered;
        public bool is_transient;
        public bool is_banned;
        public bool is_member_of_a_cloud;
        public bool has_an_avatar;
        public bool has_read_tnc;
        public Prosecution[] prosecutions;

        public void CopyTo(LoggedInUser user) {
            user.id = id;
            user.name = name;
            user.avatar = avatar;
            user.role = role;
            user.time_zone = time_zone;
            user.member_since = member_since;
            user.suspended_until = suspended_until;
            user.reason_for_suspension = reason_for_suspension;
            user.is_registered = is_registered;
            user.is_transient = is_transient;
            user.is_banned = is_banned;
            user.is_member_of_a_cloud = is_member_of_a_cloud;
            user.has_an_avatar = has_an_avatar;
            user.has_read_tnc = has_read_tnc;
            user.prosecutions = prosecutions;
        }
    }

    public class LoggedInUser : User {
        public string auth_token;
        public string email;
        public bool needs_to_confirm_registration;
        public bool needs_name_change;
        public Cloud[] clouds;
    }
}