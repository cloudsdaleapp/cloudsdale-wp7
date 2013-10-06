using System;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Cloudsdale.Models;

namespace Cloudsdale.Controls {
    public class StatusColorConverter : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var status = (Status)value;
            var c = default(Color);
            switch (status) {
                case Status.Online:
                    c = Color.FromArgb(0xFF, 0x40, 0xBF, 0x40);
                    break;
                case Status.Away:
                    c = Color.FromArgb(0xFF, 0xFF, 0xEF, 0x4F);
                    break;
                case Status.Busy:
                    c = Color.FromArgb(0xFF, 0xAF, 0x2F, 0x2F);
                    break;
                case Status.Offline:
                    c = Color.FromArgb(0xFF, 0x7F, 0x7F, 0x7F);
                    break;
            }
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
