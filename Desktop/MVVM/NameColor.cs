using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using CloudsdaleWin7.Views;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.MVVM {
    public class NameColor : IValueConverter {
        #region Implementation of IValueConverter


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Main.Instance.Clouds.SelectedIndex == -1) return null;
            var user = (User)value;
            var cloud = (Cloud)Main.Instance.Clouds.SelectedItem;

            var color = cloud.OwnerId == user.Id
                            ? Color.FromArgb(0xFF, 0x80, 0x00, 0xFF)
                            : cloud.ModeratorIds.Contains(user.Id)
                                  ? Color.FromArgb(0xFF, 0x33, 0x66, 0xFF)
                                  : Color.FromArgb(0xFF, 0x5A, 0x5A, 0x5A);
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
