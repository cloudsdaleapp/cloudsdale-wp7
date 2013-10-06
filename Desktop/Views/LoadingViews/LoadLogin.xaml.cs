using System;
using System.Windows;

namespace CloudsdaleWin7.Views.LoadingViews
{
    /// <summary>
    /// Interaction logic for LoadLogin.xaml
    /// </summary>
    public partial class LoadLogin
    {
        public LoadLogin()
        {
            InitializeComponent();
            LoadingMovie.Play();
            MainWindow.Instance.MainFrame.Navigate(new Main());
            Main.Instance.Frame.Navigate(new Home());
        }

        private void Loop(object sender, RoutedEventArgs e)
        {
            LoadingMovie.Position = TimeSpan.Zero;
            LoadingMovie.Play();
        }
    }
}
