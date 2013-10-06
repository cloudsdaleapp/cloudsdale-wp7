using System;
using System.Collections.Generic;

namespace Cloudsdale_Metro.Models {
    public class LastSession {
        public string UserId;
        public Dictionary<string, DateTime> LastLogins = new Dictionary<string, DateTime>();
    }
}
