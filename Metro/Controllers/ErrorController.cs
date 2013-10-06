using System;
using System.Linq;
using System.Threading.Tasks;
using CloudsdaleLib.Helpers;
using CloudsdaleLib.Providers;
using Windows.UI.Popups;

namespace Cloudsdale_Metro.Controllers {
    public class ErrorController : IModelErrorProvider {
        public object LastError;

        public async Task OnError<T>(WebResponse<T> response) {
            LastError = response;
            string title;
            var message = BuildMessage(response, out title);
            var dialog = new MessageDialog(message, title);
            await dialog.ShowAsync();
        }

        private static string BuildMessage<T>(WebResponse<T> response, out string title) {
            title = response.Flash != null ? response.Flash.Title : "An error occured";
            var message = response.Flash != null ? response.Flash.Message : "";
            return response.Errors.Aggregate(message, (current, error) => error.Node != null 
                ? AppendLine(current, error.Node + " '" + error.Node + "' " + error.Message) 
                : AppendLine(current, error.Message));
        }

        private static string AppendLine(string start, string line) {
            if (start.Length > 0) {
                return start + "\n" + line;
            }
            return start + line;
        }
    }
}
