using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using CloudsdaleLib;
using Cloudsdale_Metro.Controllers;
using Cloudsdale_Metro.Views;
using Cloudsdale_Metro.Views.Controls;
using Cloudsdale_Metro.Views.LoadPages;
using MetroLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using AppSettings = Cloudsdale_Metro.Views.Controls.Flyout_Panels.AppSettings;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Cloudsdale_Metro {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App {
        public readonly ConnectionController ConnectionController = new ConnectionController();

        public static ConnectionController Connection {
            get { return ((App)Current).ConnectionController; }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            InitializeComponent();
            Suspending += OnSuspending;

            GlobalCrashHandler.Configure();

            RequestedTheme = ApplicationTheme.Light;
        }

        private bool hasRegisteredSettings;

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args) {
            await Models.AppSettings.Load();

            if (!hasRegisteredSettings) {
                hasRegisteredSettings = true;
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
            }

            ModelSettings.Dispatcher = Window.Current.Dispatcher;
            await ConnectionController.EnsureAppActivated();

            TryLaunchSpecial(args.Arguments);

            if (args.PreviousExecutionState != ApplicationExecutionState.Running &&
                args.PreviousExecutionState != ApplicationExecutionState.Suspended) {
                    ConnectionController.Navigate(typeof(LoginPage));
            }
        }

        private void TryLaunchSpecial(string args) {
            try {
                var launchData = JObject.Parse(args);
                if (launchData["type"] != null && (string)launchData["type"] == "toast" &&
                    ConnectionController.SessionController.CurrentSession != null) {
                    var cloudId = (string)launchData["cloudId"];
                    var cloud = Connection.SessionController.CurrentSession.Clouds
                        .FirstOrDefault(scloud => scloud.Id == cloudId);
                    if (cloud == null) {
                        return;
                    }
                    if (ConnectionController.MessageController.CurrentCloud == Connection.MessageController[cloud] &&
                        ConnectionController.MainFrame.Content is CloudPage) {
                        return;
                    }
                    ConnectionController.MessageController.CurrentCloud = Connection.MessageController[cloud];
                    ConnectionController.Navigate(typeof(CloudPage));
                }
            } catch (JsonException) {
            }
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args) {
            var appSettings = new SettingsCommand("AppSettings", "App Settings",
                command => new AppSettings().FlyOut());
            args.Request.ApplicationCommands.Add(appSettings);

            var accountSettings = new SettingsCommand(
                "AccountSettings", "Account settings",
                command => new AccountSettings().FlyOut());

            if (ConnectionController.SessionController.CurrentSession != null && !(ConnectionController.MainFrame.Content is LoggingIn)) {
                args.Request.ApplicationCommands.Add(accountSettings);

                if (ConnectionController.MessageController.CurrentCloud != null) {

                }
            }
        }

        private static void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            ModelSettings.AppLastSuspended = DateTime.Now;
            deferral.Complete();
        }
    }
}
