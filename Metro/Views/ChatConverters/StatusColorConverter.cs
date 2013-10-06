using System;
using CloudsdaleLib.Models;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Cloudsdale_Metro.Views.ChatConverters {
    public class StatusColorConverter : IValueConverter {
        private static readonly Color Online = Colors.MediumSeaGreen;
        private static readonly Color Away = Colors.Yellow;
        private static readonly Color Busy = Colors.OrangeRed;
        private static readonly Color Offline = Colors.Gray;

        public object Convert(object value, Type targetType, object parameter, string language) {
            Color color;
            switch ((Status)value) {
                case Status.online:
                    color = Online;
                    break;
                case Status.away:
                    color = Away;
                    break;
                case Status.busy:
                    color = Busy;
                    break;
                default:
                    color = Offline;
                    break;
            }

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            var brush = (SolidColorBrush)value;
            var color = brush.Color;
            if (color == Online) {
                return Status.online;
            }
            if (color == Away) {
                return Status.away;
            }
            if (color == Busy) {
                return Status.busy;
            }
            return Status.offline;
        }
    }
}
