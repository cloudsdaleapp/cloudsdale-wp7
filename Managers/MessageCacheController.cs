using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Cloudsdale.Managers {
    public class MessageCacheController {
        private static Dictionary<string, MessageCacheController> cache =
            new Dictionary<string, MessageCacheController>();

        public static void Init() {
            Connection.Faye.ChannelMessageRecieved += FayeMessageRecieved;
        }

        static void FayeMessageRecieved(object sender, FayeConnector.FayeConnector.DataReceivedEventArgs e) {

        }

        private readonly GenericBinding<String> textblockbinding = new GenericBinding<string>(TextBlock.TextProperty);
        private int unread = 0;
        public void MarkAsRead() {
            unread = 0;
            DoUpdates();
        }
        private void DoUpdates() {
            textblockbinding.Value = (unread > 0) ? unread.ToString() : "";
        }


        public static void Subscribe(string cloud) {
            Connection.Faye.Subscribe("clouds/" + cloud + "/chat/messages");
        }

        public static void Unsubscribe(string cloud) {
            Connection.Faye.Unsubscribe("clouds/" + cloud + "/chat/messages");
        }
    }
}
