using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Callisto.Controls;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Helpers;
using Newtonsoft.Json.Linq;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WinRTXamlToolkit.AwaitableUI;
using SettingsFlyout = Callisto.Controls.SettingsFlyout;

namespace Cloudsdale_Metro.Views.Controls {
    public sealed partial class AccountSettings {

        #region Logout

        private async void LogoutClick(object sender, RoutedEventArgs e) {
            ((SettingsFlyout)Parent).IsOpen = false;
            await App.Connection.SessionController.LogOut();
        }

        #endregion

        #region Name

        private async void NameBox_OnLostFocus(object sender, RoutedEventArgs e) {
            var nameBox = sender as TextBox;
            if (nameBox == null) return;
            if (string.IsNullOrWhiteSpace(nameBox.Text)) {
                DataContext = session;
                return;
            }
            await DoUpdate("name", nameBox.Text, nameBox, NameModelError, NameProgress);
        }

        #endregion

        #region Username

        private async void UsernameBox_OnLostFocus(object sender, RoutedEventArgs e) {
            var usernameBox = sender as TextBox;
            if (usernameBox == null) return;
            if (string.IsNullOrWhiteSpace(usernameBox.Text)) {
                DataContext = session;
                return;
            }
            await DoUpdate("username", usernameBox.Text, usernameBox, UsernameModelError, UsernameProgress);
        }

        #endregion

        #region Skype

        private async void SkypeBox_OnLostFocus(object sender, RoutedEventArgs e) {
            var skypeBox = sender as TextBox;
            await DoUpdate("skype_name", skypeBox.Text, skypeBox, SkypeModelError, SkypeProgress);
        }

        #endregion

        #region Avatar

        private async void AvatarBox_OnLostFocus(object sender, RoutedEventArgs e) {
            var avatarBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(avatarBox.Text)) {
                DataContext = session;
                return;
            }
            await DoUpdate("remote_avatar_url", avatarBox.Text, avatarBox, AvatarModelError, AvatarProgress);
            avatarBox.Text = "";
        }

        private async void AvatarTapped(object sender, TappedRoutedEventArgs e) {
            var picker = new FileOpenPicker {
                FileTypeFilter = {
                    ".png", ".jpg", ".jpeg", ".gif", ".bmp"
                },
                CommitButtonText = "Upload Avatar",
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };

            var pic = await picker.PickSingleFileAsync();
            if (pic == null) {
                return;
            }
            new AccountSettings().FlyOut();
            await session.UploadAvatar(await pic.OpenStreamForReadAsync(), "image/png");
        }

        #endregion

        #region Email

        private async void EmailBox_OnLostFocus(object sender, RoutedEventArgs e) {
            var emailBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(emailBox.Text)) {
                DataContext = session;
                return;
            }
            await DoUpdate("email", emailBox.Text, emailBox, EmailModelError, EmailProgress);
        }

        #endregion

        #region Status

        private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var statusBox = sender as ComboBox;
            if (statusBox.SelectedIndex < 0 || statusBox.SelectedIndex > 3) return;
            var newStatus = (Status)statusBox.SelectedIndex;
            if (newStatus == session.PreferredStatus) return;

            await DoUpdate("preferred_status", newStatus.ToString(), statusBox, StatusModelError, StatusProgress);
        }

        #endregion

        #region Password

        private async void PasswordBox_OnLostFocus(object sender, RoutedEventArgs e) {
            var passBox = sender as PasswordBox;
            if (string.IsNullOrWhiteSpace(passBox.Password)) {
                return;
            }
            await DoUpdate("password", passBox.Password, passBox, PasswordModelError, PasswordProgress);
            passBox.Password = "";
        }

        #endregion

        #region Updating

        private async Task DoUpdate(string property, JToken input, Control inputBox, TextBlock errorBlock, UIElement progress) {
            errorBlock.Text = "";
            progress.Visibility = Visibility.Visible;
            inputBox.BorderBrush = new SolidColorBrush(Color.FromArgb(0xA3, 0, 0, 0));

            try {
                var response = await session.UpdateProperty<Session>(true, property.KeyOf(input));
                if (response.Flash != null) {
                    var nameError = response.Errors.FirstOrDefault(error => error.Node == property);
                    SetError(inputBox, errorBlock, nameError != null ? nameError.Message : response.Flash.Message);
                }
            } catch {
                SetError(inputBox, errorBlock, "An error occured");
            }

            progress.Visibility = Visibility.Collapsed;
        }

        private void SetError(Control inputBox, TextBlock errorBlock, string message) {
            errorBlock.Text = message;
            inputBox.BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private async void UIElement_OnKeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key != VirtualKey.Enter) return;
            e.Handled = true;
            var box = (Control)sender;
            box.IsEnabled = false;
            await box.WaitForLayoutUpdateAsync();
            box.IsEnabled = true;
        }

        #endregion

        #region Flyout carp
        private readonly Session session;

        public AccountSettings() {
            InitializeComponent();
            DataContext = session = App.Connection.SessionController.CurrentSession;

            InitializeFlyout();
        }

        protected override string Header {
            get { return "Account Settings"; }
        }

        protected override Uri Image {
            get { return session.Avatar.Preview; }
        }

        protected override bool IsSettings {
            get { return true; }
        }

        #endregion
    }
}
