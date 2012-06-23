using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;

// ReSharper disable ImplicitlyCapturedClosure
namespace Cloudsdale {
    public partial class Clouds {
        public Clouds() {
            InitializeComponent();
            AddPony("Connorcpu", new Uri("http://c775850.r50.cf2.rackcdn.com/avatars/4f4db65448f155761c001d9d/thumb_9145e3d94a-avatar.png"));
            AddChat("Connorcpu", "This is a big 'ole test chat, just to check that the system is working ;)",
                new Uri("http://c775850.r50.cf2.rackcdn.com/avatars/4f4db65448f155761c001d9d/thumb_9145e3d94a-avatar.png"));
            AddMedia("I'm gonna do an internet!", new Uri("http://www.youtube.com/watch?v=mdaCXH5gT_w"), new Uri("http://c775850.r50.cf2.rackcdn.com/previews/4fe49f50cff4e82ffc003b33/thumb_93d4929498-preview.png"));
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
                Margin = new Thickness(60, 0, 0, 0)
            };
            grid.Children.Add(ponyname);
            Ponies.Items.Add(grid);
        }

        public void AddChat(string name, string chat, Uri avatar) {
            while (Chats.Items.Count > 50) {
                Chats.Items.RemoveAt(0);
            }
            var grid = new Grid {
                Margin = new Thickness(0, 0, 0, 5)
            };
            var img = new Image {
                Source = new BitmapImage(avatar),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 50,
                Height = 50
            };
            grid.Children.Add(img);
            var ponyname = new TextBlock {
                Text = name,
                FontSize = 18,
                Foreground = new SolidColorBrush(Color.FromArgb(0xF0, 0xCF, 0xCF, 0xFF)),
                Margin = new Thickness(60, 0, 0, 0)
            };
            grid.Children.Add(ponyname);
            var chatbox = new TextBlock {
                Text = chat,
                FontSize = 16,
                Margin = new Thickness(60, 20, 0, 0),
                TextWrapping = TextWrapping.Wrap,
            };
            grid.Children.Add(chatbox);
            Chats.Items.Add(grid);
            Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(ChatScroller.ScrollableHeight));
        }

        public void AddMedia(string listname, Uri link, Uri preview) {
            var grid = new Grid {
                Margin = new Thickness(0,0,0,5)
            };
            var img = new Image {
                Source = new BitmapImage(preview),
                Width = 120,
                Height = 90,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5,5,0,0)
            };
            grid.Children.Add(img);
            var titlebox = new TextBlock {
                Text = listname,
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(130,0,0,0)
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
                Dispatcher.BeginInvoke(() => new WebBrowserTask { Uri = link }.Show());
            };
            grid.MouseLeave += (sender, args) => {
                grid.Projection = baseproj;
            };

            MediaList.Items.Insert(0, grid);
        }
    }
}
// ReSharper restore ImplicitlyCapturedClosure