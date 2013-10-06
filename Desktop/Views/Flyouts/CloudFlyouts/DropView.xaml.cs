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
using CloudsdaleWin7.lib.CloudsdaleLib;
using CloudsdaleWin7.lib.Helpers;

namespace CloudsdaleWin7.Views.Flyouts.CloudFlyouts
{
    /// <summary>
    /// Interaction logic for DropView.xaml
    /// </summary>
    public partial class DropView : Page
    {
        public DropView()
        {
            InitializeComponent();
            DropCollection.ItemsSource = App.Connection.MessageController.CurrentCloud.Drops;
        }

        private void Launch(object sender, MouseButtonEventArgs e)
        {
            BrowserHelper.FollowLink(((Image)sender).DataContext.ToString().AssuredLink());
        }

        private void Hide(object sender, RoutedEventArgs e)
        {
            Main.Instance.HideFlyoutMenu();
        }
    }
}
