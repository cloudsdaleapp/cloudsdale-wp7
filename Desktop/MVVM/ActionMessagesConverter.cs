using System;
using System.Globalization;
using System.Windows.Data;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.MVVM
{
    public class ActionMessagesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messages = (string[])value;
            messages[0] = Message.SlashMeFormat.Replace(messages[0], "");
            return messages;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messages = (string[])value;
            messages[0] = "/me" + messages[0];
            return messages;
        }
    }
}
