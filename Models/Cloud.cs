﻿using System;
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
using Microsoft.Phone.Tasks;
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
        public string[] Moderators = new string[0];

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
            OnPropertyChanged("IsModerator");
            OnPropertyChanged("FullMods");
            OnPropertyChanged("ShowMods");
            PonyvilleCensus.GetUser(mid).OnPropertyChanged("CloudColor");
            PonyvilleCensus.GetUser(mid).OnPropertyChanged("ModOfCurrent");
        }

        public void RemoveModerator(string mid) {
            OnPropertyChanged("FullMods");
            var list = new List<string>(Moderators);
            list.Remove(mid);
            Moderators = list.Distinct().ToArray();
            PostMods();
            OnPropertyChanged("IsModerator");
            OnPropertyChanged("FullMods");
            OnPropertyChanged("ShowMods");
            PonyvilleCensus.GetUser(mid).OnPropertyChanged("CloudColor");
            PonyvilleCensus.GetUser(mid).OnPropertyChanged("ModOfCurrent");
        }

        public void ChangeProperty(string property, JToken value, Action<string> callback = null) {
            UpdateCloudValue(id, property, value, callback);
        }

        public void PostMods() {
            UpdateCloudValue(id, "x_moderator_ids", JArray.FromObject(Moderators));
        }

        private static void UpdateCloudValue(string id, string property, JToken value, Action<string> callback = null) {
            var root = new JObject();
            root["cloud"] = new JObject();
            root["cloud"][property] = value;
            var data = Encoding.UTF8.GetBytes(root.ToString());
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
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }

        public void UpdateCloud(string fayedata) {
            var response = JObject.Parse(fayedata);
            var data = response["data"];
            if (data["name"] != null) name = (string)data["name"];

            if (data["description"] != null) description = (string)data["description"];

            if (data["rules"] != null) rules = (string)data["rules"];

            if (data["avatar"] != null) {
                var davatar = data["avatar"];
                avatar = new Avatar {
                    Normal = new Uri((string)davatar["normal"]),
                    Mini = new Uri((string)davatar["mini"]),
                    Thumb = new Uri((string)davatar["thumb"]),
                    Preview = new Uri((string)davatar["preview"]),
                    Chat = new Uri((string)davatar["chat"]),
                };
            }

            if (data["hidden"] != null) {
                hidden = (bool)data["hidden"];
                OnPropertyChanged("hidden");
            }

            if (data["owner_id"] != null) {
                Owner = (string)data["owner_id"];
                OnPropertyChanged("FullOwner");
                OnPropertyChanged("IsOwner");
            }

            if (data["moderator_ids"] != null) {
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

        public void CopyFrom(Cloud cloud) {
            if (cloud.Moderators != null) {
                Moderators = cloud.Moderators;
                OnPropertyChanged("IsModerator");
                OnPropertyChanged("FullMods");
                OnPropertyChanged("ShowMods");
            }

            if (cloud.Owner != null) {
                Owner = cloud.Owner;
                OnPropertyChanged("IsOwner");
                OnPropertyChanged("FullOwner");
            }

            if (cloud.avatar != null) {
                avatar = cloud.avatar;
            }

            if (cloud.description != null) {
                description = cloud.description;
            }

            if (cloud.hidden != null) {
                hidden = cloud.hidden;
            }

            if (cloud.name != null) {
                name = cloud.name;
            }

            if (cloud.rules != null) {
                rules = cloud.rules;
            }
        }

        public DerpyHoovesMailCenter Controller {
            get { return DerpyHoovesMailCenter.GetCloud(this); }
        }

        public string TileDescription {
            get { return string.IsNullOrWhiteSpace(description) ? "This cloud has no description" : description; }
        }

        public void UploadAvatar(PhotoResult picture) {
            byte[] data;
            using (var photo = picture.ChosenPhoto)
            using (var ms = new MemoryStream()) {
                photo.CopyTo(ms);
                data = ms.ToArray();
            }
            var boundary = Guid.NewGuid().ToString();
            var request = WebRequest.CreateHttp(new Uri("http://www.cloudsdale.org/v1/clouds/" + id));
            request.Accept = "application/json";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.Headers["X-Auth-Token"] = Connection.CurrentCloudsdaleUser.auth_token;
            request.BeginGetRequestStream(ar => {
                using (var requestStream = request.EndGetRequestStream(ar))
                using (var requestWriter = new StreamWriter(requestStream, Encoding.UTF8)) {
                    requestWriter.WriteLine("--{0}", boundary);
                    requestWriter.WriteLine("Content-Disposition: form-data; name=\"cloud[avatar]\"; filename=\""
                                            + picture.OriginalFileName.Split('\\').Last() + "\"");
                    requestWriter.WriteLine("Content-Type: image/jpeg");
                    requestWriter.WriteLine();
                    requestStream.Write(data, 0, data.Length);
                    requestWriter.WriteLine();
                    requestWriter.WriteLine("--{0}--", boundary);
                }

                request.BeginGetResponse(arr => {
                    using (var response = request.EndGetResponse(arr))
                    using (var responseStream = response.GetResponseStream())
                    using (var responseReader = new StreamReader(responseStream)) {
                        var result = JObject.Parse(responseReader.ReadToEnd());
                        CopyFrom(result["result"].ToObject<Cloud>());
                    }
                }, null);
            }, null);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CloudChatInfo {
        [JsonProperty]
        public DateTime? last_message_at;
    }
}
