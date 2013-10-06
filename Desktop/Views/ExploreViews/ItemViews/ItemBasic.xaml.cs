using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.Views.ExploreViews.ItemViews
{
    /// <summary>
    /// Interaction logic for ItemBasic.xaml
    /// </summary>
    public partial class ItemBasic
    {
        private Cloud Cloud { get; set; }

        public ItemBasic(Cloud cloud)
        {
            InitializeComponent();
            BasicAvatar.Source = new BitmapImage(cloud.Avatar.Normal);
            BasicName.Text = cloud.Name;
            Cloud = cloud;
        }

        private void ShowHiddenUi(object sender, MouseEventArgs e)
        {
            var a = new ThicknessAnimation(new Thickness(0,-38,0,39), new Thickness(0,0,0,39), new Duration(new TimeSpan(2000000)));
            a.EasingFunction = new ExponentialEase();
            BackUI.BeginAnimation(MarginProperty, a);
        }

        private void HideHiddenUi(object sender, MouseEventArgs e)
        {
            var a = new ThicknessAnimation(new Thickness(0, 0, 0, 39), new Thickness(0, -38, 0, 39),
                                           new Duration(new TimeSpan(2000000)));
            BackUI.BeginAnimation(MarginProperty, a);
        }

        private void Join(object sender, RoutedEventArgs e)
        {
            BrowserHelper.JoinCloud(Cloud);
        }
    }
}
