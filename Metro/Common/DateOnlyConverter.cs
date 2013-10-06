using System;
using Windows.UI.Xaml.Data;

namespace Cloudsdale_Metro.Common {
    public class DateOnlyConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return ((DateTime) value).Date.ToString("d");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
