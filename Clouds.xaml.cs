using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Cloudsdale.Controls;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Cloudsdale.Screenshot;
using Cloudsdale.Settings;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Networking.Proximity;
using Windows.Phone.Speech.Recognition;
using Windows.System;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using Res = Cloudsdale.Resources;
using System.Linq;

// http://i.qkme.me/3597jb.jpg GO TO BED

namespace Cloudsdale {
    public partial class Clouds {
        public static bool Wasoncloud;
        public DerpyHoovesMailCenter Controller { get; set; }
        public bool Leaving;
        private bool inUserPopup;
        private Cloud datcloud;

        public static readonly Regex CloudsdaleUrl = new Regex("http\\:\\/\\/(www\\.)?cloudsdale\\.org\\/(\\#)?clouds\\/([a-zA-Z0-9]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool DoARemove;

        public Clouds() {
            if (Connection.CurrentCloud == null) {
                Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)));
                return;
            }
            Controller = DerpyHoovesMailCenter.GetCloud(Connection.CurrentCloud);
            InitializeComponent();

            if (Connection.CurrentCloudsdaleUser.Clouds.All(cloud => cloud.id != Connection.CurrentCloud.id)) {
                Connection.JoinCloud(Connection.CurrentCloud.id);
                var list = new List<Cloud>(Connection.CurrentCloudsdaleUser.clouds) { Connection.CurrentCloud };
                list.Sort(PonyvilleDirectory.GetUserCloudListComparer());
                Connection.CurrentCloudsdaleUser.clouds = list.ToArray();
            }

            cloudPivot.Title = Connection.CurrentCloud.name;

            new Thread(() => {
                Thread.Sleep(75);
                Dispatcher.BeginInvoke(() => {
                    Chats.ItemsSource = Controller.Messages;
                    MediaList.ItemsSource = Controller.Drops;
                    Ponies.DataContext = Controller.Users;
                    ScrollDown(null, null);
                });
            }).Start();

            Wasoncloud = true;

            ApplicationBar.ForegroundColor = Colors.White;
            ApplicationBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]).Color;
        }

        public void ScrollDown(object sender, EventArgs args) {
            new Thread(() => {
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
                Thread.Sleep(100);
                Dispatcher.BeginInvoke(() => ChatScroller.ScrollToVerticalOffset(double.PositiveInfinity));
            }).Start();
        }

        private bool inanim;
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e) {
            if (inanim) return;
            if (userpopup.IsOpen) {
                userpopup.IsOpen = false;
                inUserPopup = false;
                e.Cancel = true;
                return;
            }
            if (CloudInfoPopup.IsOpen) {
                e.Cancel = true;
                CloudInfoDown.Stop();
                inanim = true;
                CloudInfoUp.Begin();
                new Thread(() => {
                    Thread.Sleep(1000);
                    inanim = false;
                    Dispatcher.BeginInvoke(() => {
                        CloudInfoPopup.IsOpen = false;
                        cloudinfoback.Visibility = Visibility.Collapsed;
                    });
                }).Start();
            }
            base.OnBackKeyPress(e);
        }

        private void PhoneApplicationPageOrientationChanged(object sender, OrientationChangedEventArgs e) {
            if (e.Orientation == PageOrientation.PortraitUp) {
                LayoutRoot.Background = (Brush)Application.Current.Resources["PortraitBackgroundChat"];
                SystemTray.IsVisible = true;
                ScrollDown(null, null);
            } else {
                LayoutRoot.Background = (Brush)Application.Current.Resources["LandscapeBackground"];
                SystemTray.IsVisible = false;
                ScrollDown(null, null);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            //if (ProximityDevice.GetDefault() != null) {
            //    var menuItems = ApplicationBar.MenuItems.Cast<ApplicationBarMenuItem>();
            //    menuItems.First(item => item.Text == "Tap + Send").IsEnabled = true;
            //}

            while (NavigationService.BackStack.Count() > 1) {
                NavigationService.RemoveBackEntry();
            }

            if (inUserPopup) {
                userpopup.IsOpen = true;
                inUserPopup = true;
            }

            if (Connection.CurrentCloud == null) {
                NavigationService.GoBack();
                return;
            }
            LoadingPopup.IsOpen = true;
            DerpyHoovesMailCenter.VerifyCloud(Connection.CurrentCloud.id, () => LoadingPopup.IsOpen = false);

            cloudinfoback.Visibility = Visibility.Collapsed;
            Leaving = true;
            Controller.Messages.CollectionChanged += ScrollDown;
            ScrollDown(null, null);

            if (datcloud != null && datcloud != Connection.CurrentCloud) {
                NavigateCloud(Connection.CurrentCloud);
            }

            datcloud = Connection.CurrentCloud;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            Debug.WriteLine("Navigated away");

            mediaPlayer.Stop();
            mediaPlayer.Source = null;

            if (Leaving && !Connection.IsMemberOfCloud(Connection.CurrentCloud.id)) {
                DerpyHoovesMailCenter.Unsubscribe(Connection.CurrentCloud.id);
            }
            Controller.Messages.CollectionChanged -= ScrollDown;
            Controller.MarkAsRead();
            Wasoncloud = false;
        }

        private void DropItemClick(object sender, RoutedEventArgs e) {
            var button = sender as FrameworkElement;
            if (button == null) return;
            var drop = button.DataContext as Drop;
            if (drop == null) return;

            Leaving = false;

            if (CloudsdaleUrl.IsMatch(drop.url.ToString())) {
                LayoutRoot.IsHitTestVisible = false;
                var apiend = new Uri("http://www.cloudsdale.org/v1/clouds/" + drop.url.ToString().Split('/').Last());
                WebPriorityManager.BeginHighPriorityRequest(apiend, args => Dispatcher.BeginInvoke(() => {
                    try {
                        var response = JObject.Parse(args.Result);
                        var cloud = PonyvilleDirectory.RegisterCloud(response["result"].ToObject<Cloud>());
                        LayoutRoot.IsHitTestVisible = true;
                        NavigateCloud(cloud);
                    } catch {
                        MessageBox.Show("We're sorry, the cloud you clicked on couldn't be loaded :< Maybe it has been deleted?");
                        LayoutRoot.IsHitTestVisible = true;
                    }
                }));
            } else {
                LastDropClicked = drop;
                NavigationService.Navigate(new Uri("/DropViewer.xaml", UriKind.Relative));
            }
        }

        public static Drop LastDropClicked;

        // ReSharper disable InconsistentNaming
        private Popup pop;
        // ReSharper restore InconsistentNaming
        private void SendBoxDoubleTap(object sender, GestureEventArgs e) {
            if (pop != null && pop.IsOpen) {
                pop.IsOpen = false;
                LayoutRoot.Children.Remove(pop);
                return;
            }

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("ux.allowaltcodes")) {
                return;
            }

            var grid = new Grid {
                Width = 250,
                Height = 75
            };

            var inputScope = new InputScope();
            var name = new InputScopeName { NameValue = InputScopeNameValue.Number };
            inputScope.Names.Add(name);

            var tb = new TextBox {
                Margin = new Thickness(5, 0, 85, 0),
                Padding = new Thickness(0),
                InputScope = inputScope,
                FontSize = 30,
                MaxLength = 5
            };
            grid.Children.Add(tb);

            var btn = new Button {
                Width = 85,
                Margin = new Thickness(5),
                Content = "INS",
                HorizontalAlignment = HorizontalAlignment.Right,
                Padding = new Thickness(0),
            };
            btn.Click += (o, args) => {
                ushort value;
                if (!ushort.TryParse(tb.Text, out value)) {
                    MessageBox.Show("Value must be between 0 and 65535");
                    tb.Focus();
                    return;
                }
                SendBox.Text += (char)value;
                tb.Text = "";
                if (IsolatedStorageSettings.ApplicationSettings.Contains("ux.recursivealtcodeentry")) {
                    tb.Focus();
                } else {
                    pop.IsOpen = false;
                    LayoutRoot.Children.Remove(pop);
                    SendBox.Focus();
                    SendBox.Select(SendBox.Text.Length, 0);
                }
            };
            grid.Children.Add(btn);

            var border = new Border {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x91, 0xDB)),
                Child = grid
            };

            pop = new Popup {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 75),
                Width = 250,
                Height = 75,
                Child = border
            };
            LayoutRoot.Children.Add(pop);

            pop.IsOpen = true;

            tb.Focus();
        }

        private void SendBoxTap(object sender, GestureEventArgs e) {
            if (pop != null && pop.IsOpen) {
                pop.IsOpen = false;
                LayoutRoot.Children.Remove(pop);
            }
        }

        private void ChatScrollerTap(object sender, GestureEventArgs e) {
            if (pop != null && pop.IsOpen) {
                pop.IsOpen = false;
                LayoutRoot.Children.Remove(pop);
            }
        }

        private void MoreDropsClick(object sender, RoutedEventArgs e) {
            MoreDrops.Visibility = Visibility.Collapsed;
            if (!Controller.DropController.CanIncreaseCapacity) {
                return;
            }
            var request = WebRequest.CreateHttp(new Uri(Res.PreviousDropsEndpoint.Replace("{cloudid}", Connection.CurrentCloud.id)));
            var nextpage = ((Controller.DropController.Capacity / 10) + 1).ToString(CultureInfo.InvariantCulture);
            request.Headers["X-Result-Page"] = nextpage;
            request.BeginGetResponse(ai => {
                try {
                    var response = request.EndGetResponse(ai);
                    string resultString;
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                        resultString = reader.ReadToEnd();
                    }
                    var result = JsonConvert.DeserializeObject<WebDropResponse>(resultString);
                    Controller.DropController.IncreaseCapacity(result.result);
                    if (Controller.DropController.CanIncreaseCapacity)
                        Dispatcher.BeginInvoke(() => MoreDrops.Visibility = Visibility.Visible);
                } catch (WebException) {
                }
            }, null);
        }

        private bool _searching;
        private void SearchClick(object sender, RoutedEventArgs e) {
            if (!_searching) {
                if (string.IsNullOrWhiteSpace(SearchBar.Text)) {
                    MessageBox.Show("Please enter a search query");
                    return;
                }
                _searching = true;
                MoreDrops.IsEnabled = false;
                SearchButtonImage.Source = new BitmapImage(new Uri("/Images/Icons/back.png", UriKind.Relative));
                MoreDrops.Content = "Searching...";
                var searchList = new ObservableCollection<Drop>();
                MediaList.ItemsSource = searchList;
                var request = WebRequest.CreateHttp(
                    new Uri(Res.DropsSearchEndpoint.Replace("{cloudid}", Connection.CurrentCloud.id)
                                                   .Replace("{query}", Uri.EscapeDataString(SearchBar.Text.Trim()))));
                request.BeginGetResponse(ai => {
                    var response = request.EndGetResponse(ai);
                    string resultString;
                    using (var stream = response.GetResponseStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                        resultString = reader.ReadToEnd();
                    }
                    var result = JsonConvert.DeserializeObject<WebDropResponse>(resultString);
                    Dispatcher.BeginInvoke(() => {
                        MoreDrops.Visibility = Visibility.Collapsed;
                        foreach (var d in result.result) {
                            searchList.Add(d);
                        }
                    });
                }, null);
            } else {
                _searching = false;
                SearchBar.Text = "";
                MoreDrops.IsEnabled = true;
                MoreDrops.Content = "Load More";
                MoreDrops.Visibility = Visibility.Visible;
                MediaList.ItemsSource = Controller.Drops;
                SearchButtonImage.Source = new BitmapImage(new Uri("/Images/Icons/search.png", UriKind.Relative));
            }
        }

        private void DropImageFailed(object sender, ExceptionRoutedEventArgs e) {
            if (sender is Image) {
                var image = sender as Image;
                image.Source = new BitmapImage(new Uri("http://assets.cloudsdale.org/assets/fallback/preview_thumb_drop.png"));
            }
        }

        private void RemoveThisCloud(object sender, EventArgs eventArgs) {
            if (MessageBox.Show("Are you sure you want to leave this cloud?",
                "", MessageBoxButton.OKCancel) != MessageBoxResult.OK) {
                return;
            }
            Connection.CurrentCloudsdaleUser.clouds = (
                from cloud in Connection.CurrentCloudsdaleUser.clouds
                where cloud.id != Connection.CurrentCloud.id
                select cloud).ToArray();
            Connection.LeaveCloud(Connection.CurrentCloud.id);
            NavigationService.GoBack();
        }

        private void UserListClick(object sender, RoutedEventArgs e) {
            if (!(sender is Button)) return;
            var button = sender as Button;
            if (!(button.DataContext is CensusUser)) return;
            var user = button.DataContext as CensusUser;

            userpopup.DataContext = user;
            userpopup.IsOpen = true;
            inUserPopup = true;

            UpdateBans(user);
        }

        private void AvatarMouseUp(object sender, MouseButtonEventArgs e) {
            if (!(sender is Image)) return;
            var image = sender as Image;

            if (image.Opacity > .9) return;
            image.Opacity = 1.0;

            if (!(image.DataContext is Message)) return;
            var message = image.DataContext as Message;

            var user = PonyvilleCensus.GetUser(message.user.id);

            userpopup.DataContext = message.user;
            userpopup.IsOpen = true;
            inUserPopup = true;

            UpdateBans(user);
        }

        private void AvatarMouseDown(object sender, MouseButtonEventArgs e) {
            ((FrameworkElement)sender).Opacity = 0.8;
        }

        private void AvatarMouseLeave(object sender, MouseEventArgs e) {
            ((FrameworkElement)sender).Opacity = 1.0;
        }

        private void CloudInfoClick(object sender, EventArgs eventArgs) {
            if (userpopup.IsOpen) {
                userpopup.IsOpen = false;
                inUserPopup = false;
            }
            CloudInfoPopup.DataContext = Connection.CurrentCloud;
            CloudInfoPopup.IsOpen = true;
            cloudinfoback.Visibility = Visibility.Visible;
            CloudInfoDown.Begin();
        }

        private void EditCloudClick(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("/EditCloud.xaml", UriKind.Relative));
        }

        private void BanBanBan(object sender, RoutedEventArgs e) {
            BanTime.Value = DateTime.Now;
            BanDate.Value = DateTime.Now.Date;

            ModToolsButtons.Visibility = Visibility.Collapsed;
            BanTools.Visibility = Visibility.Visible;
        }

        public void NavigateCloud(Cloud cloud) {
            Dispatcher.BeginInvoke(() => {
                Controller.Messages.CollectionChanged -= ScrollDown;

                Connection.CurrentCloud = cloud;

                userpopup.IsOpen = false;
                inUserPopup = false;

                Chats.ItemsSource = new Message[0];
                MediaList.ItemsSource = new Drop[0];
                Ponies.DataContext = null;
                cloudPivot.Title = "";

                new Thread(() => {
                    Thread.Sleep(100);
                    Dispatcher.BeginInvoke(() => {
                        Controller = DerpyHoovesMailCenter.GetCloud(Connection.CurrentCloud);
                        Controller.Messages.CollectionChanged += ScrollDown;

                        if (Connection.CurrentCloudsdaleUser.Clouds.All(cloud1 => cloud1.id != Connection.CurrentCloud.id)) {
                            Connection.JoinCloud(Connection.CurrentCloud.id);
                            Connection.CurrentCloudsdaleUser.clouds = new List<Cloud>
                                (Connection.CurrentCloudsdaleUser.clouds) { Connection.CurrentCloud }.ToArray();
                        }

                        cloudPivot.Title = Connection.CurrentCloud.name;

                        LoadingPopup.IsOpen = true;
                        DerpyHoovesMailCenter.VerifyCloud(Connection.CurrentCloud.id, () => LoadingPopup.IsOpen = false);

                        new Thread(() => {
                            Thread.Sleep(75);
                            Dispatcher.BeginInvoke(() => {
                                Chats.ItemsSource = Controller.Messages;
                                MediaList.ItemsSource = Controller.Drops;
                                Ponies.DataContext = Controller.Users;
                                ScrollDown(null, null);
                            });
                        }).Start();

                        Wasoncloud = true;

                        cloudPivot.SelectedIndex = 0;
                    });
                }).Start();
            });
        }

        private void InChatDropTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e) {
            var button = sender as FrameworkElement;
            if (button == null) return;
            var drop = button.DataContext as Drop;
            if (drop == null) return;

            Leaving = false;

            if (CloudsdaleUrl.IsMatch(drop.url.ToString())) {
                LayoutRoot.IsHitTestVisible = false;
                WebPriorityManager.BeginHighPriorityRequest(new Uri("http://www.cloudsdale.org/v1/clouds/" + drop.url.ToString()
                    .Split('/').Last()), args => {
                        var response = JObject.Parse(args.Result);
                        var cloud = PonyvilleDirectory.RegisterCloud(response["result"].ToObject<Cloud>());
                        Dispatcher.BeginInvoke(() => LayoutRoot.IsHitTestVisible = true);
                        NavigateCloud(cloud);
                    });
            } else {
                LastDropClicked = drop;
                NavigationService.Navigate(new Uri("/DropViewer.xaml", UriKind.Relative));
            }
        }

        private void DontBanBanBan(object sender, RoutedEventArgs e) {
            ModToolsButtons.Visibility = Visibility.Visible;
            BanTools.Visibility = Visibility.Collapsed;
        }

        private void DoTheBanBanBan(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(BanReason.Text)) {
                MessageBox.Show("Please enter a ban reason");
                return;
            }

            var button = (Button)sender;
            var user = (CensusUser)button.DataContext;

            if (BanTime.Value == null || BanDate.Value == null) {
                MessageBox.Show("Please enter a ban date/time");
                return;
            }

            var time = BanDate.Value.Value.Date + BanTime.Value.Value.TimeOfDay;

            if (MessageBox.Show(user.name + " will be banned until " + time, "Ban " + user.name,
                    MessageBoxButton.OKCancel) != MessageBoxResult.OK) {
                return;
            }

            user.Ban(BanReason.Text, time, Connection.CurrentCloud.id, () => UpdateBans(user));
            BanReason.Text = "";
            DontBanBanBan(sender, e);
        }

        private void AvatarImageFailed(object sender, ExceptionRoutedEventArgs e) {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("http://assets.cloudsdale.org/assets/fallback/avatar_preview_user.png"));
        }

        private static readonly Random AprilFoolsRandom = new Random();
        private void SendTextClick(object sender, EventArgs e) {
            if (cloudPivot.SelectedIndex != 0 || userpopup.IsOpen || CloudInfoPopup.IsOpen) {
                userpopup.IsOpen = false;
                inUserPopup = false;
                CloudInfoPopup.IsOpen = false;
                cloudinfoback.Visibility = Visibility.Collapsed;
                cloudPivot.SelectedIndex = 0;

                SendBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(SendBox.Text)) {
                return;
            }
            if (SendBox.Text.StartsWith("#shakeit")) {
                SendBox.Text = "";
                DoTheHarlemShake();
                SendBox.IsEnabled = false;
                Dispatcher.BeginInvoke(() => SendBox.IsEnabled = true);
                return;
            }

            var text = StringParser.EscapeLiteral(SendBox.Text.Replace('\r', '\n'));

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1 && AprilFoolsRandom.NextDouble() > 0.5) {
                text = text.WordJumble();
            }

            var controller = DerpyHoovesMailCenter.GetCloud(Connection.CurrentCloud);
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
                //message.content = (string)result["content"];

                cmessages.cache.Trigger(controller.IndexOf(message));
            });
            SendBox.Text = "";

            if (!(sender is SpeechRecognizerUI || sender is SpeechRecognizer)) {
                SendBox.Focus();
            }
        }

        private void SendBoxSizeChanged(object sender, SizeChangedEventArgs e) {
            ScrollDown(null, null);
        }

        private readonly Random harlemRandom = new Random();
        private bool harlemShaking;
        private void DoTheHarlemShake() {
            if (harlemShaking) return;
            harlemShaking = true;
            mediaPlayer.Source = new Uri("/Sound/harlem-shake.mp3", UriKind.Relative);
            mediaPlayer.Play();
        }
        private void HarlemShakeReady(object sender, RoutedEventArgs e) {
            var titletext = LayoutRoot.AllChildrenMatching(
                    child => child is TextBlock && (child as TextBlock).Text ==
                        (string)cloudPivot.Title).OfType<TextBlock>().First();

            ShakeText(titletext);

            new Thread(() => {
                Thread.Sleep(30 * 1000);
                harlemShaking = false;
                Dispatcher.BeginInvoke(() => {
                    titletext.Projection = new PlaneProjection();
                    var images = LayoutRoot.AllChildrenMatching<Image>().OfType<Image>();
                    foreach (var image in images) {
                        image.Projection = new PlaneProjection();
                    }
                });
            }).Start();
            new Thread(() => {
                Thread.Sleep((int)(15.3 * 1000));
                Dispatcher.BeginInvoke(() => {
                    var images = LayoutRoot.AllChildrenMatching<Image>().OfType<Image>();
                    foreach (var image in images) {
                        HarlemImage(image);
                    }
                });
            }).Start();
        }
        private void HarlemImage(UIElement image) {
            if (image.Projection == null) image.Projection = new PlaneProjection();
            Timeline animation1 = null;
            Timeline animation2 = null;
            Timeline animation3 = null;
            PropertyPath property1 = null;
            PropertyPath property2 = null;
            PropertyPath property3 = null;
            if (harlemRandom.NextDouble() < .3) {
                animation1 = new DoubleAnimation {
                    From = 0,
                    To = 360,
                    Duration = new Duration(TimeSpan.FromSeconds(1)),
                    RepeatBehavior = new RepeatBehavior(14.2),
                };
                property1 = new PropertyPath("(PlaneProjection.RotationZ)");
            }
            if (harlemRandom.NextDouble() < .5) {
                animation2 = new DoubleAnimation {
                    From = 0,
                    To = -1000,
                    Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                    RepeatBehavior = new RepeatBehavior(14.2 * 5),
                    AutoReverse = true,
                };
                property2 = new PropertyPath("(PlaneProjection.LocalOffsetZ)");
            }
            if (harlemRandom.NextDouble() < .5) {
                animation3 = new DoubleAnimation {
                    From = -10,
                    To = 10,
                    Duration = new Duration(TimeSpan.FromSeconds(0.1)),
                    RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(14.2 * 5)),
                    AutoReverse = true,
                };
                property3 = new PropertyPath("(PlaneProjection.LocalOffsetX)");
            }
            if (animation1 == null && animation2 == null && animation3 == null) return;

            var storyboard = new Storyboard();
            if (animation1 != null) {
                storyboard.Children.Add(animation1);
                Storyboard.SetTarget(animation1, image.Projection);
                Storyboard.SetTargetProperty(animation1, property1);
            }
            if (animation2 != null) {
                storyboard.Children.Add(animation2);
                Storyboard.SetTarget(animation2, image.Projection);
                Storyboard.SetTargetProperty(animation2, property2);
            }
            if (animation3 != null) {
                storyboard.Children.Add(animation3);
                Storyboard.SetTarget(animation3, image.Projection);
                Storyboard.SetTargetProperty(animation3, property3);
            }

            LayoutRoot.Resources.Add(Guid.NewGuid().ToString(), storyboard);

            storyboard.Begin();
        }
        private void ShakeText(FrameworkElement text) {
            if (text.Projection == null) text.Projection = new PlaneProjection {
                CenterOfRotationY = .5
            };


            var sidetoside = new DoubleAnimation {
                From = -10,
                To = 10,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                RepeatBehavior = new RepeatBehavior(30 * 5 / 2.0),
                AutoReverse = true,
            };

            var slightrotation = new DoubleAnimation {
                From = -10,
                To = 10,
                Duration = new Duration(TimeSpan.FromSeconds(.05)),
                RepeatBehavior = new RepeatBehavior(30 * 20 / 2.0),
                AutoReverse = true,
            };

            var centerofrotation = new DoubleAnimation {
                From = -1.1,
                To = 1.1,
                Duration = new Duration(TimeSpan.FromSeconds(.3)),
                RepeatBehavior = new RepeatBehavior(30 * 3.3333333333 / 2.0),
                AutoReverse = true,
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(sidetoside);
            storyboard.Children.Add(slightrotation);
            storyboard.Children.Add(centerofrotation);

            Storyboard.SetTarget(sidetoside, text.Projection);
            Storyboard.SetTargetProperty(sidetoside, new PropertyPath("(PlaneProjection.LocalOffsetX)"));

            Storyboard.SetTarget(slightrotation, text.Projection);
            Storyboard.SetTargetProperty(slightrotation, new PropertyPath("(PlaneProjection.RotationZ)"));

            Storyboard.SetTarget(centerofrotation, text.Projection);
            Storyboard.SetTargetProperty(centerofrotation, new PropertyPath("(PlaneProjection.CenterOfRotationY)"));

            LayoutRoot.Resources.Add(Guid.NewGuid().ToString(), storyboard);

            storyboard.Begin();
        }

        private void PromoteDemoteClick(object sender, RoutedEventArgs e) {
            var button = (Button)sender;
            var user = (User)button.DataContext;

            if (user.id == Connection.CurrentCloudsdaleUser.id) {
                MessageBox.Show("You can't demod yourself silly!");
                return;
            }

            if (user.ModOfCurrent) {
                Connection.CurrentCloud.RemoveModerator(user.id);
            } else {
                Connection.CurrentCloud.AddModerator(user.id);
            }
        }

        private void ChatLinkClicked(object sender, LinkClickedEventArgs eargs) {
            Uri linkuri;

            if (!Uri.TryCreate(eargs.LinkValue, UriKind.Absolute, out linkuri) &&
                !Uri.TryCreate("http://" + eargs.LinkValue, UriKind.Absolute, out linkuri)) {
                return;
            }

            var drop = new Drop {
                id = Guid.NewGuid().ToString(),
                preview = new Uri("http://assets.cloudsdale.org/assets/fallback/preview_thumb_drop.png"),
                title = eargs.LinkValue,
                url = linkuri,
            };

            if (CloudsdaleUrl.IsMatch(drop.url.ToString())) {
                LayoutRoot.IsHitTestVisible = false;
                var apiend = new Uri("http://www.cloudsdale.org/v1/clouds/" + drop.url.ToString().Split('/').Last());
                WebPriorityManager.BeginHighPriorityRequest(apiend, args => Dispatcher.BeginInvoke(() => {
                    try {
                        var response = JObject.Parse(args.Result);
                        var cloud = PonyvilleDirectory.RegisterCloud(response["result"].ToObject<Cloud>());
                        LayoutRoot.IsHitTestVisible = true;
                        NavigateCloud(cloud);
                    } catch {
                        MessageBox.Show("We're sorry, the cloud you clicked on couldn't be loaded :< Maybe it has been deleted?");
                        LayoutRoot.IsHitTestVisible = true;
                    }
                }));
            } else {
                LastDropClicked = drop;
                NavigationService.Navigate(new Uri("/DropViewer.xaml", UriKind.Relative));
            }
        }

        private bool userpopupLeaving;
        private void UserPopupMouseUp(object sender, MouseButtonEventArgs e) {
            if (!userpopupLeaving) return;
            userpopupLeaving = false;
            var points = e.StylusDevice.GetStylusPoints(UserPopupBorder);
            if (points.Any(point => point.X > 0 && point.X < 450 && point.Y > 0 && point.Y < 450)) {
                return;
            }
            userpopup.IsOpen = false;
            inUserPopup = false;
        }
        private void UserPopupMouseDown(object sender, MouseButtonEventArgs e) {
            var points = e.StylusDevice.GetStylusPoints(UserPopupBorder);
            if (points.Any(point => point.X > 0 && point.X < 450 && point.Y > 0 && point.Y < 450)) {
                return;
            }
            userpopupLeaving = true;
        }

        private void CloudLinkTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            var tb = (TextBox)sender;
            tb.Text = ((Cloud)tb.DataContext).Link;
            tb.SelectAll();
        }

        private void CloudLinkTap(object sender, GestureEventArgs e) {
            var tb = (TextBox)sender;
            tb.SelectAll();
            tb.Focus();
        }

        private void ScreenshotClick(object sender, RoutedEventArgs e) {
            var menuItem = (MenuItem)sender;
            var menu = (ContextMenu)menuItem.Parent;
            var grid = (Grid)menu.Owner;

            grid.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            var screenshot = new WriteableBitmap(grid, null);
            ViewShot.Image = screenshot;
            ViewShot.ScreenshotGrid = grid;
            NavigationService.Navigate(new Uri("/Screenshot/ViewShot.xaml", UriKind.Relative));
        }

        private void CopyClick(object sender, RoutedEventArgs e) {
            var menuItem = (MenuItem)sender;
            var menu = (ContextMenu)menuItem.Parent;
            var grid = (Grid)menu.Owner;
            var msg = (Message)grid.DataContext;
            Clipboard.SetText(msg.Split.Aggregate(new StringBuilder()
                .AppendLine(msg.user.name),
                (builder, line) => builder.Append("> ").AppendLine(line.Text)).ToString());
        }

        private void ScreenshotChat(object sender, EventArgs e) {
            var screenshot = new WriteableBitmap(LayoutRoot, null);
            ViewShot.Image = screenshot;
            ViewShot.ScreenshotGrid = LayoutRoot;
            NavigationService.Navigate(new Uri("/Screenshot/ViewShot.xaml", UriKind.Relative));
        }

        public void UpdateBans(User user) {
            if (Connection.CurrentCloud.IsModerator || Connection.CurrentCloudsdaleUser.role == "admin"
                || Connection.CurrentCloudsdaleUser.role == "developer" || Connection.CurrentCloudsdaleUser.role == "founder") {
                WebPriorityManager.BeginLowPriorityRequest(new Uri("http://www.cloudsdale.org/v1/clouds/" + Connection.CurrentCloud.id + "/bans.json?offender_id=" + user.id),
                    args => Dispatcher.BeginInvoke(() => {
                        user.Bans.Clear();
                        JObject.Parse(args.Result)["result"].Select(token => token.ToObject<Ban>())
                                                            .CopyTo(user.Bans);
                    }),
                    new KeyValuePair<string, string>("X-Auth-Token", Connection.CurrentCloudsdaleUser.auth_token),
                    new KeyValuePair<string, string>("Accept", "application/json"));
            }
        }

        private void RevokeBanClick(object sender, RoutedEventArgs e) {
            var ban = (Ban)((FrameworkElement)sender).DataContext;
            ban.Revoke(() => UpdateBans(ban.User));
        }

        private void AddOnSkype(object sender, RoutedEventArgs e) {
            var user = (User)((FrameworkElement)sender).DataContext;
            Launcher.LaunchUriAsync(new Uri("skype:" + user.skype_name));
        }

        private void PinToStart(object sender, EventArgs e) {
            var thisTile = ShellTile.ActiveTiles.FirstOrDefault(tile => {
                var cloudUri = CloudsdaleUriMapper.GetCloudsdaleUri(new Uri("cloudsdale://cloudsdale" + tile.NavigationUri));

                return cloudUri != null &&
                       (cloudUri.AbsolutePath == "/" + Connection.CurrentCloud.id ||
                        cloudUri.AbsolutePath == "/" + Connection.CurrentCloud.short_name);
            });

            DownloadImage(Connection.CurrentCloud.avatar.Normal, stream => {

                using (stream)
                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                using (var fs = storage.OpenFile("/Shared/ShellContent/clouds_" + Connection.CurrentCloud.id + "_avatar.png", FileMode.Create)) {
                    stream.CopyTo(fs);
                }

                var tileData = new StandardTileData {
                    Title = Connection.CurrentCloud.name,
                    BackgroundImage = new Uri("isostore:/Shared/ShellContent/clouds_" + Connection.CurrentCloud.id + "_avatar.png", UriKind.Absolute),
                };

                if (thisTile != null) {
                    thisTile.Update(tileData);
                } else {
                    ShellTile.Create(new Uri("/CloudTile?cloudUri=" + HttpUtility.UrlEncode("cloudsdale://clouds/"
                                        + Connection.CurrentCloud.id), UriKind.Relative), tileData);
                }
            });
        }

        void DownloadImage(Uri remote, Action<Stream> callback) {
            var request = WebRequest.CreateHttp(remote);
            request.Accept = "image/png";
            request.BeginGetResponse(ar => {
                using (var response = request.EndGetResponse(ar)) {
                    callback(response.GetResponseStream());
                }
            }, null);
        }

        private async void RecordVoiceClick(object sender, EventArgs e) {
            var recorder = new SpeechRecognizerUI();

            recorder.Recognizer.Grammars.AddGrammarFromPredefinedType("default", SpeechPredefinedGrammar.Dictation);
            //recorder.Recognizer.Grammars.AddGrammarFromList("ponyshit", ReadGrammarFile());

            var result = await recorder.RecognizeWithUIAsync();
            if (result.ResultStatus != SpeechRecognitionUIStatus.Succeeded) return;
            SendBox.Text = GetSpeech(result.RecognitionResult);
            SendTextClick(recorder, e);
        }

        private IEnumerable<string> ReadGrammarFile() {
            using (var fs = Application.GetResourceStream(new Uri("Settings/CloudsdaleGrammars.txt", UriKind.Relative)).Stream)
            using (var reader = new StreamReader(fs)) {
                yield return reader.ReadLine();
            }
        } 

        static string GetSpeech(SpeechRecognitionResult result) {
            return new Regex(@"\<profanity\>(.*?)\<\/profanity\>").Replace(result.Text, match => match.Groups[1].Value);
        }

        private void TapAndSendClick(object sender, EventArgs e) {
            NavigationService.Navigate(new Uri("/NFC/Share.xaml", UriKind.Relative));
        }

        private void ZenModeClick(object sender, EventArgs e) {
            NavigationService.Navigate(new Uri("/ZenMode.xaml", UriKind.Relative));
        }
    }
}
