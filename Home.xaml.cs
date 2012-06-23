using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cloudsdale {
    public partial class Home {
        public Home() {
            InitializeComponent();
            AddCloud("Connors Minecraft Server", "4f2ae8575af5a07a0d001eec", new Uri("http://c775850.r50.cf2.rackcdn.com/avatars/4fd8328bcff4e82543000229/2bad756d69-avatar.png"));
        }

        public void AddCloud(string name, string cloudid, Uri image) {
            var grid = new Grid {
                Margin = new Thickness(0,0,0,5),
                Height = 50
            };
            var img = new Image() {
                Source = new BitmapImage(image),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 50
            };
            grid.Children.Add(img);
            var cloudname = new TextBlock {
                Text = name,
                FontSize = 32,
                Margin = new Thickness(60, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.Black)
            };
            grid.Children.Add(cloudname);
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
                Connection.CurrentCloudId = cloudid;
                Connection.CurrentCloudName = name;
                NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative));
            };
            grid.MouseLeave += (sender, args) => {
                grid.Projection = baseproj;
            };

            CloudList.Items.Add(grid);
        }
    }
}