namespace Cloudsdale.Models {
    public struct LoginResponse {
        public int status;
        public object[] errors;
        public object flash;
        public Result result;
    }

    public struct Result {
        public string client_id;
        public LoggedInUser user;
    }
}
