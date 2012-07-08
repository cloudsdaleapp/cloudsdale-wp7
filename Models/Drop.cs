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
using Cloudsdale.FayeConnector.ResponseTypes;
using Microsoft.Phone.Tasks;

namespace Cloudsdale.Models {
    public class Drop : CloudsdaleItem {
        public Uri url { get; set; }
        public string title { get; set; }
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

    public class FayeDropResponse : Response {
        public Drop data;
    }
}
