using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using Res = Cloudsdale.Resources;

// http://i.qkme.me/3597jb.jpg GO TO BED

namespace Cloudsdale {
    public partial class Clouds {
        public static bool wasoncloud;
        public DerpyHoovesMailCenter Controller { get; set; }

        public Clouds() {
            if (Connection.CurrentCloud == null) {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                return;
            }
            Controller = DerpyHoovesMailCenter.GetCloud(Connection.CurrentCloud.id);
            InitializeComponent();

            cloudPivot.Title = Connection.CurrentCloud.name;

            new Thread(() => {
                Thread.Sleep(50);
                Dispatcher.BeginInvoke(() => {
                    Chats.ItemsSource = Controller.Messages;
                    MediaList.ItemsSource = Controller.Drops;
                    Ponies.ItemsSource = Controller.Users;
                    ScrollDown(null, null);
                });
            }).Start();

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
                ScrollDown(null, null);
            } else {
                cloudPivot.Background = (Brush)Resources["LandscapeBackground"];
                SystemTray.IsVisible = false;
                ScrollDown(null, null);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            Controller.Messages.CollectionChanged += ScrollDown;
            ScrollDown(null, null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            Controller.Messages.CollectionChanged -= ScrollDown;
            Controller.MarkAsRead();
            wasoncloud = false;
        }

        private void SendBoxKeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter) return;
            Connection.SendMessage(Connection.CurrentCloud.id, SendBox.Text);
            SendBox.Text = "";
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

        private void MoreDropsClick(object sender, RoutedEventArgs e) {
            MoreDrops.Visibility = Visibility.Collapsed;
            if (!Controller.DropController.CanIncreaseCapacity) {
                return;
            }
            var request = WebRequest.CreateHttp(new Uri(Res.PreviousDropsEndpoint.Replace("{cloudid}", Connection.CurrentCloud.id)));
            var nextpage = ((Controller.DropController.Capacity / 10) + 1).ToString();
            request.Headers["X-Result-Page"] = nextpage;
            request.BeginGetResponse(ai => {
                var response = request.EndGetResponse(ai);
                string resultString;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                    resultString = reader.ReadToEnd();
                }
                var result = JsonConvert.DeserializeObject<WebDropResponse>(resultString);
                Controller.DropController.IncreaseCapacity(result.result);
                if (Controller.DropController.CanIncreaseCapacity)
                    Dispatcher.BeginInvoke(() => MoreDrops.Visibility = Visibility.Visible);
            }, null);
        }

        private bool searching;
        private void SearchClick(object sender, RoutedEventArgs e) {
            if (!searching) {
                if (string.IsNullOrWhiteSpace(SearchBar.Text)) {
                    MessageBox.Show("Please enter a search query");
                    return;
                }
                searching = true;
                MoreDrops.IsEnabled = false;
                SearchButtonImage.Source = new BitmapImage(new Uri("/Images/Icons/back_white.png", UriKind.Relative));
                MoreDrops.Content = "Searching...";
                var searchList = new ObservableCollection<Drop>();
                MediaList.ItemsSource = searchList;
                var request = WebRequest.CreateHttp(
                    new Uri(Res.DropsSearchEndpoint.Replace("{cloudid}", Connection.CurrentCloud.id)
                                                   .Replace("{query}", Uri.EscapeDataString(SearchBar.Text.Trim()))));
                SearchBar.Text = "";
                request.BeginGetResponse(ai => {
                    var response = request.EndGetResponse(ai);
                    string resultString;
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                        resultString = reader.ReadToEnd();
                    }
                    var result = JsonConvert.DeserializeObject<WebDropResponse>(resultString);
                    Dispatcher.BeginInvoke(() => {
                        MoreDrops.Visibility = Visibility.Collapsed;
                        foreach (var d in result.result) {
                            searchList.Add(d);
                        }
                    });
                }, null);
            } else {
                searching = false;
                MoreDrops.IsEnabled = true;
                MoreDrops.Content = "Load More";
                MoreDrops.Visibility = Visibility.Visible;
                MediaList.ItemsSource = Controller.Drops;
                SearchButtonImage.Source = new BitmapImage(new Uri("/Images/Icons/search.png", UriKind.Relative));
            }
        }

        private void DropImageFailed(object sender, ExceptionRoutedEventArgs e) {
            if (sender is Image) {
                var image = sender as Image;
                image.Source = new BitmapImage(new Uri("http://assets.cloudsdale.org/assets/fallback/preview_thumb_drop.png"));
            }
        }

        private void AddOrRemoveCloudClick(object sender, RoutedEventArgs e) {
            var ms = Controller.Messages;
            Debugger.Break();
        }
    }
}
