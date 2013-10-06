using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudsdaleLib;
using CloudsdaleLib.Helpers;
using CloudsdaleLib.Models;
using CloudsdaleLib.Providers;
using Cloudsdale_Metro.Helpers;
using Cloudsdale_Metro.Models;
using Cloudsdale_Metro.Views;
using Cloudsdale_Metro.Views.LoadPages;
using MetroFaye;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using Windows.UI.Popups;

namespace Cloudsdale_Metro.Controllers {
    public class SessionController : ISessionProvider, IMessageReciever {
        private LastSession lastSession;
        private readonly List<Session> pastSessions = new List<Session>();
        private readonly Regex userSessionPattern = new Regex(@"^session_([0-9a-f]+)\.json$", RegexOptions.IgnoreCase);

        public string LastSession {
            get { return lastSession.UserId; }
        }

        public Session CurrentSession { get; private set; }

        public IReadOnlyList<Session> PastSessions {
            get { return new ReadOnlyCollection<Session>(pastSessions); }
        }

        public async Task LoadSession() {
            pastSessions.Clear();

            var storage = ApplicationData.Current.RoamingFolder;
            var sessionFolder = await storage.EnsureFolderExists("session");
            if (!await sessionFolder.FileExists("session.json")) {
                lastSession = new LastSession();
                CurrentSession = null;

                await SaveSession();
                return;
            }

            var lastSessionFile = await sessionFolder.GetFileAsync("session.json");
            lastSession = await JsonConvert.DeserializeObjectAsync<LastSession>(await lastSessionFile.ReadAllText()) ??
                          new LastSession();

            if (lastSession.LastLogins == null) {
                lastSession.LastLogins = new Dictionary<string, DateTime>();
            }

            var sessionFiles = (await sessionFolder.GetFilesAsync()).Where(file => userSessionPattern.IsMatch(file.Name));
            foreach (var sessionFile in sessionFiles) {
                var pastSessionObject =
                    await JsonConvert.DeserializeObjectAsync<Session>(await sessionFile.ReadAllText());
                if (pastSessionObject == null) continue;
                pastSessions.Add(pastSessionObject);
            }

            foreach (var pastSession in pastSessions.Where(pastSession => pastSession != null).Where(pastSession => !lastSession.LastLogins.ContainsKey(pastSession.Id))) {
                lastSession.LastLogins[pastSession.Id] = new DateTime(1970, 1, 1);
            }

            pastSessions.Sort((a, b) => (int)(lastSession.LastLogins[a.Id] - lastSession.LastLogins[b.Id]).Ticks);

            await SaveSession();
        }

        public async Task SaveSession() {
            if (lastSession == null) return;

            var storage = ApplicationData.Current.RoamingFolder;
            var sessionFolder = await storage.EnsureFolderExists("session");

            var lastSessionFile = await sessionFolder.CreateFileAsync("session.json", CreationCollisionOption.ReplaceExisting);
            await lastSessionFile.SaveAllText(await JsonConvert.SerializeObjectAsync(lastSession
#if DEBUG
, Formatting.Indented
#endif
));

            foreach (var pastSession in pastSessions) {
                var sessionFile = await sessionFolder.CreateFileAsync("session_" + pastSession.Id + ".json", CreationCollisionOption.ReplaceExisting);
                await sessionFile.SaveAllText(await JsonConvert.SerializeObjectAsync(pastSession
#if DEBUG
, Formatting.Indented
#endif
));
            }
        }

        public async Task LogIn(LoginForm form) {
            App.Connection.MainFrame.Navigate(typeof(LoggingIn));
            if (form.Session != null) {
                CurrentSession = form.Session;
                await CurrentSession.ForceValidate();
                lastSession.UserId = CurrentSession.Id;

                App.Connection.Faye.ExtensionData["auth_token"] = CurrentSession.AuthToken;
                lastSession.LastLogins[CurrentSession.Id] = DateTime.Now;
                await SaveSession();

                App.Connection.ConnectSession(CurrentSession);

                App.Connection.MainFrame.Navigate(typeof(Home));
                return;
            }

            var requestModel = new {
                email = form.Email,
                password = form.Password
            };
            var request = new HttpClient().AcceptsJson();
            var result = await request.PostAsync(Endpoints.Session, new JsonContent(requestModel));

            var resultString = await result.Content.ReadAsStringAsync();
            var response = await JsonConvert.DeserializeObjectAsync<WebResponse<SessionWrapper>>(resultString);

            if (response.Flash != null) {
                var dialog = new MessageDialog(response.Flash.Message, response.Flash.Title);
                await dialog.ShowAsync();
                App.Connection.MainFrame.Navigate(typeof(LoginPage));
                return;
            }

            CurrentSession = response.Result.User;
            lastSession.UserId = CurrentSession.Id;
            App.Connection.Faye.ExtensionData["auth_token"] = CurrentSession.AuthToken;

            foreach (var pastSession in new List<Session>(pastSessions).Where(pastSession => pastSession.Id == CurrentSession.Id)) {
                pastSessions.Remove(pastSession);
            }

            pastSessions.Add(CurrentSession);
            lastSession.LastLogins[CurrentSession.Id] = DateTime.Now;

            await SaveSession();

            App.Connection.ConnectSession(CurrentSession);

            App.Connection.MainFrame.Navigate(typeof(Home));
        }

        public async Task LogOut() {
            App.Connection.MainFrame.Navigate(typeof(LoggingOut));

            CurrentSession = null;
            lastSession.UserId = null;

            await SaveSession();

            App.Connection.MainFrame.Navigate(typeof(LoginPage));
        }

        public class SessionWrapper {
            public string ClientID { get; set; }
            public Session User { get; set; }
        }

        public void OnMessage(JObject message) {
            var user = message["data"].ToObject<Session>();
            user.CopyTo(CurrentSession);
        }
    }
}
