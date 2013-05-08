using System;
using Newtonsoft.Json;

namespace Cloudsdale.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Avatar {
        [JsonProperty("normal")]
        public virtual Uri Normal { get; set; }
        [JsonProperty("mini")]
        public virtual Uri Mini { get; set; }
        [JsonProperty("thumb")]
        public virtual Uri Thumb { get; set; }
        [JsonProperty("preview")]
        public virtual Uri Preview { get; set; }
        [JsonProperty("chat")]
        public virtual Uri Chat { get; set; }
    }
}
