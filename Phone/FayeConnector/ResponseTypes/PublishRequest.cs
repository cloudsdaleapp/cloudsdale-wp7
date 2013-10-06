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
    public class PublishRequest<T> {
        public T data;
        public string channel;
        public string clientId;
        public AuthTokenExt ext = new AuthTokenExt();
    }

    public class AuthTokenExt {
        public string auth_token = Connection.CurrentCloudsdaleUser.auth_token;
    }
}
