using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CloudsdaleWin7.lib;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.MVVM
{
    class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color;
            switch ((Status)value)
            {
                case Status.Online:
                    color = CloudsdaleSource.OnlineStatus;
                    break;
                case Status.Offline:
                    color = CloudsdaleSource.OfflineStatus;
                    break;
                case Status.Busy:
                    color = CloudsdaleSource.BusyStatus;
                    break;
                case Status.Away:
                    color = CloudsdaleSource.AwayStatus;
                    break;
                default:
                    color = CloudsdaleSource.OfflineStatus;
                    break;
            }
            return new SolidColorBrush(color);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
