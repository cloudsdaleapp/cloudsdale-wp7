using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CloudsdaleWin7.lib.Faye
{
    public static class Connection
    {
        public static event Action<JObject> MessageReceived;

        public static void Initialize()
        {
            FayeConnector.LostConnection += FayeConnector.Connect;
            FayeConnector.DoneConnecting += delegate
            {
                foreach (var cloud in App.Connection.SessionController.CurrentSession.Clouds)
                {
                    FayeConnector.Subscribe("/clouds/" + cloud.Id + "/chat/messages");
                }
            };
            FayeConnector.Connect();
        }

        public static async Task InitializeAsync()
        {
            FayeConnector.LostConnection += FayeConnector.Connect;
            await FayeConnector.ConnectAsync();
        }

        internal static void OnMessageReceived(JObject obj)
        {
            if (MessageReceived != null) MessageReceived(obj);
            App.Connection.MessageController.OnMessage(obj);
        }
    }
}
