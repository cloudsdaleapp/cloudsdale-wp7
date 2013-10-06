using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Cloudsdale_Metro.Views.Controls {
    public class ViewboxPanel : Panel {
        private double scale;

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ViewboxPanel),
            new PropertyMetadata(Orientation.Horizontal, OrientationChanged));

        private static void OrientationChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ((ViewboxPanel)dependencyObject).InvalidateMeasure();
            ((ViewboxPanel)dependencyObject).InvalidateArrange();
        }

        public Orientation Orientation {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize) {
            if (availableSize.Width < 1 || availableSize.Height < 1) {
                scale = 1;
                return availableSize;
            }
            return Orientation == Orientation.Horizontal
                ? HorizontalMeasure(availableSize)
                : VerticalMeasure(availableSize);
        }

        private Size HorizontalMeasure(Size availableSize) {
            var unlimitedSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            return InternalMeasure(availableSize, (element, size) => {
                element.Measure(unlimitedSize);
                size.Height = Math.Max(size.Height, element.DesiredSize.Height);
                size.Width += element.DesiredSize.Width;
                return size;
            }, size => {
                size.Height *= scale;
                size.Width *= scale;
                return size;
            });
        }

        private Size VerticalMeasure(Size availableSize) {
            var unlimitedSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            return InternalMeasure(availableSize, (element, size) => {
                element.Measure(unlimitedSize);
                size.Height += element.DesiredSize.Height;
                size.Width = Math.Max(size.Width, element.DesiredSize.Width);
                return size;
            }, size => {
                size.Width *= scale;
                size.Height *= scale;
                return size;
            });
        }

        private Size InternalMeasure(
            Size availableSize,
            Func<UIElement, Size, Size> childMesaureCallback,
            Func<Size, Size> finalSize) {

            double requiredHeight = 0;
            double requiredWidth = 0;

            var availableHeight = double.IsInfinity(availableSize.Height)
                                         ? Window.Current.Bounds.Height
                                         : availableSize.Height;
            var availableWidth = double.IsInfinity(availableSize.Width)
                                        ? Window.Current.Bounds.Width
                                        : availableSize.Width;

            foreach (var child in Children) {
                var childMeasure = childMesaureCallback(child, new Size(requiredWidth, requiredHeight));
                requiredHeight = childMeasure.Height;
                requiredWidth = childMeasure.Width;
            }

            requiredHeight = Math.Max(requiredHeight, 1);
            requiredWidth = Math.Max(requiredWidth, 1);

            scale = Math.Min(availableWidth / requiredWidth, availableHeight / requiredHeight);

            foreach (var child in Children) {
                child.Measure(new Size(availableWidth / scale, availableHeight / scale));
            }

            return finalSize(new Size(requiredWidth, requiredHeight));
        }

        protected override Size ArrangeOverride(Size finalSize) {
            return Orientation == Orientation.Horizontal
                ? HorizontalArrange(finalSize)
                : VerticalArrange(finalSize);
        }

        private Size HorizontalArrange(Size finalSize) {
            var scaleTransform = new ScaleTransform { ScaleX = scale, ScaleY = scale };

            double widthSoFar = 0;
            foreach (var child in Children) {
                var location = new Point(widthSoFar * scale, 0);
                var size = new Size(child.DesiredSize.Width, finalSize.Height / scale);
                var rect = new Rect(location, size);
                child.RenderTransform = scaleTransform;
                child.Arrange(rect);
                widthSoFar += child.DesiredSize.Width;
            }

            return finalSize;
        }

        private Size VerticalArrange(Size finalSize) {
            var scaleTransform = new ScaleTransform { ScaleX = scale, ScaleY = scale };

            double heightSoFar = 0;
            foreach (var child in Children) {
                var location = new Point(0, heightSoFar * scale);
                var size = new Size(finalSize.Width / scale, child.DesiredSize.Height);
                var rect = new Rect(location, size);
                child.RenderTransform = scaleTransform;
                child.Arrange(rect);
                heightSoFar += child.DesiredSize.Height;
            }

            return finalSize;
        }

        public static Size operator *(Size size, ViewboxPanel panel) {
            return new Size(size.Width / panel.scale, size.Height / panel.scale);
        }
    }
}
