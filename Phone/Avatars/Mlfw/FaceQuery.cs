using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Avatars.Mlfw {
    public class FaceQuery {
        private static readonly Uri BaseUri = new Uri("http://mylittlefacewhen.com/");

        public FaceQuery() {
            OrderBy = OrderBy.Default;
            Limit = 20;
            Offset = 0;
        }

        public OrderBy OrderBy { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }

        public TagMode TagMode { get; set; }
        public string Tags { get; set; }

        public void Retrieve(Action<List<MlfwImage>> callback, Action failed) {
            var parameters = "?format=json&accepted=true&removed=false";
            if (OrderBy != OrderBy.Default) {
                parameters += "&order_by=" + OrderBy.ToString().ToLower();
            }
            if (OrderBy == OrderBy.Random) {
                parameters += "&limit=" + Math.Min(Limit, 5);
            } else {
                parameters += "&limit=" + Limit;
            }
            parameters += "&offset=" + Offset;
            if (!string.IsNullOrWhiteSpace(Tags)) {
                switch (TagMode) {
                    case TagMode.Any:
                        parameters += "&tags__any=" + Uri.EscapeDataString(Tags);
                        break;
                    case TagMode.All:
                        parameters += "&tags__all=" + Uri.EscapeDataString(Tags);
                        break;
                }
            }
            var uri = new Uri(BaseUri, "/api/v3/face/" + parameters);
            DoHttp(uri, (response, fail) => {
                if (fail) {
                    Deployment.Current.Dispatcher.BeginInvoke(failed);
                    //return;
                }
                string data;
                using (response)
                using (var responseStream = response.GetResponseStream())
                using (var responseReader = new StreamReader(responseStream)) {
                    data = responseReader.ReadToEnd();
                }
                var responseData = JObject.Parse(data);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    callback(responseData["objects"].Select(face => face.ToObject<MlfwImage>()).ToList()));
            });
        }

        private void DoHttp(Uri uri, Action<HttpWebResponse, bool> callback) {
            var request = WebRequest.CreateHttp(uri);
            request.Accept = "application/json; charset=utf-8";

            request.BeginGetResponse(responseAsync => {
                try {
                    callback(request.EndGetResponse(responseAsync) as HttpWebResponse, false);
                } catch (WebException ex) {
                    callback(ex.Response as HttpWebResponse, true);
                }
            }, null);
        }
    }

    public enum OrderBy {
        Default,
        Random,
        Views,
        Hotness
    }

    public enum TagMode {
        Any, All
    }
}
