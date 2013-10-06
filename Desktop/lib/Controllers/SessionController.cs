using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CloudsdaleWin7.Views.Initial;
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.Models;
using CloudsdaleWin7.lib.Providers;
using CloudsdaleWin7.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudsdaleWin7.lib.Controllers
{
    public class SessionController : ISessionProvider
    {

        public Session CurrentSession { get; set; }

        public void OnMessage(JObject message)
        {
            var user = message["data"].ToObject<Session>();
            user.CopyTo(CurrentSession);
        }
        public async Task Login(string email, string password)
        {

            var requestModel = @"{""email"":""[:email]"", ""password"":""[:password]""".Replace("[:email]", email).Replace("[:password]", password);
            var request = new HttpClient().AcceptsJson();
            var result = await request.PostAsync(Endpoints.Session, new JsonContent(requestModel));
            var resultString = await result.Content.ReadAsStringAsync();
            var response = await JsonConvert.DeserializeObjectAsync<WebResponse<SessionWrapper>>(resultString);
            try
            {
                CurrentSession = response.Result.User;
                RegistrationCheck();
            }
            catch
            {
                CloudsdaleWin7.Login.Instance.LoggingInUi.Visibility = Visibility.Hidden;
                CloudsdaleWin7.Login.Instance.LoginUi.Visibility = Visibility.Visible;
                CloudsdaleWin7.Login.Instance.ShowMessage(response.Flash.Message);
            }
        }

        public void Logout()
        {
            MainWindow.Instance.MainFrame.Navigate(new Login());
            CurrentSession = null;
        }

        public async void PostData(string property, string key)
        {
            var data = JObject.FromObject(new
            {
                user = new {property = key}
            }).ToString();
            var client = new HttpClient().AcceptsJson();
            client.DefaultRequestHeaders.Add("X-Auth-Token", CurrentSession.AuthToken);
            await client.PutAsync(Endpoints.User.ReplaceUserId(CurrentSession.Id), new StringContent(data));
        }

        public void RefreshClouds()
        {
            Main.Instance.Clouds.ItemsSource = null;
            Main.Instance.Clouds.ItemsSource = CurrentSession.Clouds;
        }

        public class SessionWrapper
        {
            public string ClientId { get; set; }
            public Session User { get; set; }
        }

        private void InitializeClouds()
        {
            foreach (var cloud in CurrentSession.Clouds)
            {

                App.Connection.MessageController.CloudControllers.Add(cloud.Id, new CloudController(cloud));
                App.Connection.MessageController[cloud].EnsureLoaded();
            }
        }

        private void RegistrationCheck()
        {
            if (CurrentSession.NeedsToConfirmRegistration == true)
            {
                MainWindow.Instance.MainFrame.Navigate(new Confirm());
            }
            else
            {
                MainWindow.Instance.MainFrame.Navigate(new Main());
                InitializeClouds();
            }

        }
    }
}
