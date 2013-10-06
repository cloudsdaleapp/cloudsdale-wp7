using System;
using Newtonsoft.Json.Linq;

namespace CloudsdaleWin7.lib.CloudsdaleLib {
    public class CouldNotLoginException : Exception {
        private JToken data;
        public CouldNotLoginException(string responseData) {
            data = JObject.Parse(responseData);
        }

        public override string Message {
            get {
                return "An error occured while trying to log you in! Make sure your password is correct! The error report is: " + data.ToString();
            }
        }
    }
}
