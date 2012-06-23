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
using CodeTitans.Bayeux;

namespace Cloudsdale {
    public static class Connection {
        public static string CurrentCloudId;
        public static string CurrentCloudName;

        private static BayeuxConnection bayeux;

        public static void Connect() {
            bayeux = new BayeuxConnection(Resources.pushUrl);
            bayeux.Connected += Connected;
        }

        static void Connected(object sender, BayeuxConnectionEventArgs e) {
            
        }
    }
}
