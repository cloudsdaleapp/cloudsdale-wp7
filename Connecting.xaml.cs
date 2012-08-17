using Microsoft.Phone.Controls;

namespace Cloudsdale {
    public partial class Connecting : PhoneApplicationPage {
        public Connecting() {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            if (Home.comingfromhome) {
                NavigationService.GoBack();
                Connection.Faye.Disconnect();
                Home.comingfromhome = false;
            }
            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
        }
    }
}