namespace Cloudsdale.FayeConnector.ResponseTypes {
    public class HandshakeResponse : Response {
        public bool successful;
        public string version;
        public string[] supportedConnectionTypes;
        public string clientId;
        public ConnectAdvice advice;
    }

    public class ConnectAdvice {
        public string reconnect;
        public int? interval;
        public long? timeout;
    }
}
