using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.Views.CloudViews
{
    /// <summary>
    /// Interaction logic for StandardCloud.xaml
    /// </summary>
    public partial class StandardCloud : Page
    {

        private Cloud Cloud { get; set; }

        public StandardCloud(Cloud cloud)
        {
            InitializeComponent();
            CloudAvatar.Source = new BitmapImage(cloud.Avatar.Normal);
            CloudName.Text = cloud.Name;
            Shortlink.Text = "/clouds/" + cloud.ShortName;
            Cloud = cloud;
            Rules.Text = cloud.Rules.Replace("\n", "\r\n");
            Description.Text = cloud.Description.Replace("\n", "\r\n");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Main.Instance.HideFlyoutMenu();
        }

        private void Leave(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to leave this cloud?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Cloud.Leave();
        }
    }
}
