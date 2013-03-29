using System;

namespace Cloudsdale.Models {
    public class Ban {
        public string reason;
        public DateTime? due;
        public DateTime? created_at;
        public DateTime? updated_at;
        public bool? revoke;
        public string id;
        public string offender_id;
        public string enforcer_id;
        public string jurisdiction_id;
        public string jurisdiction_type;
        public bool? has_expired;
        public bool? is_active;
        public bool? is_transient;
    }
}
