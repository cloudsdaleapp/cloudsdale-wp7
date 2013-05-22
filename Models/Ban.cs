using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Cloudsdale.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Cloudsdale.Models {
    public class Ban {
        public string reason { get; set; }
        public DateTime? due;
        public DateTime? created_at;
        public DateTime? updated_at;
        public bool? revoke;
        public string id;
        public string offender_id;
        public string enforcer_id;
        public string jurisdiction_id;
        public string jurisdiction_type;
        public bool? has_expired;
        public bool? is_active;
        public bool? is_transient;

        [JsonIgnore]
        public Cloud Cloud {
            get { return PonyvilleDirectory.GetCloud(jurisdiction_id); }
        }

        [JsonIgnore]
        public User User {
            get { return PonyvilleCensus.GetUser(offender_id); }
        }

        [JsonIgnore]
        public User Issuer {
            get { return PonyvilleCensus.GetUser(enforcer_id); }
        }

        [JsonIgnore]
        public DateTime Issued {
            get { return created_at ?? DateTime.Now; }
        }

        [JsonIgnore]
        public DateTime Due {
            get { return due ?? DateTime.MaxValue; }
        }

        [JsonIgnore]
        public string Status {
            get { return is_active == true ? "Ban active" : revoke == true ? "Revoked" : "Expired"; }
        }

        [JsonIgnore]
        public bool Active {
            get { return is_active == true; }
        }

        public void Revoke(Action complete) {
            var data = Encoding.UTF8.GetBytes(JObject.FromObject(new {
                id,
                cloud_id = jurisdiction_id,
                ban = new { revoke = true }
            }).ToString());
            var request = WebRequest.CreateHttp(new Uri("http://www.cloudsdale.org/v1/clouds/" + jurisdiction_id + "/bans/" + id));
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Headers["Content-Length"] = data.Length.ToString();
            request.BeginGetRequestStream(ar => {
                using (var requestStream = request.EndGetRequestStream(ar)) {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                }

                request.BeginGetResponse(arr => {
                    using (var response = request.EndGetResponse(arr)) {
                        response.Close();
                    }
                    complete();
                }, null);
            }, null);
        }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext) {
            var model = errorContext.OriginalObject;
            var modelType = model.GetType();
            var memberName = errorContext.Member.ToString();
            var field = modelType.GetField(memberName);
            if (field.FieldType == typeof(DateTime?)) {
                field.SetValue(model, DateTime.MaxValue);
            } else if (field.FieldType == typeof (string)) {
                field.SetValue(model, "");
            } else {
                field.SetValue(model, null);
            }
            errorContext.Handled = true;
        }
    }
}
