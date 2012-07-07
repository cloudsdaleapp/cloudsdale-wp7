using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Cloudsdale {
    public partial class Clouds {
        public static bool wasoncloud;
        public MessageCacheController Controller { get; set; }

        public Clouds() {
            Controller = MessageCacheController.GetCloud(Connection.CurrentCloud.id);
            InitializeComponent();

            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                bool autocorrect;
                if (settings.TryGetValue("ux.autocorrect", out autocorrect) && !autocorrect) {
                    var scope = new InputScope();
                    var name = new InputScopeName {NameValue = InputScopeNameValue.Default};
                    scope.Names.Add(name);
                    SendBox.InputScope = scope;
                } else {
                    var scope = new InputScope();
                    var name = new InputScopeName {NameValue = InputScopeNameValue.Chat};
                    scope.Names.Add(name);
                    SendBox.InputScope = scope;
                }
            }

            cloudPivot.Title = Connection.CurrentCloud.name;
            Chats.ItemsSource = Controller.Messages;
            Controller.Messages.CollectionChanged += (sender, args) => 
                new Thread(() => {
                    Thread.Sleep(100);
                    Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
                }).Start();
            MediaList.ItemsSource = Controller.Drops;
            Ponies.ItemsSource = Controller.Users;

            new Thread(() => {
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
            }).Start();
            wasoncloud = true;
        }

        private void PhoneApplicationPageOrientationChanged(object sender, OrientationChangedEventArgs e) {
            if (e.Orientation == PageOrientation.PortraitUp) {
                cloudPivot.Background = (Brush) Resources["PortraitBackground"];
                SystemTray.IsVisible = true;
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
            } else {
                cloudPivot.Background = (Brush) Resources["LandscapeBackground"];
                SystemTray.IsVisible = false;
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            Controller.MarkAsRead();
            wasoncloud = false;
        }

        private void SendBoxKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Connection.SendMessage(Connection.CurrentCloud.id, SendBox.Text);
                SendBox.Text = "";
            }
        }

        private void DropItemClick(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            if (button == null) return;
            var drop = button.DataContext as Drop;
            if (drop == null) return;
            drop.OpenInBrowser();
        }
    }
}
