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

namespace Cloudsdale.Models {
    public class Message {
        public DateTime timestamp;
        public string content;
        public SimpleUser user;
        public Topic topic;
    }

    public class WebMessageResponse {
        public Message[] result;
    }

    public class Topic {
        public string type;
        public string id;
    }
}
