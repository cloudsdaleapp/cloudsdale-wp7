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
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Managers {
    public class BoolToVis : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (Visibility)value == Visibility.Visible;
        }

        #endregion
    }

    public class ArrayHasAnyVis : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var array = (Array)value;
            return array != null && array.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PromoOrDemo : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool)value ? "Demote Moderator" : "Promote to Moderator";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class JUri : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return new Uri(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return JToken.FromObject(value);
        }

        #endregion
    }

    public class Translucent : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Color color;
            if (value is SolidColorBrush) {
                color = (value as SolidColorBrush).Color;
            } else {
                color = (Color)value;
            }

            color.R += (byte)((255 - color.R) / 2);
            color.G += (byte)((255 - color.G) / 2);
            color.B += (byte)((255 - color.B) / 2);
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class StringTrim : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (value ?? "").ToString().ReplaceRegex(@"\r\n", "\n").ReplaceRegex(@"[\n]{2,255}", "\n\n").Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
