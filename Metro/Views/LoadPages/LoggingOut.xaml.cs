using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Cloudsdale_Metro.Views.LoadPages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoggingOut {
        public LoggingOut() {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            CloudCanvas.StartLoop();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            CloudCanvas.Stop();
        }
    }
}
