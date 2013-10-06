using System;
using System.Linq;
using CloudsdaleLib.Models;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Cloudsdale_Metro.Views.ChatConverters {
    public class CloudNameColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var user = (User)value;
            var controller = App.Connection.MessageController.CurrentCloud;
            var cloud = controller.Cloud;

            var color = cloud.OwnerId == user.Id
                            ? Color.FromArgb(0xFF, 0x80, 0x00, 0xFF)
                            : cloud.ModeratorIds.Contains(user.Id)
                                  ? Color.FromArgb(0xFF, 0x33, 0x66, 0xFF)
                                  : Color.FromArgb(0xFF, 0x5A, 0x5A, 0x5A);
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
