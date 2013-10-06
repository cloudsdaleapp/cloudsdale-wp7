using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Cloudsdale.lib.Controllers;
using Cloudsdale.lib.connection;
using Cloudsdale.lib.Controllers.CloudSubscriber;
using Cloudsdale.lib.Controllers.MessageController;
using Cloudsdale.lib.Controllers.MessageController.Processors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cloudsdale.lib;
using Cloudsdale.lib.Models;

namespace Cloudsdale
{
    public partial class Main : Form
    {

        public bool SettingsVisible;
        public static Main Instance = new Main();
        public static JObject User;
        public static JToken CurrentCloud;

        public static Regex LinkRegex =
            new Regex(
                @"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool DirectedToHome;
        public bool LoggedIn;

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Email.Text = CloudsdaleSettings.Default.PreviousEmail;
            Password.Text = CloudsdaleSettings.Default.PreviousPassword;
            autologin.Checked = CloudsdaleSettings.Default.AutoLogin;
            LoadAnimation.Visible = false;
            ImageAnimator.Animate(CloudAnimation.Image, null);
            LoggedIn = false;
            this.Width = 900;
            LoginPanel.BringToFront();
            LoginPanel.Dock = DockStyle.Fill;
            if (LoginPanel.Visible == true)
            {
                this.MaximizeBox = false;
            }
            LeaveSettings();

            if (autologin.Checked)
            {
                if (Email.Text != null)
                {
                    button1_Click(Login, EventArgs.Empty);
                }
            }
            CloudSubList.AddCloud("hang_out");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                CloudsdaleSettings.Default.PreviousEmail = Email.Text;
                CloudsdaleSettings.Default.PreviousPassword = Password.Text;
                CloudsdaleSettings.Default.AutoLogin = autologin.Checked;
                CloudsdaleSettings.Default.Save();
                LoadAnimation.Visible = true;
                ImageAnimator.Animate(LoadAnimation.Image, null);
                this.Text = "Logging in...";
                Login.Enabled = false;
                Register.Enabled = false;
                Email.ReadOnly = true;
                Password.ReadOnly = true;
                await LoginRequest();
                this.Text = "Login succesful!";

                Token.SaveToken((string)User["user"]["auth_token"]);

                this.AcceptButton = m_SendMessage;
                this.MaximizeBox = true;
                Connection.MessageReceived += o =>
                                                  {
                                                      if (o["data"] == null) return;
                                                      var cloudId = ((string) o["channel"]).Split('/')[2];
                                                      var source = MessageSource.GetSource(cloudId);
                                                      LoadMessageToSource(source, o["data"], cloudId);
                                                  };
                await Connection.InitializeAsync();
                this.Text = "Loading clouds...";
                Main MainWindow = new Main();
                await PreloadMessages((JArray) User["user"]["clouds"]);
                h_avatar.BackgroundImage = AvatarConverter.LoadImage((string) User["user"]["avatar"]["normal"]);
                h_avatar.BackgroundImageLayout = ImageLayout.Zoom;
                Login.Enabled = true;
                Register.Enabled = true;
                Email.ReadOnly = false;
                Password.ReadOnly = false;
                LoginPanel.Visible = false;
                //UserCheck.Authorize(User["user"]["name"].ToString());
                //Discord's fault.
                MessageGroup.Text = "Welcome back, " + User["user"]["name"] + "!";
                var calDate = User["user"]["member_since"].ToString();
                var newDate = calDate.Replace("+0000", "");
                h_memberSince.Text = "Member since " + newDate;
                SetupUserMenu();

                RedirectHome(true);
                LoggedIn = true;
            }
            catch (CouldNotLoginException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                Login.Enabled = true;
                Register.Enabled = true;
                Email.ReadOnly = false;
                Password.ReadOnly = false;
                this.Text = "Cloudsdale";
            }
            catch (WebException webex)
            {
                System.Windows.MessageBox.Show(webex.Message);
                Login.Enabled = true;
                Register.Enabled = true;
                Email.ReadOnly = false;
                Password.ReadOnly = false;
                this.Text = "Cloudsdale";
            }
            LoadAnimation.Visible = false;
        }

        public async Task LoginRequest()
        {
            var dataObject = new JObject();
            dataObject["email"] = Email.Text;
            dataObject["password"] = Password.Text;
            var data = Encoding.UTF8.GetBytes(dataObject.ToString(Formatting.None));

            var request = WebRequest.CreateHttp(Endpoints.Session);
            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Timeout = 1500;

            using (var requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(data, 0, data.Length);
                await requestStream.FlushAsync();
                requestStream.Close();
            }

            string responseData;
            try
            {
                using (var response = await request.GetResponseAsync())
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) return;
                    using (var responseStreamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        responseData = await responseStreamReader.ReadToEndAsync();
                    }
                }
            }
            catch (WebException ex)
            {
                using (var response = ex.Response)
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) return;
                    using (var responseStreamReader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        responseData = responseStreamReader.ReadToEnd();
                    }
                }
                throw new CouldNotLoginException(responseData);
            }

            var responseObject = JObject.Parse(responseData);
            Main.User = (JObject) responseObject["result"];
            await Connection.InitializeAsync();

        }

        private void LoadMessageToSource(MessageSource source, JToken message, string cloudId)
        {
            message["orgcontent"] = message["content"] =
                                    message["content"].ToString().UnescapeLiteral().RegexReplace(@"[ ]+", " ");
            lock (source)
            {
                JToken lastMsg;
                if (source.Messages.Any()
                    && (string) (lastMsg = source.Messages.Last())["author"]["id"] == (string) message["author"]["id"]
                    && !lastMsg["orgcontent"].ToString().StartsWith("/me")
                    && !message["content"].ToString().StartsWith("/me"))
                {
                    lastMsg["content"] += "\n" + message["content"];
                    lastMsg["drops"] = new JArray(lastMsg["drops"].Concat(message["drops"]));
                    source.Messages.RemoveAt(source.Messages.Count - 1);
                    source.Messages.Add(lastMsg);
                }
                else
                {
                    message["content"] = message["content"]
                        .ToString().RegexReplace("^/me", (string) message["author"]["name"]);
                    source.AddMessage(message);
                }             
                if (LoggedIn)
                {
                    NotificationControl.ShowSimple(cloudId, (string)message["author"]["id"]);
                }
            }
        }

        private async Task PreloadMessages(ICollection<JToken> clouds)
        {
            foreach (var cloud in clouds)
            {
                try
                {
                    this.Text = "Loading messages from " + (string)cloud["name"] + "...";
                    var request =
                        WebRequest.CreateHttp(Endpoints.CloudMessages.Replace("[:id]", cloud["id"].ToString()));
                    request.Accept = "application/json";
                    using (var response = await request.GetResponseAsync())
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null) continue;
                        using (var responseReader = new StreamReader(responseStream))
                        {
                            var responseData = JObject.Parse(await responseReader.ReadToEndAsync());
                            var source = MessageSource.GetSource(cloud);
                            foreach (var message in responseData["result"])
                            {
                                LoadMessageToSource(source, message, (string)cloud["id"]);

                            }
                            FayeConnector.Subscribe("/clouds/" + cloud["id"] + "/chat/messages");


                            CloudList.SmallImageList.Images.Add((string)cloud["id"],
                                                                LoadImage((string)cloud["avatar"]["normal"]));
                            CloudList.Items.Add((string)cloud["id"], cloud["name"].ToString().TrimStart(),
                                                CloudList.SmallImageList.Images.Count - 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private Image LoadImage(string url)
        {
            WebRequest request =
                WebRequest.Create(url);

            WebResponse response = request.GetResponse();
            Stream responseStream =
                response.GetResponseStream();

            Bitmap bmp = new Bitmap(responseStream);

            responseStream.Dispose();

            return bmp;
        }

        private void LaunchReg(object sender, EventArgs e)
        {
            Process.Start("https://www.cloudsdale.org/register");
        }

        #region Visual changes

        private void SetupUserMenu()
        {
            sp_user_avatar.Image = AvatarConverter.LoadImage((string) User["user"]["avatar"]["normal"]);

            sp_user_name.Text = (string) User["user"]["name"];

            sp_user_username.Text = (string) User["user"]["username"];

            sp_user_akaList.Text = User["user"]["also_known_as"].ToString().MultiReplace("[", "]", Environment.NewLine,
                                                                                         "").Trim();

            sp_user_skype.Text = UserModel.SkypeUsername(User["user"]["id"].ToString());

            sp_user_cloudCount.Text = CloudList.Items.Count.ToString();

            sp_user_joinDate.Text = User["user"]["member_since"].ToString().Replace("+0000", "");

            sp_user_triesLeft.Text = UserModel.NameChangesAllowed().ToString();

            if (sp_user_triesLeft.Text == "0")
            {
                sp_user_username.ReadOnly = true;
            }

            RoleModel.CreateRole(sp_user_role, (string) User["user"]["id"]);
            sp_user_role.BackColor = RoleModel.Role_Color(UserModel.Role((string) User["user"]["id"]));

            sp_user_email.Text = (string) User["user"]["email"];
        }

        private void ActivateMenuHover(object sender, EventArgs e)
        {
            if (SettingsPanel.Height != 55)
            {
                ViewTimer.Start();
            }
            else
            {
                SettingsPanel.Height = 0;
                sp_user.Visible = false;
            }
        }

        private void SettingsHover(object sender, EventArgs e)
        {
            SettingsVisible = true;
            SettingsPanel.Height = 55;
        }

        private void ShowSettings(object sender, EventArgs e)
        {
            var SettingsNewSize = SettingsPanel.Height;
            if (SettingsNewSize >= 55)
            {
                SettingsPanel.Height = 55;
                ViewTimer.Stop();
            }
            else
            {
                SettingsNewSize += 1*((SettingsNewSize + 2)/2);
                SettingsPanel.Height = SettingsNewSize;
            }
        }

        private void LeaveSettings()
        {
            SettingsPanel.Height = 0;
            sp_user.Visible = false;
            sp_settings.Visible = false;
        }
        private void CloudList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LeaveSettings();
            if (CloudList.SelectedItems.Count > 0)
            {
                if (CloudList.FocusedItem.Index >= 0)
                {
                    RedirectHome(false);

                    MessageGroup.Text = CloudList.FocusedItem.Text;
                    this.Text = CloudList.FocusedItem.Text;
                    CurrentCloud = CloudList.FocusedItem.Name;
                    if (CloudList.FocusedItem.Index == 1)
                    {
                        //messages = visible
                    }
                    else
                    {
                        //messages = not visible
                    }

                }
                else
                {
                    RedirectHome(true);
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageGroup.Text = "Welcome back, [:user]!".Replace("[:user]", (string) User["user"]["name"]);
            RedirectHome(true);
        }

        private void RedirectHome(bool RedirectedHome)
        {
            if (RedirectedHome == true)
            {
                //add the component here to release all MessageViews
                h_panel.Visible = true;

                m_NewMessage.Visible = false;
                m_SendMessage.Visible = false;

                this.Text = "Home";
            }
            else
            {
                h_panel.Visible = false;

                m_NewMessage.Visible = true;
                m_SendMessage.Visible = true;
            }
        }

        private void ResizeCheck(object sender, EventArgs e)
        {
            if (this.Width < 695)
            {
                h_about.Visible = false;
            }
            else
            {
                h_about.Visible = true;
            }
        }

        #endregion

        private void CloudContext_Opening(object sender, CancelEventArgs e)
        {
            //Add detection if user is the owner of the cloud.
        }

        private void menuItem_user_Click(object sender, EventArgs e)
        {
            if (sp_user.Visible)
            {
                sp_user.Visible = false;
            }else
            {
                sp_user.Visible = true;
                sp_settings.Visible = false;
            }
        }

        private void sp_user_avatar_Click(object sender, EventArgs e)
        {
            var browser = new OpenFileDialog();
            browser.Filter = "png image (*.png)|*.png| jpg image (*.jpg|*.jpg";
            browser.Title = "Upload new avatar.";
            browser.InitialDirectory = Environment.SpecialFolder.CommonPictures.ToString();
            browser.ShowDialog();
            if (DialogResult != DialogResult.Cancel)
            {
                //Stream selectedImage = new FileStream(browser.FileName, FileMode.Open);
                //UserModel.UploadNewAvatar(selectedImage, "test");
                //selectedImage.Dispose();
            }
        }

        private void menuItem_Logout_Click(object sender, EventArgs e)
        {
            this.Controls.Clear();
            InitializeComponent();

            Email.Text = CloudsdaleSettings.Default.PreviousEmail;
            Password.Text = CloudsdaleSettings.Default.PreviousPassword;
            autologin.Checked = CloudsdaleSettings.Default.AutoLogin;
            LoadAnimation.Visible = false;
            ImageAnimator.Animate(CloudAnimation.Image, null);
            LoggedIn = false;
            this.Width = 695;
            LoginPanel.BringToFront();
            LoginPanel.Dock = DockStyle.Fill;
            if (LoginPanel.Visible == true)
            {
                this.MaximizeBox = false;
            }
            SettingsPanel.Height = 0;
        }

        private void menuItem_Settings_Click(object sender, EventArgs e)
        {
            if (sp_settings.Visible)
            {
                sp_settings.Visible = false;
            }
            else
            {
                sp_settings.Visible = true;
                sp_user.Visible = false;
            }
        }
    }
}

