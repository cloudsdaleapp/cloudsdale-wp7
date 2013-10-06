using System;
using System.Globalization;
using System.Windows.Data;

namespace CloudsdaleWin7.MVVM {
    public class DeviceIcon : IValueConverter {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try
            {
                switch (value.ToString())
                {
                    case "mobile":
                        return "📱";
                    case "robot":
                        return "⚙";
                    default:
                        return "";
                }
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
