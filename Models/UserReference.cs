using Newtonsoft.Json;

namespace Cloudsdale.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class UserReference : CloudsdaleItem {

        public struct GetUserResult {
            public User result;
        }
    }
}
