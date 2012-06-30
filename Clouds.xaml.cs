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
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Res = Cloudsdale.Resources;

namespace Cloudsdale {
    public partial class Clouds {
        public static bool wasoncloud;

        public Clouds() {
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
            Connection.Faye.ChannelMessageRecieved += OnMessage;
            Connection.Faye.Subscribe(Res.FayeMessageChannel.Replace("{cloudid}", Connection.CurrentCloud.id));
            Connection.Faye.Subscribe(Res.FayeDropsChannel.Replace("{cloudid}", Connection.CurrentCloud.id));
            Connection.Faye.Subscribe(Res.FayeUsersChannel.Replace("{cloudid}", Connection.CurrentCloud.id));
            var wc = new WebClient();
            wc.DownloadStringCompleted += (sender, args) => {
                if (args.UserState == null) {
                    var messages = JsonConvert.DeserializeObject<WebMessageResponse>(args.Result).result;
                    foreach (var message in messages) {
                        AddChat(message);
                    }
                    wc.DownloadStringAsync(new Uri(Res.PreviousDropsEndpoint.Replace("{cloudid}", Connection.CurrentCloud.id)), new object());
                    new Thread(() => {
                        Thread.Sleep(50);
                        Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(Chats.ActualHeight));
                    }).Start();
                } else {
                    var drops = JsonConvert.DeserializeObject<WebDropResponse>(args.Result).result;
                    Array.Reverse(drops);
                    foreach (var drop in drops) {
                        AddMedia(drop);
                    }
                }
            };
            wc.DownloadStringAsync(new Uri(Res.PreviousMessagesEndpoint.Replace("{cloudid}", Connection.CurrentCloud.id)), null);
            wasoncloud = true;
        }

        internal void OnMessage(object sender, FayeConnector.FayeConnector.DataReceivedEventArgs args) {
            if (args.Channel == Res.FayeMessageChannel.Replace("{cloudid}", Connection.CurrentCloud.id)) {
                var message = JsonConvert.DeserializeObject<FayeMessageResponse>(args.Data).data;
                Dispatcher.BeginInvoke(() => AddChat(message));
            } else if (args.Channel == Res.FayeDropsChannel.Replace("{cloudid}", Connection.CurrentCloud.id)) {
                var drop = JsonConvert.DeserializeObject<FayeDropResponse>(args.Data).data;
                Dispatcher.BeginInvoke(() => AddMedia(drop));
            } else if (args.Channel == Res.FayeUsersChannel.Replace("{cloudid}", Connection.CurrentCloud.id)) {
                //Debugger.Break();
            }
        }

        public void AddPony(SimpleUser user) {
            var grid = new Grid {
                Height = 50,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var img = new Image {
                Source = new BitmapImage(user.avatar.Normal),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 50
            };
            grid.Children.Add(img);
            var ponyname = new TextBlock {
                Text = user.name,
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

        private readonly Regex _backslashN = new Regex("([^\\\\]|^)\\\\n");
        private readonly Regex _backslashT = new Regex("([^\\\\]|^)\\\\t");

        public void AddChat(Message chat, bool recursive = false) {
            if (string.IsNullOrWhiteSpace(chat.content)) return;
            if (AppendChatToLast(chat.user.name, chat.content)) return;
            chat.content = _backslashN.Replace(chat.content, "\n");
            var lines = chat.content.Split('\n');
            var msg = lines[0].Trim();
            chat.content = _backslashT.Replace(chat.content, "    ");
            chat.content = chat.content.Replace("\\\\", "\\");
            chat.content = Settings.ChatFilter.Filter(chat.content);

            while (Chats.Items.Count > 75) {
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
                Foreground = new SolidColorBrush(RoleColor(chat.user.role, Color.FromArgb(0xFF, 0x5F, 0x8F, 0xCF))),
                Margin = new Thickness(60, 0, 0, 0)
            };
            grid.Children.Add(ponyname);
            var tzi = TimeZoneInfo.Local;
            var tso = new TimeSpan(1, 0, 0);
            var timestamp = new TextBlock {
                Text = (chat.timestamp + tzi.BaseUtcOffset + tso).ToShortTimeString(),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x6F, 0x8F, 0x8F)),
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            grid.Children.Add(timestamp);
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
                chatbox.Foreground = new SolidColorBrush(Color.FromArgb(255, 50, 130, 50));
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

            if (Connection.Faye.Connected)
                new Thread(() => {
                    Thread.Sleep(50);
                    Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(Chats.ActualHeight));
                }).Start();
        }

        private bool AppendChatToLast(string name, string chat) {
            if (string.IsNullOrWhiteSpace(chat)) return true;
            if (Chats.Items.Count < 1) return false;
            var grid = Chats.Items.Last() as Grid;
            if (grid == null) return false;
            var tbs = grid.Children.OfType<TextBlock>().Where(tb => Math.Abs(tb.FontSize - 18f) < 1).ToArray();
            if (tbs.Length != 1) return false;
            if (tbs[0].Text != name) return false;
            var stacks = grid.Children.OfType<StackPanel>().ToArray();
            if (stacks.Length != 1) return false;
            var stack = stacks[0];

            chat = _backslashN.Replace(chat, "\n");
            var lines = chat.Split('\n');
            chat = lines[0].Trim();
            chat = _backslashT.Replace(chat, "    ");
            chat = chat.Replace("\\\\", "\\");
            chat = Settings.ChatFilter.Filter(chat);

            var chatbox = new TextBlock {
                Text = chat,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.Black),
                FontFamily = new FontFamily("Verdana"),
            };
            if (chat.StartsWith(">")) {
                chatbox.Foreground = new SolidColorBrush(Color.FromArgb(255, 50, 130, 50));
            }
            stack.Children.Add(chatbox);

            if (lines.Length > 1) {
                var sb = new StringBuilder();
                for (var i = 1; i < lines.Length; ++i) {
                    sb.Append(lines[i] + "\n");
                }
                AppendChatToLast(name, sb.ToString());
            }

            if (Connection.Faye.Connected)
                new Thread(() => {
                    Thread.Sleep(50);
                    Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(Chats.ActualHeight));
                }).Start();

            return true;
        }

        public void AddMedia(Drop drop) {
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
            var buttondownpoints = new StylusPointCollection();
            grid.MouseLeftButtonDown += (sender, args) => {
                buttondownpoints = args.StylusDevice.GetStylusPoints(LayoutRoot);
                var proj = new PlaneProjection {
                    RotationX = 15,
                    RotationY = -15,
                };
                grid.Projection = proj;
            };
            grid.MouseLeftButtonUp += (sender, args) => {
                grid.Projection = baseproj;
                var points = args.StylusDevice.GetStylusPoints(LayoutRoot);
                if (points.Count > 0 && buttondownpoints.Count > 0) {
                    if (points[0].X < buttondownpoints[0].X - 25 ||
                        points[0].X > buttondownpoints[0].X + 25 ||
                        points[0].Y < buttondownpoints[0].Y - 25 ||
                        points[0].Y > buttondownpoints[0].Y + 25)
                        return;
                }
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
            wasoncloud = false;
            base.OnNavigatedFrom(e);
        }

        private void SendBoxKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Connection.SendMessage(Connection.CurrentCloud.id, SendBox.Text);
                SendBox.Text = "";
            }
        }

        private static Color RoleColor(string role, Color defaultColor = default(Color)) {
            switch (role) {
                case "founder":
                    return Color.FromArgb(0xFF, 0xFF, 0x1F, 0x1F);
                case "admin":
                    return Color.FromArgb(0xFF, 0x1F, 0x7F, 0x1F);
                case "moderator":
                    return Color.FromArgb(0xFF, 0xFF, 0xAF, 0x1F);
                default:
                    return defaultColor;
            }
        }
    }
}
