using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Cloudsdale.Controls {
    public partial class SettingBoundToggle : UserControl {
        public SettingBoundToggle() {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(SettingBoundToggle), new PropertyMetadata(default(string), LabelChangedProperty));

        private static void LabelChangedProperty(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ((SettingBoundToggle)dependencyObject).LabelBlock.Text = (string)dependencyPropertyChangedEventArgs.NewValue;
        }

        public string Label {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty SettingBindingProperty =
            DependencyProperty.Register("SettingBinding", typeof(string), typeof(SettingBoundToggle), new PropertyMetadata(default(string), SettingChangedCallback));

        private static void SettingChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ((SettingBoundToggle)dependencyObject).ReloadSetting();
        }

        public string SettingBinding {
            get { return (string)GetValue(SettingBindingProperty); }
            set { SetValue(SettingBindingProperty, value); }
        }

        private void ControlLoaded(object sender, RoutedEventArgs e) {
            ReloadSetting();
        }

        public void ReloadSetting() {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            try {
                Switch.IsChecked = settings.Contains(SettingBinding) && (bool)settings[SettingBinding];
            } catch {
                Switch.IsChecked = false;
            }
        }

        private void Switch_OnChecked(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings[SettingBinding] = true;
            Switch.SwitchForeground = (Brush)Resources["PhoneChromeBrush"];
        }

        private void Switch_OnUnchecked(object sender, RoutedEventArgs e) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings[SettingBinding] = false;
            Switch.SwitchForeground = new SolidColorBrush(Colors.Transparent);
        }

        public static bool IsSet(string setting) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            try {
                return settings.Contains(setting) && (bool)settings[setting];
            } catch {
                return false;
            }
        }
    }
}
