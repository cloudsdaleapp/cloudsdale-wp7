﻿using System;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Cloudsdale {
    public partial class Clouds {
        public static bool wasoncloud;
        public DerpyHoovesMailCenter Controller { get; set; }

        public Clouds() {
            Controller = DerpyHoovesMailCenter.GetCloud(Connection.CurrentCloud.id);
            InitializeComponent();

            cloudPivot.Title = Connection.CurrentCloud.name;
            Chats.ItemsSource = Controller.Messages;
            MediaList.ItemsSource = Controller.Drops;
            Ponies.ItemsSource = Controller.Users;

            ScrollDown(null, null);
            wasoncloud = true;
        }

        public void ScrollDown(object sender, EventArgs args) {
            new Thread(() => {
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
            }).Start();
        }

        private void PhoneApplicationPageOrientationChanged(object sender, OrientationChangedEventArgs e) {
            if (e.Orientation == PageOrientation.PortraitUp) {
                cloudPivot.Background = (Brush)Resources["PortraitBackground"];
                SystemTray.IsVisible = true;
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
            } else {
                cloudPivot.Background = (Brush)Resources["LandscapeBackground"];
                SystemTray.IsVisible = false;
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            Controller.Messages.CollectionChanged += ScrollDown;
            ScrollDown(null, null);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            Controller.Messages.CollectionChanged -= ScrollDown;
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

            LastDropClicked = drop;
            NavigationService.Navigate(new Uri("/DropViewer.xaml", UriKind.Relative));
        }

        public static Drop LastDropClicked;

        private Popup pop;
        private void SendBoxDoubleTap(object sender, GestureEventArgs e) {
            if (pop != null && pop.IsOpen) {
                pop.IsOpen = false;
                return;
            }

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("ux.allowaltcodes")) {
                return;
            }

            var grid = new Grid {
                Width = 250,
                Height = 75
            };

            var inputScope = new InputScope();
            var name = new InputScopeName { NameValue = InputScopeNameValue.Number };
            inputScope.Names.Add(name);

            var tb = new TextBox {
                Margin = new Thickness(5, 0, 85, 0),
                Padding = new Thickness(0),
                InputScope = inputScope,
                FontSize = 30,
                MaxLength = 5
            };
            grid.Children.Add(tb);

            var btn = new Button {
                Width = 85,
                Margin = new Thickness(5),
                Content = "INS",
                HorizontalAlignment = HorizontalAlignment.Right,
                Padding = new Thickness(0),
            };
            btn.Click += (o, args) => {
                ushort value;
                if (!ushort.TryParse(tb.Text, out value)) {
                    MessageBox.Show("Value must be between 0 and 65535");
                    tb.Focus();
                    return;
                }
                SendBox.Text += (char)value;
                tb.Text = "";
                if (IsolatedStorageSettings.ApplicationSettings.Contains("ux.recursivealtcodeentry")) {
                    tb.Focus();
                } else {
                    pop.IsOpen = false;
                    SendBox.Focus();
                    SendBox.Select(SendBox.Text.Length, 0);
                }
            };
            grid.Children.Add(btn);

            var border = new Border {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x91, 0xDB)),
                Child = grid
            };

            pop = new Popup {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 75),
                Width = 250,
                Height = 75,
                Child = border
            };
            LayoutRoot.Children.Add(pop);

            pop.IsOpen = true;

            tb.Focus();
        }

        private void SendBoxTap(object sender, GestureEventArgs e) {
            if (pop != null && pop.IsOpen) {
                pop.IsOpen = false;
            }
        }

        private void ChatScrollerTap(object sender, GestureEventArgs e) {
            if (pop != null && pop.IsOpen) {
                pop.IsOpen = false;
            }
        }
    }
}
