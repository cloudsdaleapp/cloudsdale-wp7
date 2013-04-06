using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Cloudsdale.Controls {
    public class DisablingHubTile : HubTile {
        public static DependencyProperty IsDisabledProperty = DependencyProperty.Register
            ("IsDisabled", typeof(bool), typeof(DisablingHubTile), new PropertyMetadata(false));
        public bool IsDisabled {
            get { return (bool)GetValue(IsDisabledProperty); }
            set { SetValue(IsDisabledProperty, value); }
        }
    }
}
