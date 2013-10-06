using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using CloudsdaleWin7.Views;
using CloudsdaleWin7.Views.Flyouts.CloudFlyouts;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.lib.Helpers
{
    public static class UIHelpers
    {
        public static Hyperlink OnClickLaunch(this Hyperlink link, string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                uri = "http://" + uri;
            }

            link.Click += delegate
            {
                if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                {
                    MessageBox.Show(uri + " is not a well formed link! Please try another.");
                }
                BrowserHelper.FollowLink(uri);
            };
            return link;
        }
        public static void ShowFlyout(this User user, Cloud cloud)
        {
            Main.Instance.FlyoutFrame.Navigate(new UserFlyout(user, cloud));
        }
        public static void MessageOnSkype(string user)
        {
            Process.Start("skype:[:name]?chat".Replace("[:name]", user));
        }
    }
}
