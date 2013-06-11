using System;
#if DEBUG
using System.Diagnostics;
#endif
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using BugSense;
using Cloudsdale.Managers;
using Cloudsdale.Settings;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Res = Cloudsdale.Resources;

namespace Cloudsdale {
    public partial class App {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        public static Color[] ThemeColors = {
            default(Color),
            Color.FromArgb(0xFF, 0x1A, 0x91, 0xDB),
            Color.FromArgb(0xFF, 0x00, 0x55, 0x80),
            Color.FromArgb(0xFF, 0x3A, 0x3A, 0x3A),
        };
        public static string DeviceId;

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App() {
            Thread.CurrentThread.Name = "Main Thead";

            BugSenseHandler.Instance.Init(this, Res.BugsenseApiKey);
            BugSenseHandler.Instance.UnhandledException += ApplicationUnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            object devIdObj;
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out devIdObj)) {
                var id = (byte[])devIdObj;
                DeviceId = id.Aggregate(new StringBuilder(), (builder, b) => builder.Append(b.ToString("x2"))).ToString();
            }

            var psettings = IsolatedStorageSettings.ApplicationSettings;
            FontFamily font;
            try {
                if (psettings.Contains("chatfont")) {
                    font = new FontFamily((string)psettings["chatfont"]);
                } else {
                    font = new FontFamily("Verdana");
                    psettings["chatfont"] = "Verdana";
                    psettings.Save();
                }
            } catch {
                font = new FontFamily("Verdana");
                psettings["chatfont"] = "Verdana";
                psettings.Save();
            }

            ChatFont = font;

            this.ForceDarkTheme();

            ThemeColors[0] = ((SolidColorBrush)Resources["PhoneAccentBrush"]).Color;

            if (psettings.Contains("theme")) {
                var tcolor = (Color)psettings["theme"];
                ((SolidColorBrush)Resources["PhoneChromeBrush"]).Color = tcolor;
            } else {
                var tcolor = Color.FromArgb(0xFF, 0x1A, 0x91, 0xDB);
                ((SolidColorBrush)Resources["PhoneChromeBrush"]).Color = tcolor;
            }

            // Show graphics profiling information while debugging.
#if DEBUG
            // Display the current frame rate counters.
            //Current.Host.Settings.EnableFrameRateCounter = true;

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

        public FontFamily ChatFont {
            get {
                var style = (Style)Resources["ChatStyle"];
                return (FontFamily)((Setter)style.Setters[0]).Value;
            }
            set {
                var style = (Style)Resources["ChatStyle"];
                var setter = (Setter)style.Setters[0];
                if (setter.IsSealed) {
                    style.Setters[0] = new Setter(Control.FontFamilyProperty, value);
                } else {
                    setter.Value = value;
                }
                style = (Style)Resources["RichChatStyle"];
                setter = (Setter)style.Setters[0];
                if (setter.IsSealed) {
                    style.Setters[0] = new Setter(Control.FontFamilyProperty, value);
                } else {
                    setter.Value = value;
                }
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void ApplicationLaunching(object sender, LaunchingEventArgs e) {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void ApplicationActivated(object sender, ActivatedEventArgs e) {
            DerpyHoovesMailCenter.ValidPreloadedData.Clear();

            if (Connection.CurrentCloud != null) {
                if (RootFrame.Content is Clouds) {
                    var cloudPage = RootFrame.Content as Clouds;
                    cloudPage.LoadingPopup.IsOpen = true;
                    DerpyHoovesMailCenter.VerifyCloud(Connection.CurrentCloud.id,
                                                      () => cloudPage.LoadingPopup.IsOpen = false);
                } else {
                    DerpyHoovesMailCenter.VerifyCloud(Connection.CurrentCloud.id);
                }
            }

            try {
                if (Connection.CurrentCloudsdaleUser == null) return;
                Connection.Connect();
            } catch {
                MainPage.reconstruction = true;
                RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void ApplicationDeactivated(object sender, DeactivatedEventArgs e) {
            PonyvilleCensus.Save();

            if (DerpyHoovesMailCenter.PresenceAnnouncer != null)
                DerpyHoovesMailCenter.PresenceAnnouncer.Dispose();
            DerpyHoovesMailCenter.PresenceAnnouncer = null;
            if (Connection.Faye != null)
                Connection.Faye.Disconnect();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void ApplicationClosing(object sender, ClosingEventArgs e) {
            PonyvilleCensus.Save();

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
                PonyvilleCensus.Save();
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
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            RootFrame.UriMapper = new CloudsdaleUriMapper();

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
