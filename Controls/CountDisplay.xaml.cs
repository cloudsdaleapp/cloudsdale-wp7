using System;
using System.Windows;
using System.Windows.Data;

namespace Cloudsdale.Controls {
    public partial class CountDisplay {
        public CountDisplay() {
            InitializeComponent();
            Visibility = Visibility.Collapsed;

            var binding = new Binding("Count") {Source = this};
            var prop = DependencyProperty.RegisterAttached(
                "Count",
                typeof (int),
                typeof (CountDisplay),
                new PropertyMetadata(Changed));
            SetBinding(prop, binding);
        }

        public void Changed(DependencyObject o, DependencyPropertyChangedEventArgs args) {
            Dispatcher.BeginInvoke(() => Update((int)args.NewValue));
        }

        public int Count {
            get { return (int) GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof (int), typeof (CountDisplay), new PropertyMetadata(0));

        private void Update(int count) {
            var value = count.ToString();
            if (value.Length == 0) {
                Visibility = Visibility.Collapsed;
            } else if (Visibility == Visibility.Collapsed) {
                Visibility = Visibility.Visible;
            }
            text.Text = value;
            text.FontSize = 28 - value.Length * 4;
            text.Margin = new Thickness(Math.Max(11 - value.Length * 3, 2), -1 + value.Length * 2.8, 0, 0);
        }
    }
}
