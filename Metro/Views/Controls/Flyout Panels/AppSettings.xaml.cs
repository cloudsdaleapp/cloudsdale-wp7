using System;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Cloudsdale_Metro.Views.Controls.Flyout_Panels {
    public sealed partial class AppSettings {
        public AppSettings() {
            InitializeComponent();
            InitializeFlyout();
        }

        public Models.AppSettings Settings {
            get { return Models.AppSettings.Settings; }
        }

        protected override string Header {
            get { return "App Settings"; }
        }

        protected override Uri Image {
            get { return null; }
        }

        protected override bool IsSettings {
            get { return true; }
        }
    }
}
