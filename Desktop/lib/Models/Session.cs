using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.CloudsdaleLib;
using Newtonsoft.Json;

namespace CloudsdaleWin7.lib.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    [ResourceEndpoint(Endpoints.Session, UpdateEndpoint = Endpoints.User, RestModelType = "user")]
    public sealed class Session : User
    {
        private string _authToken;
        private string _email;
        private int _nameChangeAllowed;
        private Status _preferredStatus;
        private bool? _needsToConfirmRegistration;
        private bool? _needsPasswordChange;
        private bool? _needsNameChange;
        private bool? _needsEmailChange;
        private List<Cloud> _clouds;
        private List<Ban> _bans;

        [JsonConstructor]
        public Session(string id) : base(id) { }

        /// <summary>
        /// Checks how many times the user
        /// can change their username
        /// </summary>
        [JsonProperty("name_changes_allowed")]
        public int NameChanges
        {
            get { return _nameChangeAllowed; }
            set
            {
                if (value == _nameChangeAllowed) return;
                _nameChangeAllowed = value;
                OnPropertyChanged();
            }
        }
        public bool CanChangeName()
        {
            switch (NameChanges)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    return true;
            }
        }
        /// <summary>
        /// The Authentication Token used to
        /// authenticate this user at API endpoints
        /// </summary>
        [JsonProperty("auth_token")]
        public string AuthToken
        {
            get { return _authToken; }
            set
            {
                if (value == _authToken) return;
                _authToken = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Email address of the user
        /// </summary>
        [JsonProperty("email")]
        public string Email
        {
            get { return _email; }
            set
            {
                if (value == _email) return;
                _email = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The online status of the user
        /// </summary>
        [JsonProperty("preferred_status")]
        public Status PreferredStatus
        {
            get { return _preferredStatus; }
            set
            {
                if (value == _preferredStatus) return;
                _preferredStatus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the user has yet to confirm their registration
        /// </summary>
        [JsonProperty("needs_to_confirm_registration")]
        public bool? NeedsToConfirmRegistration
        {
            get { return _needsToConfirmRegistration; }
            set
            {
                if (value.Equals(_needsToConfirmRegistration)) return;
                _needsToConfirmRegistration = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the user must change their password
        /// </summary>
        [JsonProperty("needs_password_change")]
        public bool? NeedsPasswordChange
        {
            get { return _needsPasswordChange; }
            set
            {
                if (value.Equals(_needsPasswordChange)) return;
                _needsPasswordChange = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the user must change their name
        /// </summary>
        [JsonProperty("needs_name_change")]
        public bool? NeedsNameChange
        {
            get { return _needsNameChange; }
            set
            {
                if (value.Equals(_needsNameChange)) return;
                _needsNameChange = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the user must change their email address
        /// </summary>
        [JsonProperty("NeedsEmailChange")]
        public bool? NeedsEmailChange
        {
            get { return _needsEmailChange; }
            set
            {
                if (value.Equals(_needsEmailChange)) return;
                _needsEmailChange = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A list of all the clouds this user is a member of
        /// </summary>
        [JsonProperty("clouds")]
        public List<Cloud> Clouds
        {
            get { return _clouds; }
            set
            {
                if (Equals(value, _clouds)) return;
                if (value != null)
                    for (var i = 0; i < value.Count; ++i)
                    {
                        value[i] = Cloudsdale.CloudProvider.UpdateCloud(value[i]);
                    }
                _clouds = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A list of active bans on this user
        /// </summary>
        [JsonProperty("bans")]
        public List<Ban> Bans
        {
            get { return _bans; }
            set
            {
                if (Equals(value, _bans)) return;
                _bans = value;
                OnPropertyChanged();
            }
        }

        protected override Newtonsoft.Json.Linq.JToken ObjectFromWebResult(Newtonsoft.Json.Linq.JToken response)
        {
            return base.ObjectFromWebResult(response)["user"];
        }

        protected override async Task<HttpResponseMessage> ValidationRequest(HttpClient client, string requestUrl)
        {
            var requestModel = JsonConvert.SerializeObject(new
            {
                oauth = new
                {
                    token = BCrypt.Net.BCrypt.HashPassword(Id + "cloudsdale", Endpoints.InternalToken),
                    client_type = "desktop",
                    provider = "cloudsdale",
                    uid = Id,
                }
            }, Formatting.None);

            return await client.PostAsync(requestUrl, new JsonContent(requestModel));
        }
    }
}
