using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Cloudsdale_Metro.Common {
    /// <summary>
    /// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var val = (value is bool && (bool)value);
            if (parameter is string && (parameter as string) == "Inverse") val = !val;
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            var val = value is Visibility && (Visibility)value == Visibility.Visible;
            if (parameter is string && (parameter as string) == "Inverse") val = !val;
            return val;
        }
    }
}
