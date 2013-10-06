using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CloudsdaleLib;
using CloudsdaleLib.Helpers;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Controllers;
using Cloudsdale_Metro.Views.Controls;
using Cloudsdale_Metro.Views.Controls.Flyout_Panels;
using Newtonsoft.Json;
using WinRTXamlToolkit.AwaitableUI;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Cloudsdale_Metro.Views {
    public sealed partial class CloudPage {
        private const int AutoCloudListThreshhold = 1360;

        #region Fields

        private CloudController cloudController;

        #endregion

        #region Load/Unload

        public CloudPage() {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            cloudController = App.Connection.MessageController.CurrentCloud;
            cloudController.UnreadMessages = 0;
            DefaultViewModel["Clouds"] = App.Connection.SessionController.CurrentSession.Clouds;
            CloudGrid.Width = 1;
            ScrollCloudIn();

            await this.WaitForLayoutUpdateAsync();
            AutoShowCloudList();

            await CloudListView.WaitForLayoutUpdateAsync();
            CloudListView.SelectedItem = cloudController.Cloud;

            await cloudController.EnsureLoaded();
            DefaultViewModel["Items"] = cloudController.Messages;

            cloudController.Messages.CollectionChanged += MessagesOnCollectionChanged;

            OverlayGrid.Visibility = Visibility.Collapsed;

            await this.WaitForLayoutUpdateAsync();
            ScrollChat();
        }

        private void CloudPage_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            AutoShowCloudList();
        }

        private void AutoShowCloudList() {
            ScrollCloudIn();
            if (ActualWidth > AutoCloudListThreshhold && BackButton.Visibility == Visibility.Visible) {
                BackButton.Visibility = Visibility.Collapsed;
                if (CloudGrid.Width < 320) {
                    CloudListExpand.Begin();
                }
            } else if (ActualWidth < AutoCloudListThreshhold && BackButton.Visibility == Visibility.Collapsed) {
                BackButton.Visibility = Visibility.Visible;
                if (CloudGrid.Width > 1) {
                    CloudListCollapse.Begin();
                }
            }
        }

        protected override void GoHome(object sender, RoutedEventArgs e) {
            App.Connection.NavigateHome();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            cloudController.Messages.CollectionChanged -= MessagesOnCollectionChanged;
        }

        #endregion

        #region Chat View

        private readonly List<Task> scrollTasks = new List<Task>();

        private async void ScrollChat(bool byItem = false, double? height = null) {
            await ChatList.WaitForNonZeroSizeAsync();
            await ChatList.WaitForLayoutUpdateAsync();

            await Task.Run(() => Task.WaitAll(scrollTasks.ToArray()));

            await ChatList.WaitForNonZeroSizeAsync();
            await ChatList.WaitForLayoutUpdateAsync();
            await Task.Delay(100);

            scrollTasks.Add(InternalScrollChat(byItem, height));
        }

        private async Task InternalScrollChat(bool byItem, double? height) {
            double scrollHeight;
            double amountToScroll;

            if (byItem) {
                var lastContainer = ChatList.ItemContainerGenerator.
                    ContainerFromItem(cloudController.Messages.Last()) as ContentPresenter;
                if (lastContainer == null) return;

                var multiplier = 1.0;
                if (height != null) {
                    multiplier = (double)height;
                }

                amountToScroll = lastContainer.ActualHeight * multiplier;
                scrollHeight = ChatScroll.VerticalOffset + amountToScroll;
                scrollHeight = Math.Min(scrollHeight, ChatScroll.ScrollableHeight);
            } else if (height != null) {
                amountToScroll = (double)height;
                amountToScroll = Math.Max(amountToScroll, 0);
                scrollHeight = ChatScroll.VerticalOffset + amountToScroll;
            } else {
                scrollHeight = ChatScroll.ScrollableHeight;
            }

            if (scrollHeight <= ChatScroll.VerticalOffset) {
                return;
            }

            await ChatScroll.ScrollToVerticalOffsetWithAnimation(scrollHeight, 0.1, new ExponentialEase());
        }

        private void ChatScroll_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            if (Math.Abs(e.NewSize.Width - e.PreviousSize.Width) > 10) {
                ScrollChat();
            }
        }

        private void MessagesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args) {
            //ScrollChat(height: 0.3, byItem: true);
        }

        private void ChatList_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            var change = e.NewSize.Height - e.PreviousSize.Height;
            ScrollChat(height: change);
        }

        #endregion

        #region Messaging

        protected override void OnAcceleratorKey(AcceleratorKeyParams args) {
            if (SendBox.FocusState != FocusState.Unfocused) {
                SendBoxKey(args);
            }
        }

        private void SendBoxKey(AcceleratorKeyParams args) {
            if (args.Args.EventType != CoreAcceleratorKeyEventType.KeyDown) return;
            ScrollChat();

            if (args.Key == VirtualKey.Enter) {
                SendBoxEnter(args.ShiftKey);
            }
        }

        private void SendBoxEnter(bool shift) {
            var text = SendBox.Text.Replace("\r\n", "\n");

            if (shift) {
                var index = SendBox.SelectionStart + SendBox.SelectionLength;

                SendBox.Text = text.Substring(0, index) + "\n" + text.Substring(index);
                SendBox.SelectionLength = 0;
                SendBox.SelectionStart = index + 1;
            } else {
                SendBox.Text = string.Empty;
                SendMessage(text);
            }
        }

        private async void SendMessage(string message) {
            if (string.IsNullOrWhiteSpace(message)) return;

            message = message.TrimEnd();

            var messageModel = new Message {
                Content = message.EscapeMessage(),
                Device = "desktop",
                ClientId = App.Connection.Faye.ClientId
            };

            var messageData = await JsonConvert.SerializeObjectAsync(messageModel);

            messageModel.Id = Guid.NewGuid().ToString();
            messageModel.Author = App.Connection.SessionController.CurrentSession;

            cloudController.Messages.AddToEnd(messageModel);

            var client = new HttpClient {
                DefaultRequestHeaders = {
                    { "Accept", "application/json" }, 
                    { "X-Auth-Token", App.Connection.SessionController.CurrentSession.AuthToken }
                }
            };
            var response = await client.PostAsync(Endpoints.CloudMessages.Replace("[:id]", cloudController.Cloud.Id),
                new StringContent(messageData) {
                    Headers = {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            );

            try {
                var responseText = await response.Content.ReadAsStringAsync();
                var fullMessage = await JsonConvert.DeserializeObjectAsync<WebResponse<Message>>(responseText);

                if (fullMessage == null) return;
                if (fullMessage.Flash != null) {
                    await App.Connection.ErrorController.OnError(fullMessage);
                    return;
                }

                fullMessage.Result.PreProcess();
                fullMessage.Result.CopyTo(messageModel);
            } catch (JsonException) { }
        }

        #endregion

        #region Cloud list

        private void CloudItemClicked(object sender, ItemClickEventArgs e) {
            if (e.ClickedItem == cloudController.Cloud) {
                if (ActualWidth < AutoCloudListThreshhold) {
                    CloudListCollapse.Begin();
                }
                return;
            }

            App.Connection.MessageController.CurrentCloud = App.Connection.MessageController[(Cloud)e.ClickedItem];
            App.Connection.MessageController.CurrentCloud.UnreadMessages = 0;
            Frame.Navigate(typeof(CloudPage));
        }

        private void CloudListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = (ListView)sender;
            // ReSharper disable RedundantCheckBeforeAssignment
            if (view.SelectedItem != cloudController.Cloud) {
                view.SelectedItem = cloudController.Cloud;
            }
            // ReSharper restore RedundantCheckBeforeAssignment
        }

        private async void CloudListExpand_OnCompleted(object sender, object e) {
            await CloudListView.WaitForLayoutUpdateAsync();
            ScrollCloudIn();
        }

        protected override void GoBack(object sender, RoutedEventArgs e) {
            if (CloudGrid.Width < 320) {
                ScrollCloudIn();
                CloudListExpand.Begin();
            } else {
                CloudListCollapse.Begin();
            }
        }

        private void ScrollCloudIn() {
            var scrollViewer = CloudListView.GetFirstDescendantOfType<ScrollViewer>();
            if (scrollViewer == null) return;
            if (CloudListView.Items == null) return;

            double scrollOffset;

            var totalHeight = scrollViewer.ExtentHeight;
            double itemIndex = CloudListView.Items.IndexOf(cloudController.Cloud);
            double itemCount = CloudListView.Items.Count;
            var indexRatio = itemIndex / itemCount;
            var viewportCenter = scrollViewer.ViewportHeight / 2.0;

            scrollOffset = (totalHeight * indexRatio) - (viewportCenter * .9);

            // If offset happens to be bigger than scrollable height use the scrollable height
            // Possible for items from the end of the list
            if (scrollOffset > scrollViewer.ScrollableHeight) {
                scrollOffset = scrollViewer.ScrollableHeight;
            }

            // scroll to calculated offset
            scrollViewer.ScrollToVerticalOffset(scrollOffset);
        }

        #endregion

        #region User List

        private void UsersListClick(object sender, RoutedEventArgs e) {
            new UserList(cloudController).FlyOut();
        }

        #endregion

        #region Cloud Info
        private void CloudInfoClick(object sender, RoutedEventArgs e) {
            new CloudPanel(cloudController).FlyOut();
        }
        #endregion
    }

    #region Helper Classes

    public class MessageTemplateSelector : DataTemplateSelector {
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {
            var message = (Message)item;
            var element = (FrameworkElement)container;
            var page = element.GetFirstAncestorOfType<Page>() ?? App.Connection.MainFrame.GetFirstDescendantOfType<CloudPage>();

            if (Message.SlashMeFormat.IsMatch(message.Content)) {
                return (DataTemplate)page.Resources["ActionChatTemplate"];
            }
            return (DataTemplate)page.Resources["StandardChatTemplate"];
        }
    }

    #endregion
}
