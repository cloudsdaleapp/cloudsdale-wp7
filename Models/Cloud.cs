using System;
using Newtonsoft.Json;

namespace Cloudsdale.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Cloud : CloudsdaleItem {
        [JsonProperty]
        public string name;
        [JsonProperty]
        public string description;
        [JsonProperty]
        public DateTime? created_at;
        [JsonProperty]
        public string rules;
        [JsonProperty]
        public bool? hidden;
        [JsonProperty]
        public Avatar avatar;
        [JsonProperty]
        public bool? is_transient;
        [JsonProperty]
        public UserReference owner;
        [JsonProperty]
        public UserReference[] moderators;
        [JsonProperty]
        public CloudChatInfo chat;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CloudChatInfo {
        [JsonProperty]
        public DateTime? last_message_at;
    }
}
