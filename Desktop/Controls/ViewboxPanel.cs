using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CloudsdaleWin7.Controls
{
    public class ViewboxPanel : Panel
    {
        private double _scale;

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ViewboxPanel),
            new PropertyMetadata(Orientation.Horizontal, OrientationChanged));

        private static void OrientationChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((ViewboxPanel)dependencyObject).InvalidateMeasure();
            ((ViewboxPanel)dependencyObject).InvalidateArrange();
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
       
        protected override Size ArrangeOverride(Size finalSize)
        {
            return Orientation == Orientation.Horizontal
                ? HorizontalArrange(finalSize)
                : VerticalArrange(finalSize);
        }

        private Size HorizontalArrange(Size finalSize)
        {
            var scaleTransform = new ScaleTransform { ScaleX = _scale, ScaleY = _scale };

            double widthSoFar = 0;
            foreach (Window child in Children)
            {
                var location = new Point(widthSoFar * _scale, 0);
                var size = new Size(child.Width, finalSize.Height / _scale);
                var rect = new Rect(location, size);
                child.RenderTransform = scaleTransform;
                child.Arrange(rect);
                widthSoFar += child.DesiredSize.Width;
            }

            return finalSize;
        }

        private Size VerticalArrange(Size finalSize)
        {
            var scaleTransform = new ScaleTransform { ScaleX = _scale, ScaleY = _scale };

            double heightSoFar = 0;
            foreach (Window child in Children)
            {
                var location = new Point(0, heightSoFar * _scale);
                var size = new Size(finalSize.Width / _scale, child.DesiredSize.Height);
                var rect = new Rect(location, size);
                child.RenderTransform = scaleTransform;
                child.Arrange(rect);
                heightSoFar += child.DesiredSize.Height;
            }

            return finalSize;
        }

        public static Size operator *(Size size, ViewboxPanel panel)
        {
            return new Size(size.Width / panel._scale, size.Height / panel._scale);
        }
    }
}
