﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cloudsdale.Models;
using Newtonsoft.Json;
using Res = Cloudsdale.Resources;

namespace Cloudsdale {
    public partial class Home {
        public static bool comingfromhome = false;

        public Home() {
            comingfromhome = true;
            InitializeComponent();
            foreach (var cloud in Connection.CurrentCloudsdaleUser.clouds) {
                AddCloud(cloud);
            }
            var wc = new WebClient();
            wc.DownloadStringCompleted += (sender, args) => {
                var clouds = JsonConvert.DeserializeObject<CloudsRequest>(args.Result).result;
                foreach (var cloud in clouds) {
                    AddExploreCloud(cloud);
                }
            };
            wc.DownloadStringAsync(new Uri(Res.PopularCloudsEndpoint));
        }

        public void AddCloud(Cloud cloud) {
            var grid = new Grid {
                Margin = new Thickness(0, 0, 0, 5),
                Height = 50
            };
            var img = new Image {
                Source = new BitmapImage(cloud.avatar.Preview),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 50
            };
            grid.Children.Add(img);
            var cloudname = new TextBlock {
                Text = cloud.name,
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
                Connection.CurrentCloud = cloud;
                NavigationService.Navigate(new Uri("/Clouds.xaml", UriKind.Relative));
            };
            grid.MouseLeave += (sender, args) => {
                grid.Projection = baseproj;
            };

            CloudList.Items.Add(grid);
        }

        public void AddExploreCloud(Cloud cloud) {
            var grid = new Grid {
                Margin = new Thickness(0, 0, 0, 5),
                Height = 50
            };
            var img = new Image {
                Source = new BitmapImage(cloud.avatar.Preview),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 50
            };
            grid.Children.Add(img);
            var cloudname = new TextBlock {
                Text = cloud.name,
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
                if (MessageBox.Show("Do you want to join the cloud " + cloud.name + "?", "", 
                    MessageBoxButton.OKCancel) == MessageBoxResult.OK) {

                }
            };
            grid.MouseLeave += (sender, args) => {
                grid.Projection = baseproj;
            };

            CloudList.Items.Add(grid);
        }

        private void PopularClick(object sender, RoutedEventArgs e) {

        }

        private void RecentClick(object sender, RoutedEventArgs e) {

        }

        private void ExploreRefreshClick(object sender, RoutedEventArgs e) {

        }
    }
}