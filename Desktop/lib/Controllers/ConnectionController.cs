using System;
using System.Windows.Controls;
using CloudsdaleWin7.lib.CloudsdaleLib;
using CloudsdaleWin7.lib.Faye;
using CloudsdaleWin7.lib.Models;
using CloudsdaleWin7.lib.Models.Client;

namespace CloudsdaleWin7.lib.Controllers
{
    public class ConnectionController
    {
        public Frame MainFrame { get; set; }
        public readonly SessionController SessionController = new SessionController();
        public readonly ErrorController ErrorController = new ErrorController();
        public readonly MessageController MessageController = new MessageController();
        public readonly ModelController ModelController = new ModelController();
        public readonly SubscriptionController SubscriptionController = new SubscriptionController();
        public readonly NotificationController NotificationController = new NotificationController();

        public ConnectionController()
        {
            Cloudsdale.SessionProvider = SessionController;
            Cloudsdale.ModelErrorProvider = ErrorController;
            Cloudsdale.CloudServicesProvider = MessageController;
            Cloudsdale.UserProvider = ModelController;
            Cloudsdale.CloudProvider = ModelController;

            Cloudsdale.MetadataProviders["Selected"] = new BooleanMetadataProvider();
            Cloudsdale.MetadataProviders["CloudController"] = new CloudControllerMetadataProvider();
            Cloudsdale.MetadataProviders["Status"] = new UserStatusMetadataProvider();
            Cloudsdale.MetadataProviders["IsOnline"] = new UserOnlineMetadataProvider();
        }

        public void ConnectSession()
        {
            FayeConnector.LostConnection += FayeConnector.Connect;
            FayeConnector.DoneConnecting += delegate
            {
                FayeConnector.Subscribe("/users/" + App.Connection.SessionController.CurrentSession.Id + "/private");
                foreach (var cloud in App.Connection.SessionController.CurrentSession.Clouds)
                {
                    FayeConnector.Subscribe("/clouds/" + cloud.Id + "/chat/messages");
                }
            };
            FayeConnector.Connect();
        }
    }
}
