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

namespace Cloudsdale.FayeConnector.ResponseTypes {
    public class SubscribeResponse : Response {
        public bool successful;
        public string clientId;
        public string subscribtion;
        public string error;
    }
    public class UnsubscribeResponse : Response {
        public bool successful;
        public string clientId;
        public string subscribtion;
        public string error;
    }
}
