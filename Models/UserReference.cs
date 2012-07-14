using System.Runtime.Serialization;

namespace Cloudsdale.Models {
    [DataContract]
    public class UserReference : CloudsdaleItem {

        public struct GetUserResult {
            public User result;
        }
    }
}
