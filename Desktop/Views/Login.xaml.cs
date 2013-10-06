using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CloudsdaleWin7 {
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login
    {
        public static Login Instance;
        public static bool LoggingOut = false;

        public Login()
        {
            Instance = this;
            InitializeComponent();
            EmailBox.Text = App.Settings["email"];
            if (String.IsNullOrEmpty(EmailBox.Text)) EmailBox.Focus();
            if (!String.IsNullOrEmpty(EmailBox.Text) && String.IsNullOrEmpty(PasswordBox.Password)) PasswordBox.Focus();
        }

        private async void LoginAttempt(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (String.IsNullOrEmpty(EmailBox.Text) || String.IsNullOrEmpty(PasswordBox.Password))
            {
                ShowMessage("You can't have empty fields, silly filly. Try again.");
                return;
            }
            ErrorMessage.Text = "";
            LoginUi.Visibility = Visibility.Hidden;
            LoggingInUi.Visibility = Visibility.Visible;
           try
           {
               Thread.Sleep(1000);
               App.Settings.ChangeSetting("email", EmailBox.Text);
               await App.Connection.SessionController.Login(EmailBox.Text, PasswordBox.Password);


           }catch (Exception ex)
           {
               ShowMessage(ex.Message);
               LoginUi.Visibility = Visibility.Visible;
               LoggingInUi.Visibility = Visibility.Hidden;
           }
        }

        public void ShowMessage(string message)
        {
            #region Show Message

            var board = new Storyboard();
            var animation = new DoubleAnimation(0.0, 100.0, new Duration(new TimeSpan(200000000)));
            board.Children.Add(animation);
            Storyboard.SetTargetName(animation, ErrorMessage.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            ErrorMessage.Text = message;
            board.Begin(this);

            #endregion
        }

        private void BeginRegister(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
