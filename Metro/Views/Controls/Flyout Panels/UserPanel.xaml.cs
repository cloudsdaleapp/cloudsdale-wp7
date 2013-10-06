using System;
using System.Diagnostics;
using System.Net.Http;
using CloudsdaleLib;
using CloudsdaleLib.Helpers;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Common;
using Cloudsdale_Metro.Helpers;
using Cloudsdale_Metro.Views.Controls.Flyout_Panels;
using Newtonsoft.Json;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Newtonsoft.Json.Linq;

namespace Cloudsdale_Metro.Views.Controls {
    public sealed partial class UserPanel {
        private readonly LayoutAwarePage.ObservableDictionary<string, object> _defaultViewModel
            = new LayoutAwarePage.ObservableDictionary<string, object>();
        public IObservableMap<string, object> DefaultViewModel { get { return _defaultViewModel; } }

        protected override string Header {
            get { return User.Name; }
        }

        protected override Uri Image {
            get { return null; }
        }

        protected override bool IsSettings {
            get { return false; }
        }

        private User User { get; set; }
        private Session Session { get; set; }
        private Cloud Cloud { get; set; }
        public UserPanel(User user) {
            User = user;
            Session = App.Connection.GetSession();
            Cloud = App.Connection.MessageController.CurrentCloud.Cloud;

            InitializeComponent();

            InitializeFlyout();
        }

        private async void UserPanel_OnLoaded(object sender, RoutedEventArgs e) {
            DefaultViewModel["User"] = User;
            DefaultViewModel["HasAka"] = User.AlsoKnownAs.Length > 0;
            DefaultViewModel["IsModerator"] = Session.IsModerator();
            DefaultViewModel["CanBan"] = false;
            DefaultViewModel["TrollBan"] = false;

            if (App.Connection.GetSession().IsModerator()) {
                DefaultViewModel["Bans"] = new Ban[0];
                DefaultViewModel["BansLoading"] = true;

                var client = new HttpClient {
                    DefaultRequestHeaders = {
                        {"Accept", "application/json"},
                        {"X-Auth-Token", Session.AuthToken}
                    },
                };

                var response = await client.GetAsync(
                    Endpoints.CloudUserBans
                    .Replace("[:id]", Cloud.Id)
                    .Replace("[:offender_id]", User.Id));

                var resultData = await response.Content.ReadAsStringAsync();

                try {
                    var bans = await JsonConvert.DeserializeObjectAsync<WebResponse<Ban[]>>(resultData);
                    DefaultViewModel["BansLoading"] = false;
                    DefaultViewModel["Bans"] = bans.Result;
                } catch (JsonException) { }
            }

            DefaultViewModel["CanBan"] =
                User.Id != Session.Id
                && !User.IsModerator() || Session.IsOwner();
            DefaultViewModel["TrollBan"] =
                User.Role == "founder" || User.Role == "developer";
        }

        private async void RevokeBan(object sender, RoutedEventArgs e) {
            await ((Ban)((FrameworkElement)sender).DataContext).UpdateProperty<Ban>(false, "revoke".KeyOf<JToken>("true"));
        }

        private void BanClick(object sender, RoutedEventArgs e) {
            new BanPanel((User)((FrameworkElement)sender).DataContext).FlyOut();
        }
    }
}
