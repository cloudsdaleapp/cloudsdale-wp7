using System;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Cloudsdale_Metro.Common {
    public class DoubleToggleConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var values = ((string)parameter).Split(';').Select(double.Parse).ToList();
            return (bool)(value ?? true) ? values[0] : values[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            var values = ((string)parameter).Split(';').Select(double.Parse).ToList();
            return Math.Abs((double)value - values[0]) < ((values[0] - values[1]) / 2);
        }
    }
}
