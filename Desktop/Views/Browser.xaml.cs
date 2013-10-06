using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CloudsdaleWin7
{
    /// <summary>
    /// Interaction logic for Browser.xaml
    /// </summary>
    public partial class Browser : Page
    {
        public static Browser Instance;
        public Browser()
        {
            InitializeComponent();
            Instance = this;
            WebBrowser.Navigated += webBrowser1_Navigated;
            Width = MainWindow.Instance.MainFrame.Width;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!WebAddress.Text.StartsWith("http://"))
            {
                WebBrowser.Navigate("http://" + WebAddress.Text);
            }else{ WebBrowser.Navigate(WebAddress.Text);}
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try{WebBrowser.GoBack();}catch{}
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try{WebBrowser.GoForward();}catch{}
        }

        public void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember(
            "Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        }

        void webBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            HideScriptErrors(WebBrowser,
            true);
        }
        public void NavigateTo(string address)
        {
            WebAddress.Text = address;
            WebBrowser.Navigate(address);
        }
    }
}
