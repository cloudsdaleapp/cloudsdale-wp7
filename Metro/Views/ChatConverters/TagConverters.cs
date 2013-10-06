using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Cloudsdale_Metro.Views.ChatConverters {
    public class ShowTag : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var role = (string)value;
            return role == "normal" || string.IsNullOrWhiteSpace(role) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class TagText : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var role = (string)value;
            return role == "developer" ? "dev" : role;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }

    public class TagColor : IValueConverter {
        private static readonly Color Donor = Colors.Gold;
        private static readonly Color Legacy = Colors.DarkGray;
        private static readonly Color Associate = Colors.SteelBlue;
        private static readonly Color Verified = Colors.DeepSkyBlue;
        private static readonly Color Admin = Colors.MediumSeaGreen;
        private static readonly Color Developer = Colors.Purple;
        private static readonly Color Founder = Colors.DeepPink;

        public object Convert(object value, Type targetType, object parameter, string language) {
            var role = (string)value;
            Color color;
            switch (role) {
                case "donor":
                    color = Donor;
                    break;
                case "legacy":
                    color = Legacy;
                    break;
                case "associate":
                    color = Associate;
                    break;
                case "verified":
                    color = Verified;
                    break;
                case "admin":
                    color = Admin;
                    break;
                case "developer":
                    color = Developer;
                    break;
                case "founder":
                    color = Founder;
                    break;
                default:
                    color = Legacy;
                    break;
            }
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }
    }
}
