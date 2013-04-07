using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
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

            timer2.Interval = new TimeSpan(0, 0, 0, 0, 40);
            timer2.Tick += (sender, args) => {
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
            timer2.Start();
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
                    TileCanvas.Children.Remove(control);
                }
                tiles.Clear();
                for (var i = 0; i < _source.Count; i++) {
                    CreateTile(_source[i], i);
                }
            }
        }

        private void SourceOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset) {
                foreach (var control in tiles) {
                    TileCanvas.Children.Remove(control);
                }
                tiles.Clear();
            } else if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add) {
                CreateTile((Cloud) notifyCollectionChangedEventArgs.NewItems[0],
                           notifyCollectionChangedEventArgs.NewStartingIndex);
            }
        }

        private bool isDragging;
        private ContentPresenter draggingItem;

        private void GestureListenerDragStarted(object sender, DragStartedGestureEventArgs e) {

            isDragging = !Scroller.IsHitTestVisible;

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

            var tile = (DisablingHubTile) sender;
            var grid = (TiltGrid) tile.Parent;
            var content = tiles.First(presenter => presenter.DataContext == grid.DataContext);

            var x = Canvas.GetLeft(content);
            x += e.HorizontalChange;
            Canvas.SetLeft(content, x);

            var y = Canvas.GetTop(content);
            y += e.VerticalChange;

            Canvas.SetTop(content, y);
        }

        private void GestureListenerDragCompleted(object sender, DragCompletedGestureEventArgs e) {
            if (!isDragging) {
                Scroller.IsHitTestVisible = true;
                Pivot.IsHitTestVisible = true;
                return;
            }

            var tile = (DisablingHubTile)sender;
            var grid = (TiltGrid)tile.Parent;
            var content = tiles.First(presenter => presenter.DataContext == grid.DataContext);

            Canvas.SetZIndex(content, 0);

            ItemSource = ItemSource;

            OnEvent(DragStop);

            var cloudId = ((Cloud) grid.DataContext).id;
            var newindex = IndexForPosition(Canvas.GetLeft(content), Canvas.GetTop(content));
            var movement = newindex - PonyvilleDirectory.CloudIndex(cloudId);
            PonyvilleDirectory.MoveItem(cloudId, movement);

            Connection.CurrentCloudsdaleUser.clouds = Connection.CurrentCloudsdaleUser.clouds;
        }

        private void CreateTile(Cloud cloud, int index) {
            var control = new ContentPresenter { ContentTemplate = ItemTemplate, Content = cloud};
            Canvas.SetLeft(control, (index % 2 == 0) ? 5 : 185);
            Canvas.SetTop(control, Math.Floor(index/2.0) * 180);
            TileCanvas.Children.Add(control);
            tiles.Add(control);

            LayoutRoot.Height = Math.Ceiling(tiles.Count/2.0)*180;
        }

        private static void OnEvent(Action a) {
            if (a != null) {
                a();
            }
        }

        private DispatcherTimer timer;
        private readonly DispatcherTimer timer2 = new DispatcherTimer();

        private void GestureListenerGestureBegin(object sender, GestureEventArgs e) {
            timer = new DispatcherTimer();
            Pivot.IsHitTestVisible = false;
            timer.Tick += (o, args) => {
                timer.Stop();
                Scroller.IsHitTestVisible = false;
            };
            timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            timer.Start();
        }

        private void GestureListenerGestureCompleted(object sender, GestureEventArgs e) {
            timer.Stop();

            draggingItem = null;
            isDragging = false;

            Scroller.IsHitTestVisible = true;
            Pivot.IsHitTestVisible = true;
        }

        public int IndexForPosition(double x, double y) {
            var index = (int) Math.Floor((y+90)/180.0) * 2;

            if (x > 100) ++index;

            return index;
        }

        private void GestureListenerTap(object sender, GestureEventArgs e) {
            var tile = (DisablingHubTile) sender;
            var cloud = (Cloud) tile.DataContext;
            CloudClicked(this, new CloudEventArgs{Cloud = cloud});
        }

        public class CloudEventArgs : EventArgs {
            public Cloud Cloud;
        }

        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Scroller.IsHitTestVisible = true;
            Pivot.IsHitTestVisible = true;
        }
    }
}
