using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Cloudsdale.Managers {
    public static class WebPriorityManager {
        private static readonly List<Request> QueueL = new List<Request>();
        private static readonly List<Request> QueueM = new List<Request>();
        private static readonly List<Request> QueueH = new List<Request>();
        private static readonly Timer TimerL = new Timer(HandleL, null, 10, 200);
        private static readonly Timer TimerM = new Timer(HandleM, null, 10, 100);
        private static readonly Timer TimerH = new Timer(HandleH, null, 10, 50);

        private static void HandleL(object state) {
            if (QueueL.Count == 0) return;
            var item = QueueL[0];
            QueueL.RemoveAt(0);
            var wc = new WebClient();
            foreach (var header in item.headers) {
                wc.Headers[header.Key] = header.Value;
            }
            wc.DownloadStringCompleted += (sender, args) => item.callback(args);
            wc.DownloadStringAsync(item.uri);
        }

        private static void HandleM(object state) {
            if (QueueM.Count == 0) return;
            var item = QueueM[0];
            QueueM.RemoveAt(0);
            var wc = new WebClient();
            foreach (var header in item.headers) {
                wc.Headers[header.Key] = header.Value;
            }
            wc.DownloadStringCompleted += (sender, args) => item.callback(args);
            wc.DownloadStringAsync(item.uri);
        }

        private static void HandleH(object state) {
            if (QueueH.Count == 0) return;
            var item = QueueH[0];
            QueueH.RemoveAt(0);
            var wc = new WebClient();
            foreach (var header in item.headers) {
                wc.Headers[header.Key] = header.Value;
            }
            wc.DownloadStringCompleted += (sender, args) => item.callback(args);
            wc.DownloadStringAsync(item.uri);
        }

        public static void BeginLowPriorityRequest(Uri uri, Action<DownloadStringCompletedEventArgs> callback,
            params KeyValuePair<string, string>[] headers) {
            QueueL.Add(new Request {
                uri = uri, callback = callback, headers = headers
            });
        }

        public static void BeginMediumPriorityRequest(Uri uri, Action<DownloadStringCompletedEventArgs> callback,
            params KeyValuePair<string, string>[] headers) {
            QueueM.Add(new Request {
                uri = uri, callback = callback, headers = headers
            });
        }

        public static void BeginHighPriorityRequest(Uri uri, Action<DownloadStringCompletedEventArgs> callback,
            params KeyValuePair<string, string>[] headers) {
            QueueH.Add(new Request {
                uri = uri, callback = callback, headers = headers
            });
        }

        private class Request {
            internal Uri uri;
            internal Action<DownloadStringCompletedEventArgs> callback;
            internal KeyValuePair<string, string>[] headers;
        }
    }
}
