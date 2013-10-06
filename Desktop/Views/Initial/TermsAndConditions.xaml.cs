using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudsdaleWin7.lib;
using CloudsdaleWin7.lib.Helpers;

namespace CloudsdaleWin7.Views.Initial
{
    /// <summary>
    /// Interaction logic for TermsAndConditions.xaml
    /// </summary>
    public partial class TermsAndConditions : Page
    {
        public TermsAndConditions()
        {
            InitializeComponent();
        }

        private async void Accept(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient().AcceptsJson();
            client.DefaultRequestHeaders.Add("X-Auth-Token", App.Connection.SessionController.CurrentSession.Id);
            var response = await
                client.PutAsync(Endpoints.User.ReplaceUserId(App.Connection.SessionController.CurrentSession.Id),
                                new StringContent(""));
            if (response.IsSuccessStatusCode)
            {
                Main.Instance.Frame.Navigate(new Home());
            }

        }
    }
}
