using System;
using CloudsdaleLib.Models;
using Windows.UI.Xaml.Data;

namespace Cloudsdale_Metro.Common {
    public class StatusIndexConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return (Status)value;
        }
    }
}
