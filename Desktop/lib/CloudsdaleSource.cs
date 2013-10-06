using System;
using System.IO;
using System.Windows.Media;

namespace CloudsdaleWin7.lib
{
    class CloudsdaleSource
    {
        #region Color list
        public static Color PrimaryBlue = Color.FromRgb(99, 160, 208);
        public static Color PrimaryBlueDark = Color.FromRgb(63, 133, 179);
        public static Color PrimaryText = Color.FromRgb(77, 77, 77);
        public static Color PrimaryBackground = Color.FromRgb(250, 250, 250);
        public static Color InnerContent = Color.FromRgb(34, 34, 34);
        public static Color InnerBackground = Colors.White;
        public static Color ErrorBright = Color.FromRgb(191, 94, 91);
        public static Color ErrorDark = Color.FromRgb(183, 76, 73);
        public static Color SuccessBright = Color.FromRgb(128, 206, 0);
        public static Color SuccessDark = Color.FromRgb(112, 180, 0);

        public static Color FounderTag = Color.FromRgb(255, 183, 230);
        public static Color DevTag = Color.FromRgb(142, 60, 255);
        public static Color AdminTag = Color.FromRgb(99, 151, 63);
        public static Color AssociateTag = Color.FromRgb(110, 110, 167);
        public static Color DonatorTag = Color.FromRgb(220, 220, 30);
        public static Color LegacyTag = Color.FromRgb(160, 160, 160);
        public static Color VerifiedTag = Color.FromRgb(40, 40, 250);

        public static Color OnlineStatus = Colors.LimeGreen;
        public static Color OfflineStatus = Colors.LightGray;
        public static Color AwayStatus = Colors.Yellow;
        public static Color BusyStatus = Colors.Red;
        #endregion
        #region Folders

        public static string Folder = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Cloudsdale\";
        public static string File = Folder + "Cloudsdale.exe";
        public static string SettingsFile = Folder + "settings.json";

        #endregion
    }
}
