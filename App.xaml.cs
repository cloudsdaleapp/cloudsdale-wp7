using System;
#if DEBUG
using System.Diagnostics;
#endif
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using BugSense;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Res = Cloudsdale.Resources;

namespace Cloudsdale {
    public partial class App {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App() {

            BugSenseHandler.Instance.Init(this, Res.BugsenseApiKey);
            BugSenseHandler.Instance.UnhandledException += ApplicationUnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
#if DEBUG
            // Display the current frame rate counters.
            Current.Host.Settings.EnableFrameRateCounter = true;

            // Show the areas of the app that are being redrawn in each frame.
            //Application.Current.Host.Settings.EnableRedrawRegions = true;

            // Enable non-production analysis visualization mode, 
            // which shows areas of a page that are handed off to GPU with a colored overlay.
            //Application.Current.Host.Settings.EnableCacheVisualization = true;

            // Disable the application idle detection by setting the UserIdleDetectionMode property of the
            // application's PhoneApplicationService object to Disabled.
            // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
            // and consume battery power when the user is not using the phone.
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

#endif

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void ApplicationLaunching(object sender, LaunchingEventArgs e) {
            Managers.PonyvilleCensus.Load();
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void ApplicationActivated(object sender, ActivatedEventArgs e) {
            try {
                if (RootFrame.Content is MainPage || RootFrame.Content is FacebookAuth.Login) return;
                Connection.Connect();
            } catch {
                MainPage.reconstruction = true;
                RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void ApplicationDeactivated(object sender, DeactivatedEventArgs e) {

            Managers.PonyvilleCensus.Save();

            if (Managers.DerpyHoovesMailCenter.PresenceAnnouncer != null)
                Managers.DerpyHoovesMailCenter.PresenceAnnouncer.Dispose();
            Managers.DerpyHoovesMailCenter.PresenceAnnouncer = null;
            if (Connection.Faye != null)
                Connection.Faye.Disconnect();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void ApplicationClosing(object sender, ClosingEventArgs e) {
            Managers.PonyvilleCensus.Save();

            if (Connection.Faye != null)
                Connection.Faye.Disconnect();
        }

        // Code to execute if a navigation fails
        private void RootFrameNavigationFailed(object sender, NavigationFailedEventArgs e) {
            if (e.Exception is ApplicationTerminationException) {
                return;
            }
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached) {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
#else
            MainPage.reconstruction = true;
            RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
#endif
        }

        // Code to execute on Unhandled Exceptions
        private static void ApplicationUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            if (e.ExceptionObject is ApplicationTerminationException) {
                throw e.ExceptionObject;
            }
#if DEBUG
            Debug.WriteLine(e.ExceptionObject);
            Debugger.Break();
#endif
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized;

        // Do not add any additional code to this method
        private void InitializePhoneApplication() {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrameNavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e) {
            // Set the root visual to allow the application to render
            // ReSharper disable RedundantCheckBeforeAssignment
            if (RootVisual != RootFrame)
                // ReSharper restore RedundantCheckBeforeAssignment
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}
