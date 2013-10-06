using System.Linq;
using System.Threading.Tasks;
using CloudsdaleWin7.lib.ErrorConsole.CConsole;
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.Providers;

namespace CloudsdaleWin7.lib.Controllers
{
    public class ErrorController : IModelErrorProvider
    {
        public object LastError;

        public Task OnError<T>(WebResponse<T> response)
        {
            LastError = response;
            string title;
            var message = BuildMessage(response, out title);
            return WriteError.ShowError(message);
        }

        private static string BuildMessage<T>(WebResponse<T> response, out string title)
        {
            title = response.Flash != null ? response.Flash.Title : "An error occured";
            var message = response.Flash != null ? response.Flash.Message : "";
            return response.Errors.Aggregate(message, (current, error) => error.Node != null
                ? AppendLine(current, error.Node + " '" + error.Node + "' " + error.Message)
                : AppendLine(current, error.Message));
        }

        private static string AppendLine(string start, string line)
        {
            if (start.Length > 0)
            {
                return start + "\n" + line;
            }
            return start + line;
        }
    }
}
