using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using CloudsdaleWin7.Views;
using CloudsdaleWin7.Views.Flyouts.CloudFlyouts;
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.Controls
{
    /// <summary>
    /// Interaction logic for StandMessageView.xaml
    /// </summary>
    public partial class StandardMessageView : UserControl
    {
        public StandardMessageView()
        {
            InitializeComponent();
        }

        private void Mention(object sender, MouseButtonEventArgs e)
        {
            Main.CurrentView.InputBox.Text = "@" + ((Run) sender).Text + " ";
            Main.CurrentView.InputBox.Focus();
        }

        private void UserInfo(object sender, MouseButtonEventArgs e)
        {
            var user = ((User) ((Run) sender).DataContext);
            if (Main.Instance.FlyoutFrame.Width.Equals(250))
            {
                Main.Instance.FlyoutFrame.Navigate(new UserFlyout(user,
                                                                  App.Connection.MessageController.CurrentCloud.Cloud));
            }
            else
            {
                Main.Instance.ShowFlyoutMenu(new UserFlyout(user, App.Connection.MessageController.CurrentCloud.Cloud));
            }
        }

        private void Quote(object sender, RoutedEventArgs e)
        {
            CloudView.Instance.InputBox.Text = "> " + ((MenuItem) sender).DataContext;
        }
        
    }
}
