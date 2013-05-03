using System.Windows;
using System.Windows.Media;

namespace Cloudsdale.Settings {
    static public class ForceTheme {

        static public void ForceLightTheme(this Application application) {
            ((SolidColorBrush)application.Resources["PhoneForegroundBrush"]).Color = Color.FromArgb(0xDE, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneBackgroundBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneContrastForegroundBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneContrastBackgroundBrush"]).Color = Color.FromArgb(0xDE, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneDisabledBrush"]).Color = Color.FromArgb(0x4D, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextCaretBrush"]).Color = Color.FromArgb(0xDE, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextBoxBrush"]).Color = Color.FromArgb(0x26, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextBoxForegroundBrush"]).Color = Color.FromArgb(0xDE, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextBoxEditBackgroundBrush"]).Color = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextBoxEditBorderBrush"]).Color = Color.FromArgb(0xDE, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextBoxReadOnlyBrush"]).Color = Color.FromArgb(0x2E, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneSubtleBrush"]).Color = Color.FromArgb(0x66, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextBoxSelectionForegroundBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxBrush"]).Color = Color.FromArgb(0x26, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxDisabledBrush"]).Color = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxCheckBrush"]).Color = Color.FromArgb(0xDE, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxCheckDisabledBrush"]).Color = Color.FromArgb(0x4D, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxPressedBrush"]).Color = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxPressedBorderBrush"]).Color = Color.FromArgb(0xDE, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneSemitransparentBrush"]).Color = Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneChromeBrush"]).Color = Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD);
            ((SolidColorBrush)application.Resources["PhoneInactiveBrush"]).Color = Color.FromArgb(0x33, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneInverseInactiveBrush"]).Color = Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5);
            ((SolidColorBrush)application.Resources["PhoneInverseBackgroundBrush"]).Color = Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD);
            ((SolidColorBrush)application.Resources["PhoneBorderBrush"]).Color = Color.FromArgb(0x99, 0x00, 0x00, 0x00);

        }

        static public void ForceDarkTheme(this Application application) {
            ((SolidColorBrush)application.Resources["PhoneForegroundBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneBackgroundBrush"]).Color = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneContrastForegroundBrush"]).Color = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneContrastBackgroundBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneDisabledBrush"]).Color = Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneTextCaretBrush"]).Color = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextBoxBrush"]).Color = Color.FromArgb(0xBF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneTextBoxForegroundBrush"]).Color = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneTextBoxEditBackgroundBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneTextBoxEditBorderBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneTextBoxReadOnlyBrush"]).Color = Color.FromArgb(0x77, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneSubtleBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneTextBoxSelectionForegroundBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxBrush"]).Color = Color.FromArgb(0xBF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxDisabledBrush"]).Color = Color.FromArgb(0x66, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxCheckBrush"]).Color = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxCheckDisabledBrush"]).Color = Color.FromArgb(0x66, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxPressedBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneRadioCheckBoxPressedBorderBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneSemitransparentBrush"]).Color = Color.FromArgb(0xAA, 0x00, 0x00, 0x00);
            ((SolidColorBrush)application.Resources["PhoneChromeBrush"]).Color = Color.FromArgb(0xFF, 0x1A, 0x91, 0xDB); //Color.FromArgb(0xFF, 0x00, 0x55, 0x80); Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneInactiveBrush"]).Color = Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneInverseInactiveBrush"]).Color = Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC);
            ((SolidColorBrush)application.Resources["PhoneInverseBackgroundBrush"]).Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            ((SolidColorBrush)application.Resources["PhoneBorderBrush"]).Color = Color.FromArgb(0xBF, 0xFF, 0xFF, 0xFF);
        }
    }

}
