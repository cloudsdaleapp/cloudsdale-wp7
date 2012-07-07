using System;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Cloudsdale {
    public partial class Clouds {
        public static bool wasoncloud;
        public MessageCacheController Controller { get; set; }

        // XNA Objects
        public readonly GameTimer timer;
        private readonly SharedGraphicsDeviceManager graphics = SharedGraphicsDeviceManager.Current;
        private readonly SpriteBatch spriteBatch;

        public Clouds() {
            Controller = MessageCacheController.GetCloud(Connection.CurrentCloud.id);
            InitializeComponent();

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            timer = new GameTimer {UpdateInterval = TimeSpan.FromTicks(333333)};
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;

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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            graphics.GraphicsDevice.SetSharingMode(true);
            timer.Start();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            timer.Stop();
            graphics.GraphicsDevice.SetSharingMode(false);
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

        private void OnUpdate(object sender, GameTimerEventArgs e) {
            
        }

        private void OnDraw(object sender, GameTimerEventArgs e) {

        }
    }
}
