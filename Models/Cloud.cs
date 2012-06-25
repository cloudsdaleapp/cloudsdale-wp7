using System;
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
    public struct Cloud {
        public string id;
        public string name;
        public string description;
        public DateTime created_at;
        public string rules;
        public bool hidden;
        public Avatar avatar;
        public bool is_transient;
        public UserReference owner;
        public UserReference[] moderators;
        public CloudChatInfo chat;
    }

    public struct CloudChatInfo {
        public DateTime last_message_at;
    }
}
