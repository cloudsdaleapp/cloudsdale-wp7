using System;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Common;
using Cloudsdale_Metro.Controllers;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Cloudsdale_Metro.Views.Controls.Flyout_Panels {
    public sealed partial class CloudPanel {
        private readonly LayoutAwarePage.ObservableDictionary<string, object> _defaultViewModel
            = new LayoutAwarePage.ObservableDictionary<string, object>();
        public IObservableMap<string, object> DefaultViewModel { get { return _defaultViewModel; } }
        private readonly CloudController controller;

        public Cloud Cloud { get; set; }

        public CloudPanel(CloudController controller) {
            InitializeComponent();
            this.controller = controller;
            Cloud = this.controller.Cloud;

            InitializeFlyout();
        }

        private void CloudPanel_OnLoaded(object sender, RoutedEventArgs e) {
            DefaultViewModel["Cloud"] = Cloud;
        }

        protected override string Header {
            get { return controller.Cloud.Name; }
        }

        protected override Uri Image {
            get { return null; }
        }

        protected override bool IsSettings {
            get { return false; }
        }
    }
}
