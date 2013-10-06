using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using CloudsdaleWin7.lib;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.MVVM
{
    class SlashMe : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter.ToString() == "Inverse")
                return value.ToString().StartsWith("/me ") ? Visibility.Collapsed : Visibility.Visible;
            return value.ToString().StartsWith("/me ") ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class NameFromMessage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((User)value).Name;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Could not convert back!");
        }
    }

    public class ChatColors : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().StartsWith("/me")) return Colors.Green;
            if (value.ToString().StartsWith("//")) return Colors.CadetBlue;
            return CloudsdaleSource.PrimaryText;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CloudsdaleSource.PrimaryText;
        }
    }
}
