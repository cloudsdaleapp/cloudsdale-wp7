using System;

namespace Cloudsdale.Models {
    public class Cloud : CloudsdaleItem {
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
