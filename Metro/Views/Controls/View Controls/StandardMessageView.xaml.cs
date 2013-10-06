using System;
using Windows.UI.Xaml.Documents;
using CloudsdaleLib.Models;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Cloudsdale_Metro.Views.Controls {
    public sealed partial class StandardMessageView {
        public StandardMessageView() {
            InitializeComponent();
        }

        private void StandardMessageView_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            Separator.Width = e.NewSize.Width;

            if (e.NewSize.Width < 600) {
                DropGrid.Visibility = Visibility.Collapsed;
                AltDropGrid.Visibility = Visibility.Visible;

                NormalTitle.Visibility = Visibility.Collapsed;
                TinyTitle.Visibility = Visibility.Visible;

                TagGrid.Width = 40;
            } else {
                DropGrid.Visibility = Visibility.Visible;
                AltDropGrid.Visibility = Visibility.Collapsed;
                DropGrid.MaxWidth = Math.Min(e.NewSize.Width - 320, 450);

                NormalTitle.Visibility = Visibility.Visible;
                TinyTitle.Visibility = Visibility.Collapsed;

                TagGrid.Width = 60;
            }
        }

        private async void DropClicked(object sender, ItemClickEventArgs e) {
            var drop = (Drop)e.ClickedItem;
            await Launcher.LaunchUriAsync(drop.Url);
        }

        private void AvatarTapped(object sender, TappedRoutedEventArgs e) {
            new UserPanel(((Message)DataContext).User).FlyOut();
        }

        private void OnNameClick(Hyperlink sender, HyperlinkClickEventArgs args) {
            new UserPanel(((Message)DataContext).User).FlyOut();
        }
    }
}
