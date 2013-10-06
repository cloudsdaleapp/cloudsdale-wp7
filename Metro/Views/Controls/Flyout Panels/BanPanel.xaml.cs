using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using CloudsdaleLib;
using CloudsdaleLib.Models;

namespace Cloudsdale_Metro.Views.Controls.Flyout_Panels {
    public sealed partial class BanPanel {
        private readonly User user;
        private Ban Ban { get; set; }

        public BanPanel(User user) {
            InitializeComponent();
            this.user = user;
            Ban = new Ban(null) {
                EnforcerId = Cloudsdale.SessionProvider.CurrentSession.Id,
                OffenderId = user.Id,
                Due = DateTime.Now.AddHours(1)
            };

            InitializeFlyout();
        }

        protected override string Header {
            get { return "Ban " + user.Name; }
        }

        protected override Uri Image {
            get { return null; }
        }

        protected override bool IsSettings {
            get { return false; }
        }
    }
}
