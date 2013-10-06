using System;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Controllers;
using WinRTXamlToolkit.AwaitableUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Cloudsdale_Metro.Views.Controls {
    public sealed partial class UserList {
        private readonly CloudController _controller;
        public UserList(CloudController controller) {
            InitializeComponent();
            _controller = controller;
            InitializeFlyout();
        }

        private async void UserList_OnLoaded(object sender, RoutedEventArgs e) {
            await this.WaitForNonZeroSizeAsync();
            DataContext = _controller;
        }

        private void UserClicked(object sender, ItemClickEventArgs e) {
            new UserPanel((User)e.ClickedItem).FlyOut();
        }

        protected override string Header {
            get { return "Users"; }
        }

        protected override Uri Image {
            get { return _controller.Cloud.Avatar.Preview; }
        }

        protected override bool IsSettings {
            get { return false; }
        }
    }
}
