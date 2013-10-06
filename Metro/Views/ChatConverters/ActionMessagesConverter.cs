using System;
using CloudsdaleLib.Models;
using Windows.UI.Xaml.Data;

namespace Cloudsdale_Metro.Views.ChatConverters {
    public class ActionMessagesConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var messages = (string[])value;
            messages[0] = Message.SlashMeFormat.Replace(messages[0], "");
            return messages;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            var messages = (string[])value;
            messages[0] = "/me" + messages[0];
            return messages;
        }
    }
}
