using System;
using System.Windows;
using System.Windows.Controls;
using Cloudsdale.Avatars.Mlfw;
using Microsoft.Phone.Controls;

namespace Cloudsdale.Avatars {
    public partial class MLFW {
        private readonly FaceQuery query;

        public MLFW() {
            query = new FaceQuery();
            InitializeComponent();
        }

        private void OrderByChanged(object sender, SelectionChangedEventArgs e) {
            query.OrderBy = (OrderBy)((ListPicker)sender).SelectedIndex;
        }

        private void TagModeChanged(object sender, SelectionChangedEventArgs e) {
            query.TagMode = ((ListPicker)sender).SelectedIndex == 0 ? TagMode.All : TagMode.Any;
        }

        private void TagsTextChanged(object sender, TextChangedEventArgs e) {
            query.Tags = ((TextBox)sender).Text;
        }

        private void SearchClick(object sender, RoutedEventArgs e) {
            ((Button)sender).IsEnabled = false;
            Results.ItemsSource = new MlfwImage[0];
            query.Retrieve(images => {
                ((Button)sender).IsEnabled = true;
                Results.ItemsSource = images;
            }, () => {
                MessageBox.Show("Couldn't load faces! D:");
                ((Button)sender).IsEnabled = true;
            });
        }

        private void FaceClicked(object sender, RoutedEventArgs e) {
            var face = (MlfwImage)((FrameworkElement)sender).DataContext;
            MLFWConfirm.target = face;
            NavigationService.Navigate(new Uri("/Avatars/MLFWConfirm.xaml", UriKind.Relative));
        }
    }
}