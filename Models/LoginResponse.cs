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
    public struct LoginResponse {
        public int status;
        public object[] errors;
        public object flash;
        public Result result;
    }

    public struct Result {
        public string client_id;
        public LoggedInUser user;
    }
}
