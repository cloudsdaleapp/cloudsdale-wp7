using System.Windows;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Screenshot {
    public partial class UploadResult {
        public static JObject ViewResult;

        public UploadResult() {
            InitializeComponent();
            DataContext = ViewResult;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            NavigationService.RemoveBackEntry();
        }

        private void CopyClick(object sender, RoutedEventArgs e) {
            Clipboard.SetText(ViewResult["payload"]["link"].ToString());
        }

    }
}