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
    public class SimpleUser : UserReference {
        public string name;
        public Avatar avatar;
        public string role;
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
    }

    public class LoggedInUser : User {
        public string auth_token;
        public string email;
        public bool needs_to_confirm_registration;
        public bool needs_name_change;
        public Cloud[] clouds;
    }
}
