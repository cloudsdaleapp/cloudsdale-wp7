using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Facebook;
using Microsoft.Phone.Controls;

namespace Cloudsdale.FacebookAuth {
    public partial class Login {

        private static readonly string AppId = Cloudsdale.Resources.facebookAppId;

        /// <summary>
        /// Extended permissions is a comma separated list of permissions to ask the user.
        /// </summary>
        /// <remarks>
        /// For extensive list of available extended permissions refer to 
        /// https://developers.facebook.com/docs/reference/api/permissions/
        /// </remarks>
        private const string ExtendedPermissions = "email";

        public Login() {
            InitializeComponent();
        }

        private void WebBrowser1Loaded(object sender, RoutedEventArgs e) {
            var loginUrl = GetFacebookLoginUrl(AppId, ExtendedPermissions);
            webBrowser1.Navigate(loginUrl);
        }

        private static Uri GetFacebookLoginUrl(string appId, string extendedPermissions) {
            var parameters = new Dictionary<string, object>();
            parameters["response_type"] = "token";
            parameters["display"] = "touch";

            return FacebookOAuthClient.GetLoginUrl(appId, new Uri("https://www.facebook.com/connect/login_success.html"), extendedPermissions.Split(','), parameters);
        }

        private void WebBrowser1Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            FacebookOAuthResult oauthResult;
            if (!FacebookOAuthResult.TryParse(e.Uri, out oauthResult)) {
                return;
            }

            if (oauthResult.IsSuccess) {
                var accessToken = oauthResult.AccessToken;
                LoginSucceded(accessToken);
            } else {
                // user cancelled
                MessageBox.Show(oauthResult.ErrorDescription);
            }
        }

        private void LoginSucceded(string accessToken) {
            var fb = new FacebookClient(accessToken);
            AutoResetEvent are = new AutoResetEvent(false);
            Dispatcher.BeginInvoke(() => {
                NavigationService.Navigate(new Uri("/Connecting.xaml", UriKind.Relative));
                are.Set();
            });

            fb.GetCompleted += (o, e) => {
                if (e.Error != null) {
                    Dispatcher.BeginInvoke(() => MessageBox.Show(e.Error.Message));
                    return;
                }

                var result = (IDictionary<string, object>)e.GetResultData();
                var id = (string)result["id"];

                Connection.FacebookUid = id;
                Connection.LoginType = 1;
                are.WaitOne();
                Connection.Connect((Page) ((PhoneApplicationFrame)Application.Current.RootVisual).Content);
            };

            fb.GetAsync("me?fields=id");
        }
    }
}