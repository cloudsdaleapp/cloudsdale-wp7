using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Res = Cloudsdale.Resources;

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
            } else {
                cloudPivot.Background = (Brush) Resources["LandscapeBackground"];
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            Controller.MarkAsRead();
            wasoncloud = false;
            base.OnNavigatedFrom(e);
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
