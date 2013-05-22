using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Cloudsdale.NFC {
    public partial class Share : PhoneApplicationPage {
        public Share() {
            InitializeComponent();
            DataContext = Connection.CurrentCloud;
        }
    }
}