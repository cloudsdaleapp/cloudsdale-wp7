using System;
using System.Threading.Tasks;

namespace CloudsdaleWin7.lib.ErrorConsole.CConsole
{
    public class WriteError
    {
        public async static Task ShowError(string error)
        {
            var messageFormat = "[" + DateTime.Now.ToString() + "] " + error + Environment.NewLine;
            ErrorConsole.Instance.Show();
            ErrorConsole.Instance.ConsoleText.Text += messageFormat;
        }
    }
}
