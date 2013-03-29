using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;

namespace Cloudsdale.Avatars {
    public partial class ChangeAvatar {
        public static IAvatarUploadable target;
        private static ChangeAvatar instance;
        public static void UpdatedAvatar(Uri newAvatar) {
            instance.Avatar.Source = new BitmapImage(newAvatar);
            instance.LoadingBar.IsIndeterminate = false;
        }

        public static bool mlfw;

        public ChangeAvatar() {
            instance = this;
            InitializeComponent();
            DataContext = target;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            if (mlfw) {
                Avatar.Source = null;
                LoadingBar.IsIndeterminate = true;
            }
        }

        private void ChooseFromPhoneClick(object sender, RoutedEventArgs e) {
            var task = new PhotoChooserTask {
                PixelHeight = 200,
                PixelWidth = 200,
                ShowCamera = true
            };
            task.Completed += PhotoChosen;
            task.Show();
        }

        private void MlfwClick(object sender, RoutedEventArgs e) {
            mlfw = true;
            NavigationService.Navigate(new Uri("/Avatars/MLFW.xaml", UriKind.Relative));
        }

        private void PhotoChosen(object sender, PhotoResult photoResult) {
            if (photoResult.TaskResult != TaskResult.OK) return;

            target.UploadAvatar(photoResult.ChosenPhoto, "image/png", UpdatedAvatar);
            Avatar.Source = null;
            LoadingBar.IsIndeterminate = true;
        }

        private void ImageImageFailed(object sender, ExceptionRoutedEventArgs e) {
            Avatar.Source = new BitmapImage(target.DefaultAvatar);
        }
    }
}