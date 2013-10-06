using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Cloudsdale_Metro.Views.ChatConverters {
    public class MessageDeviceConverter : IValueConverter {
        public static readonly Dictionary<string, string> IconMappings = new Dictionary<string, string> {
            {"desktop", ""}, {"mobile", "\U0001F4F1"}, {"robot", "\u2699"}
        };

        public object Convert(object value, Type targetType, object parameter, string language) {
            var device = (string)value;
            return IconMappings.ContainsKey(device) ? IconMappings[device] : IconMappings.First().Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            var icon = (string)value;
            return IconMappings.FirstOrDefault(kvp => kvp.Value == icon).Key ?? IconMappings.First().Key;
        }
    }
}
