using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cloudsdale.Models;
using Microsoft.Phone.Tasks;

namespace Cloudsdale {
    public partial class EditCloud {
        public bool AfterInit;

        public EditCloud() {
            DataContext = Connection.CurrentCloud;
            InitializeComponent();
            Dispatcher.BeginInvoke(() => IsHidden.Checked += IsHiddenChecked);
            AfterInit = true;
        }

        private void NameTap(object sender, GestureEventArgs e) {
            if (!ResetTaps()) {
                CloudNameText.Visibility = Visibility.Collapsed;
                CloudNameBox.Visibility = Visibility.Visible;
            }
        }

        private void StackPanelTap(object sender, GestureEventArgs e) {
            ResetTaps();
        }

        private bool inUpload;
        public bool ResetTaps() {
            if (inUpload) return true;
            if (CloudNameBox.Visibility == Visibility.Visible) {
                if (string.IsNullOrWhiteSpace(CloudNameBox.Text)) {
                    CloudNameBox.Text = Connection.CurrentCloud.name;
                    MessageBox.Show("Cloud names cannot be blank");
                    return true;
                }
                inUpload = true;
                CloudNameText.Visibility = Visibility.Visible;
                CloudNameBox.Visibility = Visibility.Collapsed;
                Pullover.IsOpen = true;
                UploadBar.IsEnabled = true;
                var text = CloudNameBox.Text;
                Connection.CurrentCloud.ChangeProperty("name", text, response => {
                    if (response == "422 Unprocessable Entity") {
                        inUpload = false;
                        Pullover.IsOpen = false;
                        UploadBar.IsEnabled = false;
                        CloudNameBox.Text = Connection.CurrentCloud.name;
                        MessageBox.Show("Invalid cloud name (is it already in use?)");
                        return;
                    }

                    inUpload = false;
                    Pullover.IsOpen = false;
                    UploadBar.IsEnabled = false;
                    ResetTaps();
                });
                return true;
            }
            if (RulesBox.Visibility == Visibility.Visible) {
                inUpload = true;
                RulesText.Visibility = Visibility.Visible;
                RulesBox.Visibility = Visibility.Collapsed;
                Pullover.IsOpen = true;
                UploadBar.IsEnabled = true;
                var text = RulesBox.Text;
                Connection.CurrentCloud.ChangeProperty("rules", text, response => {
                    inUpload = false;
                    Pullover.IsOpen = false;
                    UploadBar.IsEnabled = false;
                    ResetTaps();
                });
                return true;
            }
            if (DescriptionBox.Visibility == Visibility.Visible) {
                inUpload = true;
                DescriptionText.Visibility = Visibility.Visible;
                DescriptionBox.Visibility = Visibility.Collapsed;
                Pullover.IsOpen = true;
                UploadBar.IsEnabled = true;
                var text = DescriptionBox.Text;
                Connection.CurrentCloud.ChangeProperty("description", text, response => {
                    inUpload = false;
                    Pullover.IsOpen = false;
                    UploadBar.IsEnabled = false;
                    ResetTaps();
                });
                return true;
            }

            return false;
        }

        private void CloudNameBoxKeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.Enter) return;
            ResetTaps();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            if (ResetTaps()) {
                e.Cancel = true;
            }
        }

        private void RulesTextDoubleTap(object sender, GestureEventArgs e) {
            if (!ResetTaps()) {
                RulesText.Visibility = Visibility.Collapsed;
                RulesBox.Visibility = Visibility.Visible;
            }
        }

        private void DescriptionTextDoubleTap(object sender, GestureEventArgs e) {
            if (!ResetTaps()) {
                DescriptionText.Visibility = Visibility.Collapsed;
                DescriptionBox.Visibility = Visibility.Visible;
            }
        }

        private void IsHiddenChecked(object sender, RoutedEventArgs e) {
            if (!AfterInit) return;
            inUpload = true;
            Pullover.IsOpen = true;
            UploadBar.IsEnabled = true;
            Connection.CurrentCloud.ChangeProperty("hidden", true, response => {
                inUpload = false;
                Pullover.IsOpen = false;
                UploadBar.IsEnabled = false;
            });
        }

        private void IsHiddenUnchecked(object sender, RoutedEventArgs e) {
            if (!AfterInit) return;
            inUpload = true;
            Pullover.IsOpen = true;
            UploadBar.IsEnabled = true;
            Connection.CurrentCloud.ChangeProperty("hidden", false, response => {
                inUpload = false;
                Pullover.IsOpen = false;
                UploadBar.IsEnabled = false;
            });
        }

        private void ChangeAvatarClick(object sender, RoutedEventArgs e) {
            var picChooser = new PhotoChooserTask();
            picChooser.Completed += (o, result) => Connection.CurrentCloud.UploadAvatar(result);
            picChooser.Show();
        }

        private void RemoveModeratorClick(object sender, RoutedEventArgs e) {
            var btn = (Button) sender;
            var user = (User) btn.DataContext;

            Connection.CurrentCloud.RemoveModerator(user.id);
        }
    }
}