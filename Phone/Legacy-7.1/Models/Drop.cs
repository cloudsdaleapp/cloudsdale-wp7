using System;
using System.Windows;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;

namespace Cloudsdale.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Drop : CloudsdaleItem {
        [JsonProperty]
        public Uri url { get; set; }
        [JsonProperty]
        public string title { get; set; }
        [JsonProperty]
        public Uri preview { get; set; }

        public void OpenInBrowser() {
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                OpenInBrowserInternal();
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(OpenInBrowserInternal);
            }
        }

        private void OpenInBrowserInternal() {
            var task = new WebBrowserTask {
                Uri = url
            };
            task.Show();
        }
    }

    public class WebDropResponse {
        public Drop[] result;
    }
}
