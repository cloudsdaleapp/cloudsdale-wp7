using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
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
        private static readonly Queue<Request> QueueL = new Queue<Request>();
        private static readonly Queue<Request> QueueM = new Queue<Request>();
        private static readonly Queue<Request> QueueH = new Queue<Request>();
        private static readonly Timer TimerL = new Timer(HandleL, null, 10, 200);
        private static readonly Timer TimerM = new Timer(HandleM, null, 10, 100);
        private static readonly Timer TimerH = new Timer(HandleH, null, 10, 50);

        private static void HandleL(object state) {
            if (QueueL.Count == 0) return;
            var item = QueueL.Dequeue();
            var wc = new WebClient();
            foreach (var header in item.headers) {
                wc.Headers[header.Key] = header.Value;
            }
            wc.Encoding = Encoding.UTF8;
            wc.DownloadStringCompleted += (sender, args) => item.callback(args);
            wc.DownloadStringAsync(item.uri);
        }

        private static void HandleM(object state) {
            if (QueueM.Count == 0) return;
            var item = QueueM.Dequeue();
            var wc = new WebClient();
            foreach (var header in item.headers) {
                wc.Headers[header.Key] = header.Value;
            }
            wc.Encoding = Encoding.UTF8;
            wc.DownloadStringCompleted += (sender, args) => item.callback(args);
            wc.DownloadStringAsync(item.uri);
        }

        private static void HandleH(object state) {
            if (QueueH.Count == 0) return;
            var item = QueueH.Dequeue();
            var wc = new WebClient();
            foreach (var header in item.headers) {
                wc.Headers[header.Key] = header.Value;
            }
            wc.Encoding = Encoding.UTF8;
            wc.DownloadStringCompleted += (sender, args) => item.callback(args);
            wc.DownloadStringAsync(item.uri);
        }

        public static void BeginLowPriorityRequest(Uri uri, Action<DownloadStringCompletedEventArgs> callback,
            params KeyValuePair<string, string>[] headers) {
            QueueL.Enqueue(new Request {
                uri = uri,
                callback = callback,
                headers = headers
            });
        }

        public static void BeginMediumPriorityRequest(Uri uri, Action<DownloadStringCompletedEventArgs> callback,
            params KeyValuePair<string, string>[] headers) {
            QueueM.Enqueue(new Request {
                uri = uri,
                callback = callback,
                headers = headers
            });
        }

        public static void BeginHighPriorityRequest(Uri uri, Action<DownloadStringCompletedEventArgs> callback,
            params KeyValuePair<string, string>[] headers) {
            QueueH.Enqueue(new Request {
                uri = uri,
                callback = callback,
                headers = headers
            });
        }

        private class Request {
            internal Uri uri;
            internal Action<DownloadStringCompletedEventArgs> callback;
            internal KeyValuePair<string, string>[] headers;
        }
    }
}
