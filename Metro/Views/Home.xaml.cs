using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Controllers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Cloudsdale_Metro.Views {
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class Home : Cloudsdale_Metro.Common.LayoutAwarePage {
        public Home() {
            InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState) {
            DefaultViewModel["Items"] = App.Connection.SessionController.CurrentSession.Clouds;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            CloudCanvas.StartLoop();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CloudCanvas.Stop();
            base.OnNavigatedFrom(e);
        }

        private void CloudItemClicked(object sender, ItemClickEventArgs e) {
            var cloud = (Cloud)e.ClickedItem;
            var controller = (CloudController)cloud.UIMetadata["CloudController"].Value;

            App.Connection.MessageController.CurrentCloud = controller;
            App.Connection.MainFrame.Navigate(typeof(CloudPage));
        }
    }
}
