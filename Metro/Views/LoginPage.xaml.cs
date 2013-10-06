using System.Threading.Tasks;
using System.Linq;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Models;
using Cloudsdale_Metro.Views.LoadPages;
using Newtonsoft.Json;
using WinRTXamlToolkit.AwaitableUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Cloudsdale_Metro.Views {
    public sealed partial class LoginPage {
        private static readonly LoginForm LoginForm = new LoginForm();

        public LoginPage() {
            InitializeComponent();
            DataContext = LoginForm;

            SessionGrid.ItemsSource = App.Connection.SessionController.PastSessions;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            CloudCanvas.StartLoop();
            var loginSuccessful = false;
            do {
                try {
                    await DoLastLogin();
                    loginSuccessful = true;
                } catch (JsonException) {

                }
            } while (!loginSuccessful);
            LoginForm.Session = App.Connection.SessionController.CurrentSession;
        }

        private async Task DoLastLogin() {
            if (App.Connection.SessionController.LastSession == null) return;
            var session = App.Connection.SessionController.PastSessions.FirstOrDefault(
                user => user.Id == App.Connection.SessionController.LastSession);
            if (session == null) return;
            LoginForm.Session = session;
            await this.WaitForLoadedAsync();

            await DoLogin();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CloudCanvas.Stop();
        }

        private async void LoginClick(object sender, RoutedEventArgs e) {
            await DoLogin();
        }

        private void SessionGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (CloudsdaleModel item in e.AddedItems) {
                item.UIMetadata["Selected"].Value = true;
            }
            foreach (CloudsdaleModel item in e.RemovedItems) {
                item.UIMetadata["Selected"].Value = false;
            }

            var grid = (GridView)sender;
            if (grid.SelectedIndex == -1) {
                LoginForm.Session = null;
            } else {
                LoginForm.Session = (Session)grid.SelectedItem;
            }
        }

        async Task DoLogin() {
            Frame.Navigate(typeof(LoggingIn));
            await App.Connection.SessionController.LogIn(LoginForm);
        }
    }
}
