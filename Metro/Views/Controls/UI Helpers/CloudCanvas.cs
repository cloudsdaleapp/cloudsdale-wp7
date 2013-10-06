using System;
using System.Linq;
using System.Threading.Tasks;
using WinRTXamlToolkit.AwaitableUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace Cloudsdale_Metro.Views.Controls {
    public class CloudCanvas : Canvas, IDisposable {
        private bool isActive = true;

        public CloudCanvas() {
            CloudCount = 10;
            BaseTime = 10;
            RandomTime = 10;
            TopMargin = 50;
        }

        public int CloudCount { get; set; }
        public int BaseTime { get; set; }
        public int RandomTime { get; set; }
        public int TopMargin { get; set; }

        private static readonly Random Random = new Random();
        public async void StartLoop() {
            var cloudCount = CloudCount;

            await this.WaitForNonZeroSizeAsync();

            for (var i = 0; i < cloudCount; ++i) {
                SpawnCloudLoop();
                //await Task.Delay((BaseTime + RandomTime / 2) * 1000 / cloudCount);
            }
        }

        private async void SpawnCloudLoop() {
            await Task.Delay(await CreateCloud(true));

            while (isActive) {
                await Task.Delay(await CreateCloud(false));
            }
        }

        public void Stop() {
            isActive = false;
            foreach (var kvp in Resources.Where(kvp => kvp.Value is Storyboard).ToList()) {
                var animation = (Storyboard)kvp.Value;
                animation.SkipToFill();
                Resources.Remove(kvp);
            }
        }

        public async Task<int> CreateCloud(bool isInitial) {
            var cloud = new Image {
                Source = new BitmapImage(new Uri("ms-appx:/Assets/BackgroundClouds.png")),
                Width = Random.Next(800, 2000),
                Stretch = Stretch.Uniform,
                Visibility = Visibility.Collapsed
            };

            Children.Add(cloud);

            await cloud.WaitForNonZeroSizeAsync();

            var cloudYMax = (int)(ActualHeight - cloud.ActualHeight / 2);
            var cloudY = TopMargin > cloudYMax
                ? TopMargin
                : Random.Next(TopMargin, cloudYMax);

            SetTop(cloud, cloudY);
            SetZIndex(cloud, (int)cloud.Width);

            var time = BaseTime * 1000 + (int)(Random.NextDouble() * RandomTime * 1000);

            var from = (int)(-cloud.Width);
            var to = (int)ActualWidth;

            SetLeft(cloud, from);

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation {
                AutoReverse = false,
                Duration = new Duration(TimeSpan.FromMilliseconds(time)),
                RepeatBehavior = new RepeatBehavior(1),
                From = from,
                To = to,
                EnableDependentAnimation = true,
            };
            animation.Completed += delegate {
                Children.Remove(cloud);
            };
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, cloud);
            Storyboard.SetTargetProperty(animation, "(Canvas.Left)");

            Resources.Add(Guid.NewGuid().ToString(), storyboard);

            storyboard.Begin();

            if (isInitial) {
                storyboard.Seek(TimeSpan.FromMilliseconds(Random.Next(0, time)));
            }

            cloud.Visibility = Visibility.Visible;

            return time;
        }

        public void Dispose() {
            isActive = false;
        }

        ~CloudCanvas() {
            Dispose();
        }
    }
}
