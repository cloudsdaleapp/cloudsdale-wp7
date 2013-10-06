using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Windows.Phone.Speech.Recognition;

namespace Cloudsdale {
    public partial class ZenMode {
        private readonly Cloud cloud;
        private readonly SpeechRecognizer recognizer;
        private bool doneWithZen;
        private Task zenListener;

        public ZenMode() {
            InitializeComponent();
            DataContext = cloud = Connection.CurrentCloud;
            cloud.Controller.Messages.CollectionChanged += ScrollDown;
            recognizer = new SpeechRecognizer();
            doneWithZen = false;
        }

        public async Task SpeechLoop() {
            recognizer.Settings.EndSilenceTimeout = TimeSpan.FromSeconds(0.3);
            recognizer.Settings.InitialSilenceTimeout = TimeSpan.FromSeconds(10);
            while (true) {
                var doWait = false;
                try {

                    LayoutRoot.Background = new SolidColorBrush(Colors.Green);
                    await Task.Delay(50);
                    LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
                    var spresult = await recognizer.RecognizeAsync();
                    if (doneWithZen) return;

                    if (spresult.TextConfidence == SpeechRecognitionConfidence.Rejected) {
                        continue;
                    }

                    var text = GetSpeech(spresult);

                    if (string.IsNullOrWhiteSpace(text)) continue;

                    var controller = cloud.Controller;
                    var cmessages = controller.messages;
                    var message = new Message {
                        id = Guid.NewGuid().ToString(),
                        device = "mobile",
                        content = text,
                        timestamp = DateTime.Now,
                        user = PonyvilleCensus.GetUser(Connection.CurrentCloudsdaleUser.id)
                    };
                    cmessages.AddToEnd(message);

                    Connection.SendMessage(Connection.CurrentCloud.id, text, response => {
                        var result = response["result"];
                        message.id = (string)result["id"];
                        message.drops = result["drops"].Select(jdrop => jdrop.ToObject<Drop>()).ToArray();

                        cmessages.cache.Trigger(controller.IndexOf(message));
                    });
                } catch {
                    doWait = true;
                }
                if (doWait) {
                    await Task.Delay(500);
                }
            }
        }

        static string GetSpeech(SpeechRecognitionResult result) {
            return new Regex(@"\<profanity\>(.*?)\<\/profanity\>").Replace(result.Text, match => match.Groups[1].Value);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            zenListener = SpeechLoop();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e) {
            doneWithZen = true;
            zenListener.AsAsyncAction().Cancel();
            cloud.Controller.Messages.CollectionChanged -= ScrollDown;
            base.OnNavigatedTo(e);
        }

        public void ScrollDown(object sender, EventArgs args) {
            new Thread(() => {
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
            }).Start();
        }

        private void AvatarImageFailed(object sender, ExceptionRoutedEventArgs e) {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("http://assets.cloudsdale.org/assets/fallback/avatar_preview_user.png"));
        }
    }
}