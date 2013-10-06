﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Cloudsdale_Metro.Common {
    public class NonZeroToVisibility : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return ((int) value) != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
