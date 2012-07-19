using System;
using Newtonsoft.Json;

namespace Cloudsdale.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Avatar {
        [JsonProperty]
        public virtual Uri Normal { get; set; }
        [JsonProperty]
        public virtual Uri Mini { get; set; }
        [JsonProperty]
        public virtual Uri Thumb { get; set; }
        [JsonProperty]
        public virtual Uri Preview { get; set; }
        [JsonProperty]
        public virtual Uri Chat { get; set; }
    }
}
