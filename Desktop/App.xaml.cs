using CloudsdaleWin7.lib.CloudsdaleLib;
using CloudsdaleWin7.lib.Controllers;

namespace CloudsdaleWin7 {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public readonly ConnectionController ConnectionController = new ConnectionController();
        public static Settings Settings = new Settings();

        public static ConnectionController Connection
        {
            get { return ((App)Current).ConnectionController; }
        }

        public static void Close()
        {
            Settings.Save();
            Connection.SubscriptionController.SaveSubscriptions();
        }
    }
}
