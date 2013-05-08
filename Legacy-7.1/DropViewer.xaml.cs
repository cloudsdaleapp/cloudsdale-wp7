using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Cloudsdale {
    public partial class DropViewer {

        public DropViewer() {
            InitializeComponent();
            browser.IsScriptEnabled = true;
            browser.Navigate(Clouds.LastDropClicked.url);
        }

        private void BackClick(object sender, EventArgs e) {
            try {
                browser.InvokeScript("eval", "history.go(-1)");
            } catch (Exception x) {
#if DEBUG
                Debug.WriteLine(x);
#else
                BugSense.BugSenseHandler.Instance.LogError(x, "Creating a history.go(-1)");
#endif
            }
        }

        private void OpenClick(object sender, EventArgs e) {
            new WebBrowserTask {Uri = Clouds.LastDropClicked.url}.Show();
        }
    }
}