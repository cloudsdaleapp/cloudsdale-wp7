using Newtonsoft.Json;

namespace Cloudsdale.Models {

    [JsonObject(MemberSerialization.OptIn)]
    public class CloudsdaleItem {
        [JsonProperty]
        public string id;
    }
}
