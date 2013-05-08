using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Cloudsdale.Managers;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using System.Linq;

namespace Cloudsdale.Controls {
    public partial class CloudTileManager {

        public event EventHandler<CloudEventArgs> CloudClicked;

        public CloudTileManager() {
            InitializeComponent();

            scrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
            scrollTimer.Tick += (sender, args) => {
                if (!isDragging || draggingItem == null) return;
                var y = Canvas.GetTop(draggingItem);
                if (y < Scroller.VerticalOffset && Scroller.VerticalOffset > 0) {
                    Scroller.ScrollToVerticalOffset(Scroller.VerticalOffset - 20);
                    y -= 20;
                    Canvas.SetTop(draggingItem, y);
                } else if (y > Scroller.VerticalOffset + ActualHeight - 173 && Scroller.VerticalOffset < Scroller.ScrollableHeight) {
                    Scroller.ScrollToVerticalOffset(Scroller.VerticalOffset + 20);
                    y += 20;
                    Canvas.SetTop(draggingItem, y);
                }
            };
            scrollTimer.Start();
        }

        public Pivot Pivot { get; set; }

        public event Action DragStart;
        public event Action DragStop;

        private readonly List<ContentPresenter> tiles = new List<ContentPresenter>();

        private ObservableCollection<Cloud> _source;
        public ObservableCollection<Cloud> ItemSource {
            get { return _source; }
            set {
                if (_source != null) _source.CollectionChanged -= SourceOnCollectionChanged;
                _source = value;
                _source.CollectionChanged += SourceOnCollectionChanged;
                foreach (var control in tiles) {
                    if (_source.All(cloud => cloud.id != ((Cloud)control.Content).id)) TileCanvas.Children.Remove(control);
                }
                for (var i = 0; i < _source.Count; i++) {
                    CreateTile(_source[i], i);
                }
            }
        }

        private bool MoveTile(CloudsdaleItem cloud, int index) {
            var tile = tiles.FirstOrDefault(control => ((Cloud)control.Content).id == cloud.id);
            if (tile == null) return false;
            if (isDragging && draggingItem != null && tile.DataContext == draggingItem.DataContext) return true;

            var x = (index % 2 == 0) ? 5 : 185;
            var y = Math.Floor(index / 2.0) * 180;

            var leftAnimation = new DoubleAnimation {
                From = Canvas.GetLeft(tile),
                To = x,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
            };

            var topAnimation = new DoubleAnimation {
                From = Canvas.GetTop(tile),
                To = y,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(leftAnimation);
            storyboard.Children.Add(topAnimation);

            Storyboard.SetTarget(leftAnimation, tile);
            Storyboard.SetTarget(topAnimation, tile);

            Storyboard.SetTargetProperty(leftAnimation, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTargetProperty(topAnimation, new PropertyPath(Canvas.TopProperty));

            Resources.Add(Guid.NewGuid().ToString(), storyboard);

            storyboard.Begin();

            return true;
        }

        private void CreateTile(CloudsdaleItem cloud, int index) {
            if (MoveTile(cloud, index)) return;
            var control = new ContentPresenter { ContentTemplate = ItemTemplate, Content = cloud };
            Canvas.SetLeft(control, (index % 2 == 0) ? 5 : 185);
            Canvas.SetTop(control, Math.Floor(index / 2.0) * 180);
            TileCanvas.Children.Add(control);
            tiles.Add(control);

            LayoutRoot.Height = Math.Ceiling(tiles.Count / 2.0) * 180;
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset) {
            } else if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add) {
                CreateTile((Cloud)notifyCollectionChangedEventArgs.NewItems[0],
                           notifyCollectionChangedEventArgs.NewStartingIndex);
            }
        }

        private bool isDragging;
        private ContentPresenter draggingItem;
        private bool readyToDrag;

        private void GestureListenerDragStarted(object sender, DragStartedGestureEventArgs e) {

            isDragging = readyToDrag;

            if (!isDragging) {
                Scroller.IsHitTestVisible = true;
                Pivot.IsHitTestVisible = true;
                return;
            }

            var tile = (DisablingHubTile)sender;
            var grid = (TiltGrid)tile.Parent;
            var content = tiles.First(presenter => presenter.DataContext == grid.DataContext);

            draggingItem = content;

            Canvas.SetZIndex(content, 5);

            OnEvent(DragStart);
            Scroller.IsHitTestVisible = false;
        }

        private void GestureListenerDragDelta(object sender, DragDeltaGestureEventArgs e) {
            if (!isDragging) {
                Scroller.IsHitTestVisible = true;
                Pivot.IsHitTestVisible = true;
                return;
            }

            Scroller.IsHitTestVisible = false;

            var tile = (DisablingHubTile)sender;
            var grid = (TiltGrid)tile.Parent;
            var content = tiles.First(presenter => presenter.DataContext == grid.DataContext);

            var x = Canvas.GetLeft(content);
            x += e.HorizontalChange;
            Canvas.SetLeft(content, x);

            var y = Canvas.GetTop(content);
            y += e.VerticalChange;

            Canvas.SetTop(content, y);

            var cloudId = ((Cloud)grid.DataContext).id;
            var newindex = IndexForPosition(Canvas.GetLeft(content), Canvas.GetTop(content));
            var movement = newindex - PonyvilleDirectory.CloudIndex(cloudId);
            PonyvilleDirectory.MoveItem(cloudId, movement);

            Connection.CurrentCloudsdaleUser.clouds = Connection.CurrentCloudsdaleUser.clouds;
        }

        private void GestureListenerDragCompleted(object sender, DragCompletedGestureEventArgs e) {
            if (!isDragging) {
                Scroller.IsHitTestVisible = true;
                Pivot.IsHitTestVisible = true;
                return;
            }

            isDragging = false;

            var tile = (DisablingHubTile)sender;
            var grid = (TiltGrid)tile.Parent;
            var content = tiles.First(presenter => presenter.DataContext == grid.DataContext);

            Canvas.SetZIndex(content, 0);

            OnEvent(DragStop);

            var cloudId = ((Cloud)grid.DataContext).id;
            var newindex = IndexForPosition(Canvas.GetLeft(content), Canvas.GetTop(content));
            var movement = newindex - PonyvilleDirectory.CloudIndex(cloudId);
            PonyvilleDirectory.MoveItem(cloudId, movement);

            Connection.CurrentCloudsdaleUser.clouds = Connection.CurrentCloudsdaleUser.clouds;
        }

        private static void OnEvent(Action a) {
            if (a != null) {
                a();
            }
        }

        private readonly DispatcherTimer scrollTimer = new DispatcherTimer();

        private void GestureListenerGestureBegin(object sender, GestureEventArgs e) {
            readyToDrag = false;
        }

        private void GestureListenerGestureCompleted(object sender, GestureEventArgs e) {
            draggingItem = null;
            isDragging = false;
            readyToDrag = false;

            Scroller.IsHitTestVisible = true;
            Pivot.IsHitTestVisible = true;

            ((HubTile)sender).Opacity = 1;
        }

        public int IndexForPosition(double x, double y) {
            var index = (int)Math.Floor((y + 90) / 180.0) * 2;

            if (x > 100) ++index;

            return index;
        }

        private void GestureListenerTap(object sender, GestureEventArgs e) {
            var tile = (DisablingHubTile)sender;
            var cloud = (Cloud)tile.DataContext;
            CloudClicked(this, new CloudEventArgs { Cloud = cloud });
        }

        public class CloudEventArgs : EventArgs {
            public Cloud Cloud;
        }

        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Scroller.IsHitTestVisible = true;
            Pivot.IsHitTestVisible = true;
        }

        private void GestureListenerHold(object sender, GestureEventArgs e) {
            readyToDrag = true;

            Scroller.IsHitTestVisible = false;
            Pivot.IsHitTestVisible = false;
            ((HubTile)sender).Opacity = .5;
        }
    }
}
