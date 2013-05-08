using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Cloudsdale.Information {
    public partial class About : PhoneApplicationPage {
        public About() {
            InitializeComponent();
        }

        private void MeetTheCrewClick(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/Information/MeetTheCrew.xaml", UriKind.Relative));
        }

        private void TnCClick(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/Information/TermsAndConditions.xaml", UriKind.Relative));
        }

        private void PrivacyClick(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/Information/PrivacyPolicy.xaml", UriKind.Relative));
        }
    }
}