using System;
using System.Collections.Generic;

namespace CloudsdaleWin7.lib.Models.Client
{
    public class LastSession
    {
        public string UserId;
        public Dictionary<string, DateTime> LastLogins = new Dictionary<string, DateTime>();
    }
}
