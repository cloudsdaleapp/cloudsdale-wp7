using System;
using System.Collections.Generic;
using System.Threading;
using Cloudsdale.FayeConnector.ResponseTypes;
using Newtonsoft.Json;
using WebSocket4Net;

namespace Cloudsdale.FayeConnector {
    public class FayeConnector {
        private readonly string address;
        private WebSocket socket;
        private readonly AutoResetEvent are = new AutoResetEvent(false);
        private string clientId;
        private readonly List<String> subbedchans = new List<string>();

        public event EventHandler<FayeEventArgs> HandshakeComplete;
        public event EventHandler<FayeEventArgs> HandshakeFailed;
        public event EventHandler<SubscribeEventArgs> SubscriptionComplete;
        public event EventHandler<SubscribeEventArgs> SubscriptionFailed;
        public event EventHandler<UnsubscribeEventArgs> UnsubscriptionComplete;
        public event EventHandler<DataReceivedEventArgs> ChannelMessageRecieved; 

        public FayeConnector(string address) {
            this.address = address;
        }

        public void Handshake() {
            new Thread(HandshakeInternal).Start();
        }
        private void HandshakeInternal() {
            if (Connecting) {
                socket.Close();
                socket = null;
            } else if (Connected) {
                Disconnect();
            }

            lock (are) {
                if (socket != null && socket.State == WebSocketState.Open | socket.State == WebSocketState.Connecting) {
                    socket.Close();
                }
                socket = new WebSocket(address);
                socket.Opened += AreSet;
                socket.Open();
                are.WaitOne();
                socket.Opened -= AreSet;

                var handshakeresponse = "";
                EventHandler<MessageReceivedEventArgs> handshakecallback = ((sender, args) => {
                    handshakeresponse = args.Message;
                    are.Set();
                });
                socket.MessageReceived += handshakecallback;
                socket.Send(FayeResources.Handshake);
                are.WaitOne();
                socket.MessageReceived -= handshakecallback;
                var response = JsonConvert.DeserializeObject<HandshakeResponse[]>(handshakeresponse);
                if ((response.Length < 1 || !response[0].successful) && HandshakeFailed != null) {
                    HandshakeFailed(this, new FayeEventArgs(this, handshakeresponse));
                }
                clientId = response[0].clientId;

                var connectdata = FayeResources.Connect.Replace("%CLIENTID%", clientId);
                socket.MessageReceived += (sender, args) => {
                    try {
                        var res = JsonConvert.DeserializeObject<Response[]>(args.Message);
                        if (res.Length < 1) return;
                        if (res[0].channel != "/meta/connect") return;
                        socket.Send(connectdata);
// ReSharper disable EmptyGeneralCatchClause
                    } catch {
// ReSharper restore EmptyGeneralCatchClause
                    }
                };
                socket.Send(connectdata);

                socket.MessageReceived += MessageCallback;

                if (HandshakeComplete != null) {
                    HandshakeComplete(this, new FayeEventArgs(this, handshakeresponse));
                }
            }
        }
        private void AreSet(object o, EventArgs e) {
            are.Set();
        }

        private void MessageCallback(object o, MessageReceivedEventArgs args) {
            var messages = SplitMessage(args.Message);

            foreach (var m in messages)
            try {
                var c = JsonConvert.DeserializeObject<Response>(m).channel;
                ProcessMesssage(m, c);
// ReSharper disable EmptyGeneralCatchClause
            } catch {
// ReSharper restore EmptyGeneralCatchClause
            }
        }

        private static IEnumerable<string> SplitMessage(string message) {
            message = message.Trim().TrimStart('[').TrimEnd(']');
            var split = new List<string>();
            var bracketlevel = 0;
            var lastsplit = -1;
            for (var i = 0; i < message.Length; ++i) {
                switch (message[i]) {
                    case '{':
                        ++bracketlevel;
                        break;
                    case '}':
                        --bracketlevel;
                        break;
                    case ',':
                        if (bracketlevel == 0) {
                            split.Add(message.Substring(lastsplit + 1, i - lastsplit - 1));
                            lastsplit = i;
                        }
                        break;
                }
            }
            split.Add(message.Substring(lastsplit + 1));
            return split;
        }

        private void ProcessMesssage(string data, string channel) {
            switch (channel) {
                case "/meta/subscribe":
                    var subscribedata = JsonConvert.DeserializeObject<SubscribeResponse>(data);
                    if (!subscribedata.successful) {
                        if (SubscriptionFailed != null)
                            SubscriptionFailed(this, new SubscribeEventArgs(this, subscribedata.error, subscribedata.subscription));
                    } else {
                        subbedchans.Add(subscribedata.subscription);
                        if (SubscriptionComplete != null)
                            SubscriptionComplete(this, new SubscribeEventArgs(this, data, subscribedata.subscription));
                    }
                    break;
                case "/meta/unsubscribe":
                    var unsubscribedata = JsonConvert.DeserializeObject<UnsubscribeResponse>(data);
                    while (subbedchans.Contains(unsubscribedata.channel)) {
                        subbedchans.Remove(unsubscribedata.channel);
                    }
                    if (UnsubscriptionComplete != null) {
                        UnsubscriptionComplete(this, new UnsubscribeEventArgs(this, data, unsubscribedata.channel));
                    }
                    break;
                default:
                    if (ChannelMessageRecieved == null) break;
                    if (subbedchans.Contains(channel)) {
                        ChannelMessageRecieved(this, new DataReceivedEventArgs(this, data, channel));
                    }
                    break;
            }
        }

        public void Subscribe(string channel) {
            socket.Send(FayeResources.Subscribe.Replace("%CLIENTID%", clientId).Replace("%CHANNEL%", channel));
        }

        public void Unsubscribe(string channel) {
            socket.Send(FayeResources.Unsubscribe.Replace("%CLIENTID%", clientId).Replace("%CHANNEL%", channel));
        }

        public void Disconnect() {
            socket.Send(FayeResources.Disconnect.Replace("%CLIENTID%", clientId));
            socket.Close();
            socket = null;
        }

        public bool Connected {
            get { return socket != null && socket.State == WebSocketState.Open; }
        }
        public bool Connecting {
            get { return socket != null && socket.State == WebSocketState.Connecting; }
        }
        public bool Closed {
            get { return !Connected && !Connecting; }
        }

        public class FayeEventArgs : EventArgs {

            protected FayeEventArgs() {
            }

            internal FayeEventArgs(FayeConnector connector, string data) {
                Connector = connector;
                Data = data;
            }

            public FayeConnector Connector { get; internal set; }
            public string Data { get; internal set; }
        }

        public class SubscribeEventArgs : FayeEventArgs {

            public string Channel { get; internal set; }

            public SubscribeEventArgs(FayeConnector connector, string data, string channel)
                : base(connector, data) {
                Channel = channel;
            }
        }

        public class UnsubscribeEventArgs : FayeEventArgs {

            public string Channel { get; internal set; }

            public UnsubscribeEventArgs(FayeConnector connector, string data, string channel)
                : base(connector, data) {
                Channel = channel;
            }
        }

        public class DataReceivedEventArgs : FayeEventArgs {
            public string Channel { get; internal set; }

            public DataReceivedEventArgs(FayeConnector connector, string data, string channel)
                : base(connector, data) {
                Channel = channel;
            }
        }
    }
}
