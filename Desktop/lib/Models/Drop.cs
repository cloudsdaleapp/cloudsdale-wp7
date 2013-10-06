using System;
using Newtonsoft.Json;

namespace CloudsdaleWin7.lib.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Drop : CloudsdaleModel
    {
        public readonly string Id;
        private string _title;
        private Uri _url;
        private Uri _preview;

        public Drop(string id)
        {
            Id = id;
        }

        /// <summary>
        /// The title of the resource at the other end of the drop
        /// </summary>
        [JsonProperty("title")]
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// URL to the link posted
        /// </summary>
        [JsonProperty("url")]
        public Uri Url
        {
            get { return _url; }
            set
            {
                if (Equals(value, _url)) return;
                _url = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Preview of the posted webpage
        /// </summary>
        [JsonProperty("preview")]
        public Uri Preview
        {
            get { return _preview; }
            set
            {
                if (Equals(value, _preview)) return;
                _preview = value;
                OnPropertyChanged();
            }
        }
    }
}
