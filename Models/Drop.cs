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

namespace Cloudsdale.Models {
    public class Drop {
        public Uri url { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public Uri preview { get; set; }
    }

    public class WebDropResponse {
        public Drop[] result;
    }

    public class FayeDropResponse : Response {
        public Drop data;
    }
}
