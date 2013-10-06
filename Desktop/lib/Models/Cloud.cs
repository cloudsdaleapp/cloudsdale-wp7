using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CloudsdaleWin7.Views;
using CloudsdaleWin7.lib.CloudsdaleLib;
using CloudsdaleWin7.lib.Controllers;
using CloudsdaleWin7.lib.Faye;
using CloudsdaleWin7.lib.Helpers;
using Newtonsoft.Json;

namespace CloudsdaleWin7.lib.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    [ResourceEndpoint(Endpoints.Cloud, RestModelType = "cloud")]
    public sealed class Cloud : CloudsdaleResource, IAvatarUploadTarget
    {
        private string _name;
        private string _shortName;
        private string[] _userIds;
        private string[] _moderatorIds;
        private string _ownerId;
        private Avatar _avatar;
        private bool? _hidden;
        private string _rules;
        private DateTime? _created;
        private string _description;
        private string _unreadMessages;
        public bool IsSubscribed { get; set; }

        /// <summary>
        /// Creates a cloud object based on the given ID
        /// </summary>
        /// <param name="id">The permanent ID to be associated with this object</param>
        [JsonConstructor]
        public Cloud(string id) : base(id) { }

        public Cloud RawCloud
        {
            get { return this; }
        }

        public CloudController RawController
        {
            get { return App.Connection.MessageController[this]; }
        }

        /// <summary>
        /// The name of the cloud
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The shortname of the cloud
        /// </summary>
        [JsonProperty("short_name")]
        public string ShortName
        {
            get { return _shortName; }
            set
            {
                if (value == _shortName) return;
                _shortName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Description of the cloud
        /// </summary>
        [JsonProperty("description")]
        public string Description
        {
            get { return _description; }
            set
            {
                if (value == _description) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// When the cloud was created
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime? Created
        {
            get { return _created; }
            set
            {
                if (value.Equals(_created)) return;
                _created = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Rules for the cloud
        /// </summary>
        [JsonProperty("rules")]
        public string Rules
        {
            get { return _rules; }
            set
            {
                if (value == _rules) return;
                _rules = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the cloud is private
        /// </summary>
        [JsonProperty("hidden")]
        public bool? Hidden
        {
            get { return _hidden; }
            set
            {
                if (value.Equals(_hidden)) return;
                _hidden = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Avatar for the cloud
        /// </summary>
        [JsonProperty("avatar")]
        public Avatar Avatar
        {
            get
            {
                if (_avatar != null)
                {
                    _avatar.Owner = this;
                }
                return _avatar;
            }
            set
            {
                if (Equals(value, _avatar)) return;
                _avatar = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ID for the owner of the cloud
        /// </summary>
        [JsonProperty("owner_id")]
        public string OwnerId
        {
            get { return _ownerId; }
            set
            {
                if (value == _ownerId) return;
                _ownerId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// IDs for the moderators of the cloud
        /// </summary>
        [JsonProperty("moderator_ids")]
        public string[] ModeratorIds
        {
            get { return _moderatorIds; }
            set
            {
                if (Equals(value, _moderatorIds)) return;
                _moderatorIds = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// IDs for all the members of the cloud
        /// </summary>
        [JsonProperty("user_ids")]
        public string[] UserIds
        {
            get { return _userIds; }
            set
            {
                if (Equals(value, _userIds)) return;
                _userIds = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Uploads a new avatar for the cloud
        /// </summary>
        /// <param name="pictureStream">Stream for the new cloud avatar</param>
        /// <param name="mimeType">Mime type of the picture being uploaded</param>
        /// <returns></returns>
        public async Task UploadAvatar(Stream pictureStream, string mimeType)
        {
            HttpContent postData;
            using (var dataStream = new MemoryStream())
            {
                using (pictureStream)
                {
                    await pictureStream.CopyToAsync(dataStream);
                }

                postData = new MultipartFormDataContent("--" + Guid.NewGuid() + "--") {
                    new ByteArrayContent(dataStream.ToArray()) {
                        Headers = {
                            ContentDisposition = new ContentDispositionHeaderValue("form-data") {
                                Name = "cloud[avatar]",
                                FileName = "GenericImage.png"
                            },
                            ContentLength = dataStream.Length,
                        }
                    }
                };
            }

            var request = new HttpClient
            {
                DefaultRequestHeaders = {
                    { "Accept", "application/json" },
                    { "X-Auth-Token", Cloudsdale.SessionProvider.CurrentSession.AuthToken } 
                }
            };

            var response = await request.PostAsync(Endpoints.Cloud.Replace("[:id]", Id), postData);
            var result = JsonConvert.DeserializeObject<WebResponse<Cloud>>(await response.Content.ReadAsStringAsync());

            if (response.StatusCode != HttpStatusCode.OK)
            {
                await Cloudsdale.ModelErrorProvider.OnError(result);
            }
            else
            {
                result.Result.CopyTo(this);
            }
        }
        public async void Leave()
        {
            FayeConnector.Unsubscribe("/clouds/" + Id + "/chat/messages");
            FayeConnector.Unsubscribe("clouds/" + Id + "users/**");

            var client = new HttpClient
            {
                DefaultRequestHeaders =
                {
                    {"Accept", "application/json"},
                    {"X-Auth-Token", App.Connection.SessionController.CurrentSession.AuthToken}
                }
            };
            await client.DeleteAsync(Endpoints.CloudUserRestate.Replace("[:id]", Id).ReplaceUserId(App.Connection.SessionController.CurrentSession.Id));
            App.Connection.SessionController.CurrentSession.Clouds.Remove(this);
            App.Connection.SessionController.RefreshClouds();
            Main.Instance.Clouds.SelectedIndex = -1;
            Main.Instance.HideFlyoutMenu();
        }
        public override string ToString()
        {
            return Name + "[" + Id + "]";
        }
    }
}
