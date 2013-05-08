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
using Microsoft.Phone.Notification;

namespace Cloudsdale {
    public static class Toasts {
        public static readonly string ChannelName = "CDNotifications";

        public static event EventHandler<NotificationChannelErrorEventArgs> ErrorOccurred; 

        public static void GetToastUri(Action<Uri> callback) {
            var channel = HttpNotificationChannel.Find(ChannelName);
            if (channel == null) {
                channel = new HttpNotificationChannel(ChannelName);

                channel.RegisterEvents(callback);

                channel.Open();
                channel.BindToShellToast();
            } else {
                channel.RegisterEvents(callback);
            }

            callback(channel.ChannelUri);
        }

        private static void RegisterEvents(this HttpNotificationChannel channel, Action<Uri> changedCallback) {
            channel.ChannelUriUpdated += (sender, args) => changedCallback(args.ChannelUri);
            channel.ErrorOccurred += ErrorOccurred;
        }
    }
}
