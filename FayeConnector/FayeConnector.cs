using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Cloudsdale.FayeConnector.ResponseTypes;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using WebSocket4Net;

namespace Cloudsdale.FayeConnector {
    public class FayeConnector {
        private const int ConnectTimeout = 30000;
        private readonly string address;
        private WebSocket socket;
        private readonly AutoResetEvent are = new AutoResetEvent(false);
        private string clientId;
        private bool disconnectOrdered;
        public string ClientId {
            get { return clientId; }
        }
        private readonly List<String> subbedchans = new List<string>();

        /// <summary>
        /// Callback for handshake completion
        /// </summary>
        public event EventHandler<FayeEventArgs> HandshakeComplete;
        /// <summary>
        /// Callback for handshake failure
        /// </summary>
        public event EventHandler<FayeEventArgs> HandshakeFailed;
        /// <summary>
        /// Callback for subscription completion
        /// </summary>
        public event EventHandler<SubscribeEventArgs> SubscriptionComplete;
        /// <summary>
        /// Callback for subscription failure
        /// </summary>
        public event EventHandler<SubscribeEventArgs> SubscriptionFailed;
        /// <summary>
        /// Callback for unsubscription completion
        /// </summary>
        public event EventHandler<UnsubscribeEventArgs> UnsubscriptionComplete;
        /// <summary>
        /// Callback for a content received in a subscribed channel
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> ChannelMessageRecieved;

        /// <summary>
        /// Creates a new faye connector
        /// </summary>
        /// <param name="address">The websocket address to connect to</param>
        public FayeConnector(string address) {
            this.address = address;
        }

        public bool IsSubscribed(string channel) {
            if (subbedchans.Contains(channel)) return true;

            var chansplit = channel.Split(
                new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            lock (subbedchans) {
                var temp = subbedchans.Where(s => s.EndsWith("*"));
                foreach (var chan in temp) {
                    var stem = chan.Split(
                        new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (chan.EndsWith("**")) {
                        if (stem.Length < chansplit.Length) continue;
                    } else {
                        if (stem.Length != chansplit.Length) continue;
                    }
                    var matches = true;
                    for (var i = 0; i < stem.Length - 1; ++i) {
                        if (stem[i] == chansplit[i]) continue;
                        matches = false;
                        break;
                    }
                    if (matches) return true;
                }
            }
            Debug.WriteLine("subbed to " + channel + ": " + false);
            return false;
        }

        /// <summary>
        /// Begins the handshaking process
        /// </summary>
        public void Handshake(Action timeoutCallback = null) {
            if (timeoutCallback == null)
                timeoutCallback = DefaultHandshakeCallback;
            new Thread(() => HandshakeInternal(() => Deployment.Current.Dispatcher.BeginInvoke(timeoutCallback))).Start();
        }
        private void HandshakeInternal(Action timeout) {
            disconnectOrdered = false;
            // If there's an existing socket connected or in the middle of connecting... CRUSH ITS SOUL!
            lock (are) {
                if (Connecting) {
                    socket.Close();
                    socket = null;
                } else if (Connected) {
                    Disconnect();
                }
            }

            // Lock! This is my object! MINE I TELL YOU!
            lock (are) {
                // Make a new websocket :3
                socket = new WebSocket(address);
                // Open the socket with hacked-on synchronosity
                socket.Opened += AreSet;
                socket.Open();
                if (!are.WaitOne(ConnectTimeout)) {
                    timeout();
                    return;
                }
                socket.Opened -= AreSet;

                socket.Closed += (sender, args) => {
                    if (disconnectOrdered) return;
                    Connection.Connect();
                };

                // Get a response for the handshack (moar hacked-on synchronosity)
                var handshakeresponse = "";
                EventHandler<MessageReceivedEventArgs> handshakecallback = ((sender, args) => {
                    handshakeresponse = args.Message;
                    are.Set();
                });
                socket.MessageReceived += handshakecallback;
                socket.Send(FayeResources.Handshake.Replace(":auth", Connection.CurrentCloudsdaleUser.auth_token));
                if (!are.WaitOne(ConnectTimeout)) {
                    timeout();
                    return;
                }
                socket.MessageReceived -= handshakecallback;

                // If dat response is a failure... THROW THE CHEEEEESE (and by cheese I mean callback)
                var response = JsonConvert.DeserializeObject<HandshakeResponse[]>(handshakeresponse);
                if ((response.Length < 1 || !response[0].successful) && HandshakeFailed != null) {
                    HandshakeFailed(this, new FayeEventArgs(this, handshakeresponse));
                    return;
                }
                clientId = response[0].clientId;

                // Create the infinite loop of meta connects :3
                var connectdata = FayeResources.Connect.Replace("%CLIENTID%", clientId)
                    .Replace(":auth", Connection.CurrentCloudsdaleUser.auth_token);
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

                // Hook the final callback for the socket and rais the handshake complete :3
                socket.MessageReceived += MessageCallback;

                if (HandshakeComplete != null) {
                    HandshakeComplete(this, new FayeEventArgs(this, handshakeresponse));
                }
            }
        }
        private void AreSet(object o, EventArgs e) {
            are.Set();
        }

        private static void DefaultHandshakeCallback() {
            if (MessageBox.Show("Can't connect to cloudsdale\r\nRetry?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                Connection.Connect();
            } else {
                // ReSharper disable PossibleNullReferenceException
                (Application.Current.RootVisual as PhoneApplicationFrame).
                    Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                // ReSharper restore PossibleNullReferenceException
            }
        }

        // process dat msg
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

        /// <summary>
        /// Split dat array into objects!
        /// </summary>
        /// <param name="message">Possible array object</param>
        /// <returns>the objects :3</returns>
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

        /// <summary>
        /// Process dat content :3
        /// </summary>
        /// <param name="data">Raw object</param>
        /// <param name="channel">channel</param>
        private void ProcessMesssage(string data, string channel) {
            switch (channel) {
                // subscription callback handling
                case "/meta/subscribe":
                    var subscribedata = JsonConvert.DeserializeObject<SubscribeResponse>(data);
                    if (!subscribedata.successful) {
                        if (SubscriptionFailed != null)
                            SubscriptionFailed(this, new SubscribeEventArgs(this, subscribedata.error, subscribedata.subscription));
                    } else {
                        lock (subbedchans)
                            subbedchans.Add(subscribedata.subscription);
                        if (SubscriptionComplete != null)
                            SubscriptionComplete(this, new SubscribeEventArgs(this, data, subscribedata.subscription));
                    }
                    break;
                // unsubscription callback handling
                case "/meta/unsubscribe":
                    var unsubscribedata = JsonConvert.DeserializeObject<UnsubscribeResponse>(data);

                    lock (subbedchans)
                        while (subbedchans.Contains(unsubscribedata.channel)) {
                            subbedchans.Remove(unsubscribedata.channel);
                        }
                    if (UnsubscriptionComplete != null) {
                        UnsubscriptionComplete(this, new UnsubscribeEventArgs(this, data, unsubscribedata.channel));
                    }
                    break;
                // It's something else. If it's one of the subbed channels, CALL IT IN!
                default:
                    if (ChannelMessageRecieved == null) break;
                    ChannelMessageRecieved(this, new DataReceivedEventArgs(this, data, channel));
                    break;
            }
        }

        /// <summary>
        /// Subscribe.... Get's the people going!
        /// </summary>
        /// <param name="channel"></param>
        public void Subscribe(string channel) {
            if (socket == null) return;
            socket.Send(FayeResources.Subscribe.Replace("%CLIENTID%", clientId).Replace("%CHANNEL%", channel)
                .Replace(":auth", Connection.CurrentCloudsdaleUser.auth_token));
        }

        /// <summary>
        /// Unsubscribe. Foine hoe.... ;_;
        /// </summary>
        /// <param name="channel"></param>
        public void Unsubscribe(string channel) {
            socket.Send(FayeResources.Unsubscribe.Replace("%CLIENTID%", clientId).Replace("%CHANNEL%", channel)
                .Replace(":auth", Connection.CurrentCloudsdaleUser.auth_token));
        }

        public void Publish<T>(string channel, T data) {
            try {
                var request = new PublishRequest<T> { channel = channel, data = data, clientId = clientId };
                socket.Send(JsonConvert.SerializeObject(request));
#pragma warning disable 168
            } catch (Exception ex) {
#pragma warning restore 168
#if DEBUG
                Debugger.Break();
#else
                BugSense.BugSenseHandler.Instance.LogError(ex);
#endif
            }
        }

        public void SendRaw(string data) {
            if (socket == null) return;
            socket.Send(data);
        }

        /// <summary>
        /// SERVER GTFO!
        /// </summary>
        public void Disconnect() {
            subbedchans.Clear();
            disconnectOrdered = true;
            if (Closed) return;
            try {
                lock (are) {
                    socket.Send(FayeResources.Disconnect.Replace("%CLIENTID%", clientId));
                    socket = null;
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// It's connected! :::::::::DDDDDDDDDDD
        /// </summary>
        public bool Connected {
            get { return socket != null && socket.State == WebSocketState.Open; }
        }
        /// <summary>
        /// It's doing its connect thingy :o
        /// </summary>
        public bool Connecting {
            get { return socket != null && socket.State == WebSocketState.Connecting; }
        }
        /// <summary>
        /// IT WENT GTFO!
        /// </summary>
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

        ~FayeConnector() {
            Disconnect();
        }
    }
}
