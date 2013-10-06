using System;
using System.Threading.Tasks;
using CloudsdaleLib;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Models;
using Cloudsdale_Metro.Views;
using Cloudsdale_Metro.Views.LoadPages;
using MetroFaye;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace Cloudsdale_Metro.Controllers {
    public class ConnectionController {
        private Frame connectView;

        public Frame MainFrame { get; private set; }

        public readonly SessionController SessionController = new SessionController();
        public readonly ErrorController ErrorController = new ErrorController();
        public readonly MessageController MessageController = new MessageController();
        public readonly ModelController ModelController = new ModelController();

        public ConnectionController() {
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

        public MessageHandler Faye;

        public async Task EnsureAppActivated() {
            MainFrame = Window.Current.Content as Frame;

            if (connectView == null) {
                connectView = new Frame {
                    Content = new Connecting(),
                    Transitions = new TransitionCollection {
                        new EdgeUIThemeTransition()
                    }
                };
            }

            await SessionController.LoadSession();

            if (MainFrame == null) {
                MainFrame = new Frame {
                    Transitions = new TransitionCollection {
                        new EdgeUIThemeTransition { Edge = EdgeTransitionLocation.Right }
                    }
                };
            }

            Window.Current.Content = MainFrame;

            Window.Current.Activate();

            await EnsureFayeConnection();
        }

        public void Navigate(Type pageType) {
            MainFrame.Navigate(pageType);
        }

        public async Task EnsureFayeConnection() {
            if (Faye == null || !Faye.IsConnected) {
                Window.Current.Content = connectView;

                Faye = MetroFaye.Faye.CreateClient(new Uri(Endpoints.PushAddress));
                Faye.PrimaryReciever = MessageController;
                Faye.Disconnected += FayeOnDisconnected;

                var error = false;
                try {
                    await Faye.ConnectAsync();
                } catch {
                    error = true;
                }
                if (error) {
                    await EnsureFayeConnection();
                    return;
                }
            }

            Window.Current.Content = MainFrame;
        }

        private async void FayeOnDisconnected() {
            Faye = null;
            await MainFrame.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async delegate {
                await EnsureFayeConnection();
                if (SessionController.CurrentSession == null) return;
                ConnectSession(SessionController.CurrentSession);
                if (MessageController.CurrentCloud == null) return;
                await MessageController.CurrentCloud.EnsureLoaded();
            });
        }

        public void ConnectSession(Session session) {
            Faye.Subscribe("/users/" + session.Id + "/private");

            foreach (var cloud in session.Clouds) {
                Faye.Subscribe("/clouds/" + cloud.Id);
                Faye.Subscribe("/clouds/" + cloud.Id + "/chat/messages");
            }
        }

        public void NavigateHome() {
            Navigate(typeof(Home));
            MessageController.CurrentCloud = null;
        }
    }
}
