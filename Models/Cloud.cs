using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Cloudsdale.Managers;
using Newtonsoft.Json;
using System.Linq;

namespace Cloudsdale.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Cloud : CloudsdaleItem, INotifyPropertyChanged {

        private string _name;
        private string _description;
        private string _rules;

        [JsonProperty]
        public string name {
            get { return _name; }
            set {
                _name = value;
                OnPropertyChanged("name");
            }
        }
        [JsonProperty]
        public string description {
            get { return _description; }
            set {
                _description = value;
                OnPropertyChanged("description");
            }
        }
        [JsonProperty]
        public DateTime? created_at;
        [JsonProperty]
        public string rules {
            get { return _rules; }
            set {
                _rules = value;
                OnPropertyChanged("rules");
            }
        }
        [JsonProperty]
        public bool? hidden;
        [JsonProperty]
        public Avatar avatar { get; set; }
        [JsonProperty]
        public bool? is_transient;
        [JsonProperty("owner_id")]
        public string Owner;
        [JsonProperty]
        public UserReference[] moderators;
        [JsonProperty]
        public CloudChatInfo chat;

        public Visibility ShowRules {
            get { return String.IsNullOrWhiteSpace(rules) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public User FullOwner {
            get { return PonyvilleCensus.GetUser(Owner); }
        }

        public IEnumerable<User> FullMods {
            get { return from uid in Moderators.Distinct() where uid != Owner select PonyvilleCensus.GetUser(uid); }
        }

        public Visibility ShowMods {
            get { return FullMods.Any() ? Visibility.Visible : Visibility.Collapsed; }
        }

        [JsonProperty("moderator_ids")]
        public string[] Moderators;

        [JsonProperty("user_ids")]
        public string[] Users;

        public bool IsOwner {
            get { return Owner == Connection.CurrentCloudsdaleUser.id; }
        }

        public bool IsModerator {
            get { return IsOwner || Moderators.Any(mid => mid == Connection.CurrentCloudsdaleUser.id); }
        }

        public void AddModerator(string mid) {
            OnPropertyChanged("FullMods");
            var list = new List<string>(Moderators) { mid };
            Moderators = list.Distinct().ToArray();
            PostMods();
        }

        public void RemoveModerator(string mid) {
            OnPropertyChanged("FullMods");
            var list = new List<string>(Moderators);
            list.Remove(mid);
            Moderators = list.Distinct().ToArray();
            PostMods();
        }

        public void ChangeProperty(string property, object value) {
            UpdateCloudValue(id, property, value);
        }

        public void PostMods() {
            UpdateCloudValue(id, "x_moderator_ids", Moderators);
        }

        public override bool Equals(object obj) {
            if (obj is Cloud) {
                return (obj as Cloud).id == id;
            }
            return Equals(this, obj);
        }

        public override int GetHashCode() {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            return id.GetHashCode();
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        public static bool operator ==(Cloud x, Cloud y) {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null)) {
                return true;
            }
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) {
                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(Cloud x, Cloud y) {
            return !(x == y);
        }

        private static void UpdateCloudValue(string id, string property, object value) {
            var dataXDocument = new XDocument();
            var root = new XElement("cloud");
            var mods = new XElement(property, value);
            root.AddFirst(mods);
            dataXDocument.AddFirst(root);
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeXNode(dataXDocument));
            var request = WebRequest.CreateHttp("http://cloudsdale.org/v1/clouds/" + id + ".json");
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Headers["X-Auth-Token"] = Connection.CurrentCloudsdaleUser.auth_token;
            request.Headers["Content-Length"] = data.Length.ToString();
            request.BeginGetRequestStream(result => {
                using (var requestStream = request.EndGetRequestStream(result)) {
                    requestStream.Write(data, 0, data.Length);
                }
                request.BeginGetResponse(responseResult => {
                    var response = request.EndGetResponse(responseResult);
                    response.Close();
                }, null);
            }, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CloudChatInfo {
        [JsonProperty]
        public DateTime? last_message_at;
    }
}
