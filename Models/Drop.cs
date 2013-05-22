using System;
using System.Runtime.Serialization;
using System.Windows;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext) {
            var model = errorContext.OriginalObject;
            var modelType = model.GetType();
            var memberName = errorContext.Member.ToString();
            var field = modelType.GetField(memberName);
            if (field.FieldType == typeof(DateTime?)) {
                field.SetValue(model, DateTime.MaxValue);
            } else if (field.FieldType == typeof(string)) {
                field.SetValue(model, "");
            } else {
                field.SetValue(model, null);
            }
            errorContext.Handled = true;
        }
    }

    public class WebDropResponse {
        public Drop[] result;
    }
}
