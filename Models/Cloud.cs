using System;
using System.Runtime.Serialization;

namespace Cloudsdale.Models {
    [DataContract]
    [KnownType(typeof(CloudChatInfo))]
    [KnownType(typeof(UserReference))]
    [KnownType(typeof(Avatar))]
    public class Cloud : CloudsdaleItem {
        [DataMember]
        public string name;
        [DataMember]
        public string description;
        [DataMember]
        public DateTime? created_at;
        [DataMember]
        public string rules;
        [DataMember]
        public bool? hidden;
        [DataMember]
        public Avatar avatar;
        [DataMember]
        public bool? is_transient;
        [DataMember]
        public UserReference owner;
        [DataMember]
        public UserReference[] moderators;
        [DataMember]
        public CloudChatInfo chat;
    }

    [DataContract]
    public class CloudChatInfo {
        [DataMember]
        public DateTime? last_message_at;
    }
}
