using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Res = Cloudsdale.Resources;

namespace Cloudsdale {
    public partial class Clouds {
        public Clouds() {
            InitializeComponent();
            cloudPivot.Title = Connection.CurrentCloud.name;
            Connection.Faye.ChannelMessageRecieved += OnMessage;
            Connection.Faye.Subscribe(Res.FayeMessageChannel.Replace("{cloudid}", Connection.CurrentCloud.id));
            Connection.Faye.Subscribe(Res.FayeDropsChannel.Replace("{cloudid}", Connection.CurrentCloud.id));
            var wc = new WebClient();
            wc.DownloadStringCompleted += (sender, args) => {
                if (args.UserState == null) {
                    var messages = JsonConvert.DeserializeObject<Models.WebMessageResponse>(args.Result).result;
                    foreach (var message in messages) {
                        AddChat(message);
                    }
                    wc.DownloadStringAsync(new Uri(Res.PreviousDropsEndpoint.Replace("{cloudid}", Connection.CurrentCloud.id)), new object());
                } else {
                    var drops = JsonConvert.DeserializeObject<Models.WebDropResponse>(args.Result).result;
                    Array.Reverse(drops);
                    foreach (var drop in drops) {
                        AddMedia(drop);
                    }
                    Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(Chats.ActualHeight));
                }
            };
            wc.DownloadStringAsync(new Uri(Res.PreviousMessagesEndpoint.Replace("{cloudid}", Connection.CurrentCloud.id)), null);
        }

        private void OnMessage(object sender, FayeConnector.FayeConnector.DataReceivedEventArgs args) {
            if (args.Channel == Res.FayeMessageChannel.Replace("{cloudid}", Connection.CurrentCloud.id)) {
                var message = JsonConvert.DeserializeObject<Models.FayeMessageResponse>(args.Data).data;
                Dispatcher.BeginInvoke(() => AddChat(message));
            } else if (args.Channel == Res.FayeDropsChannel.Replace("{cloudid}", Connection.CurrentCloud.id)) {
                var drop = JsonConvert.DeserializeObject<Models.FayeDropResponse>(args.Data).data;
                Dispatcher.BeginInvoke(() => AddMedia(drop));
            }
        }

        public void AddPony(string name, Uri imageurl) {
            var grid = new Grid {
                Height = 50,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var img = new Image {
                Source = new BitmapImage(imageurl),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 50
            };
            grid.Children.Add(img);
            var ponyname = new TextBlock {
                Text = name,
                FontSize = 34,
                Margin = new Thickness(60, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.Black)
            };
            grid.Children.Add(ponyname);
            Ponies.Items.Add(grid);
        }

        public void RemovePony(string name) {
            foreach (var grid in from grid in Ponies.Items.OfType<Grid>() from child in grid.Children.OfType<TextBlock>() where child.Text.Equals(name, StringComparison.InvariantCultureIgnoreCase) select grid) {
                Ponies.Items.Remove(grid);
            }
        }

        public void AddChat(Models.Message chat, bool recursive = false) {
            if (string.IsNullOrWhiteSpace(chat.content)) return;
            if (AppendChatToLast(chat.user.name, chat.content)) return;
            var lines = chat.content.Split('\n');
            var msg = lines[0].Trim();

            while (Chats.Items.Count > 50) {
                Chats.Items.RemoveAt(0);
            }
            var grid = new Grid {
                Margin = new Thickness(0, 0, 0, 5)
            };
            var img = new Image {
                Source = new BitmapImage(chat.user.avatar.Chat),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 50,
                Height = 50
            };
            grid.Children.Add(img);
            var ponyname = new TextBlock {
                Text = chat.user.name,
                FontSize = 18,
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x5F, 0x8F, 0xCF)),
                Margin = new Thickness(60, 0, 0, 0)
            };
            grid.Children.Add(ponyname);
            var stack = new StackPanel {
                Margin = new Thickness(60, 20, 0, 0)
            };
            var chatbox = new TextBlock {
                Text = msg,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black),
                FontFamily = new FontFamily("Verdana"),
            };
            if (msg.StartsWith(">")) {
                chatbox.Foreground = new SolidColorBrush(Color.FromArgb(255,100,155,100));
            }
            stack.Children.Add(chatbox);
            grid.Children.Add(stack);
            Chats.Items.Add(grid);

            if (lines.Length > 1) {
                var sb = new StringBuilder();
                for (var i = 1; i < lines.Length; ++i) {
                    sb.Append(lines[i] + "\n");
                }
                AppendChatToLast(chat.user.name, sb.ToString());
            }
        }

        private bool AppendChatToLast(string name, string chat) {
            if (string.IsNullOrWhiteSpace(chat)) return true;
            if (Chats.Items.Count < 1) return false;
            var grid = Chats.Items.Last() as Grid;
            if (grid == null) return false;
            var tbs = grid.Children.OfType<TextBlock>().ToArray();
            if (tbs.Length != 1) return false;
            if (tbs[0].Text != name) return false;
            var stacks = grid.Children.OfType<StackPanel>().ToArray();
            if (stacks.Length != 1) return false;
            var stack = stacks[0];

            var lines = chat.Split('\n');
            chat = lines[0].Trim();

            var chatbox = new TextBlock {
                Text = chat,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black),
                FontFamily = new FontFamily("Verdana"),
            };
            if (chat.StartsWith(">")) {
                chatbox.Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 155, 100));
            }
            stack.Children.Add(chatbox);

            if (lines.Length > 1) {
                var sb = new StringBuilder();
                for (var i = 1; i < lines.Length; ++i) {
                    sb.Append(lines[i] + "\n");
                }
                AppendChatToLast(name, sb.ToString());
            }

            return true;
        }

        public void AddMedia(Models.Drop drop) {
            var grid = new Grid {
                Margin = new Thickness(0,0,0,5)
            };
            var img = new Image {
                Source = new BitmapImage(drop.preview),
                Width = 120,
                Height = 90,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5,5,0,0)
            };
            grid.Children.Add(img);
            var titlebox = new TextBlock {
                Text = drop.title,
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(130, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.Black),
                FontFamily = new FontFamily("Verdana"),
            };
            grid.Children.Add(titlebox);

            var baseproj = grid.Projection;
            grid.MouseLeftButtonDown += (sender, args) => {
                var proj = new PlaneProjection {
                    RotationX = 15,
                    RotationY = -15,
                };
                grid.Projection = proj;
            };
            grid.MouseLeftButtonUp += (sender, args) => {
                grid.Projection = baseproj;
                Dispatcher.BeginInvoke(() => new WebBrowserTask { Uri = drop.url }.Show());
            };
            grid.MouseLeave += (sender, args) => {
                grid.Projection = baseproj;
            };

            MediaList.Items.Insert(0, grid);
        }

        public void ClearPonies() {
            Ponies.Items.Clear();
        }
        public void ClearChat() {
            Chats.Items.Clear();
        }
        public void ClearMedia() {
            MediaList.Items.Clear();
        }

        private void PhoneApplicationPageOrientationChanged(object sender, OrientationChangedEventArgs e) {
            if (e.Orientation == PageOrientation.PortraitUp) {
                cloudPivot.Background = (Brush) Resources["PortraitBackground"];
            } else {
                cloudPivot.Background = (Brush) Resources["LandscapeBackground"];
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            Connection.Faye.Unsubscribe(Res.FayeMessageChannel.Replace("{cloudid}", Connection.CurrentCloud.id));
            Connection.Faye.Unsubscribe(Res.FayeDropsChannel.Replace("{cloudid}", Connection.CurrentCloud.id));
            Connection.Faye.ChannelMessageRecieved -= OnMessage;
            base.OnNavigatedFrom(e);
        }
    }
}