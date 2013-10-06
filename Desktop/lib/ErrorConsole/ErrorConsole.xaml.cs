using System.Windows;

namespace CloudsdaleWin7.lib.ErrorConsole
{
    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class ErrorConsole : Window
    {
        public static ErrorConsole Instance;
        public ErrorConsole()
        {
            InitializeComponent();
            Instance = this;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ConsoleText.Text);
        }
    }
}
