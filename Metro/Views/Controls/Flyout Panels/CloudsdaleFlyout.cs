using System;
using Callisto.Controls;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SettingsFlyout = Callisto.Controls.SettingsFlyout;

namespace Cloudsdale_Metro.Views.Controls.Flyout_Panels {
    public abstract class CloudsdaleFlyout : UserControl {
        private SettingsFlyout flyout;
        protected abstract string Header { get; }
        protected abstract Uri Image { get; }

        protected abstract bool IsSettings { get; }

        protected void InitializeFlyout() {
            flyout = new SettingsFlyout {
                HeaderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x91, 0xDB)),
                HeaderText = Header,
                Background = new SolidColorBrush(Colors.Transparent),
                ContentBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0)),
                FlyoutWidth = SettingsFlyout.SettingsFlyoutWidth.Narrow,
                Content = this
            };

            if (!IsSettings) {
                flyout.BackClicked += (sender, args) => {
                    args.Cancel = true;
                    flyout.IsOpen = false;
                };
            }

            if (Image != null) {
                flyout.SmallLogoImageSource = new BitmapImage(Image);
            }
        }

        public void FlyOut() {
            flyout.IsOpen = true;
        }
    }
}
