using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using Cloudsdale.Managers;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

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
        public bool? hidden { get; set; }

        private Avatar _avatar;

        [JsonProperty]
        public Avatar avatar {
            get { return _avatar; }
            set {
                _avatar = value;
                OnPropertyChanged("avatar");
            }
        }
        [JsonProperty]
        public bool? is_transient;
        [JsonProperty("owner_id")]
        public string Owner;
        [JsonProperty]
        public CloudChatInfo chat;

        public Visibility ShowRules {
            get { return String.IsNullOrWhiteSpace(rules) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public User FullOwner {
            get { return PonyvilleCensus.GetUser(Owner); }
        }

        public IEnumerable<CensusUser> FullMods {
            get { return from uid in Moderators.Distinct() where uid != Owner select PonyvilleCensus.GetUser(uid); }
        }

        public Visibility ShowMods {
            get { return FullMods.Any() ? Visibility.Visible : Visibility.Collapsed; }
        }

        [JsonProperty("moderator_ids")]
        public string[] Moderators;

        public bool IsOwner {
            get { return Owner == Connection.CurrentCloudsdaleUser.id; }
        }

        public Visibility ShowEdit {
            get { return IsOwner ? Visibility.Visible : Visibility.Collapsed; }
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

        public void ChangeProperty(string property, object value, Action<string> callback = null) {
            UpdateCloudValue(id, property, value, callback);
        }

        public void PostMods() {
            UpdateCloudValue(id, "x_moderator_ids", Moderators);
        }

        private static void UpdateCloudValue(string id, string property, object value, Action<string> callback = null) {
            var dataXDocument = new XDocument();
            var root = new XElement("cloud");
            var mods = new XElement(property, value);
            root.AddFirst(mods);
            dataXDocument.AddFirst(root);
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeXNode(dataXDocument));
            var request = WebRequest.CreateHttp("http://www.cloudsdale.org/v1/clouds/" + id);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.Headers["X-Auth-Token"] = Connection.CurrentCloudsdaleUser.auth_token;
            request.Headers["Content-Length"] = data.Length.ToString();
            request.BeginGetRequestStream(result => {
                var requestStream = request.EndGetRequestStream(result);
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                request.BeginGetResponse(responseResult => {
                    try {
                        var response = request.EndGetResponse(responseResult);
                        string responseString;
                        using (var responseStream = response.GetResponseStream())
                        using (var responseReader = new StreamReader(responseStream, Encoding.UTF8)) {
                            responseString = responseReader.ReadToEnd();
                        }
                        response.Close();
                        if (callback != null) {
                            Deployment.Current.Dispatcher.BeginInvoke(() => callback(responseString));
                        }
                    } catch (WebException e) {
                        if (callback != null) {
                            Deployment.Current.Dispatcher.BeginInvoke(() => callback(e.Response.Headers["Status"]));
                        }
                    }
                }, null);
            }, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateCloud(string fayedata) {
            var response = JObject.Parse(fayedata);
            var data = response["data"];
            name = (string)data["name"];

            description = (string)data["description"];

            rules = (string)data["rules"];

            var davatar = data["avatar"];
            avatar = new Avatar {
                Normal = new Uri((string)davatar["normal"]),
                Mini = new Uri((string)davatar["mini"]),
                Thumb = new Uri((string)davatar["thumb"]),
                Preview = new Uri((string)davatar["preview"]),
                Chat = new Uri((string)davatar["chat"]),
            };

            hidden = (bool)data["hidden"];
            OnPropertyChanged("hidden");

            Owner = (string)data["owner_id"];
            OnPropertyChanged("FullOwner");
            OnPropertyChanged("IsOwner");

            var moderator_oids = (JArray)data["moderator_ids"];
            var newmods = new string[moderator_oids.Count];
            for (var i = 0; i < moderator_oids.Count; ++i) {
                newmods[i] = (string)moderator_oids[i];
            }
            Moderators = newmods;
            OnPropertyChanged("IsModerator");
            OnPropertyChanged("FullMods");
            OnPropertyChanged("ShowMods");
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CloudChatInfo {
        [JsonProperty]
        public DateTime? last_message_at;
    }
}
