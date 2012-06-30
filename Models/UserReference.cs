using System;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace Cloudsdale.Models {
    public class UserReference {
        public string id;

        public User FullUser {
            get {
                var wc = new WebClient();
                var mre = new ManualResetEvent(false);
                var jsonresult = "";
                wc.DownloadStringCompleted += (sender, args) => {
                    jsonresult = args.Result;
                    mre.Set();
                };
                wc.DownloadStringAsync(new Uri(string.Format(Resources.getUserEndpoint, id)));
                mre.WaitOne();

                var settings = new JsonSerializerSettings {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    CheckAdditionalContent = false
                };
                try {
                    var res = JsonConvert.DeserializeObject<GetUserResult>(jsonresult, settings);
                    return res.result;
                } catch {
                    return new User();
                }
            }
        }

        public struct GetUserResult {
            public User result;
        }
    }
}
