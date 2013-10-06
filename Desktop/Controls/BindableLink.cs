using System;
using System.Windows;
using System.Windows.Documents;
using CloudsdaleWin7.lib.CloudsdaleLib;
using CloudsdaleWin7.lib.Helpers;

namespace CloudsdaleWin7.Controls
{
    class BindableLink : Hyperlink
    {
        public string NavigateOnClick { get; set; }

        public BindableLink()
        {
            Click += DoOnClick;
        }
        private void DoOnClick(object sender, RoutedEventArgs e)
        {
            BrowserHelper.FollowLink(NavigateOnClick.AssuredLink());
        }
    }
}
