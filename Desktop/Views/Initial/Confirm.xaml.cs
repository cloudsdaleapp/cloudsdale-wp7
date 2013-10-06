using System.Windows;
using CloudsdaleWin7.lib.Models.Updaters;

namespace CloudsdaleWin7.Views.Initial
{
    /// <summary>
    /// Interaction logic for Confirm.xaml
    /// </summary>
    public partial class Confirm
    {
        public Confirm()
        {
            InitializeComponent();
        }

        private void ButtonClick1(object sender, RoutedEventArgs e)
        {
            Waiting.Visibility = Visibility.Visible;
            UDUModel.UpdateSessionModel("needs_to_confirm_registration", false);
            if (App.Connection.SessionController.CurrentSession.NeedsToConfirmRegistration == false)
            {
                Waiting.Visibility = Visibility.Hidden;
                MessageBox.Show("We're sorry! An error occured when trying to process your request.");
            }else
            {
                if (App.Connection.SessionController.CurrentSession.HasReadTnc == false)
                {
                    App.Connection.MainFrame.Navigate(new TermsAndConditions());
                }
                else
                {
                    App.Connection.MainFrame.Navigate(new Main());
                }
            }
        }
    }
}
