using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudsdaleWin7.lib;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.Views.CloudViews
{
    /// <summary>
    /// Interaction logic for OwnedCloud.xaml
    /// </summary>
    public partial class OwnedCloud : Page
    {
        public OwnedCloud(Cloud cloud)
        {
            InitializeComponent();
            Shortlink.Text = "/clouds/" + cloud.ShortName;
            Shortlink.IsReadOnly = cloud.ShortName != cloud.Id;
            CloudAvatar.Source = new BitmapImage(cloud.Avatar.Normal);
            Name.Text = cloud.Name;
            DescriptionBox.Text = cloud.Description.Replace("\n", Environment.NewLine);
            RulesBox.Text = cloud.Rules.Replace("\n", Environment.NewLine);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Main.Instance.HideFlyoutMenu();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
                             {
                                 InitialDirectory = Environment.SpecialFolder.MyPictures.ToString(),
                                 Title = "Upload a new cloud avatar...",
                                 Filter = "Image files |*.png"
                             };
            dialog.ShowDialog();
        }
    }
}
