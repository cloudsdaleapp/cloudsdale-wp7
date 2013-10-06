using System;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Cloudsdale_Metro.Helpers {
    public class MenuCommand<T> : ICommand where T : ICommand, new() {
        private readonly ICommand command;
        public MenuCommand() {
            command = new T();
            command.CanExecuteChanged += CanExecuteChanged;
        }

        public bool CanExecute(object parameter) {
            return command.CanExecute(parameter);
        }

        public void Execute(object parameter) {
            command.Execute(parameter);
            foreach (var popup in from popup in Window.Current.Content.Children<Popup>()
                                  where popup.IsLightDismissEnabled
                                  select popup) {
                popup.IsOpen = false;
            }
        }

        public event EventHandler CanExecuteChanged;

        ~MenuCommand() {
            command.CanExecuteChanged -= CanExecuteChanged;
        }
    }

    public class OpenLinkCommand : ICommand {
        public bool CanExecute(object parameter) {
            return true;
        }

        public async void Execute(object parameter) {
            await Launcher.LaunchUriAsync((Uri)parameter);
        }

        public event EventHandler CanExecuteChanged;
    }

    public class CopyLinkCommand : ICommand {
        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            var package = new DataPackage();
            package.SetText(parameter.ToString());
            Clipboard.SetContent(package);
        }

        public event EventHandler CanExecuteChanged;
    }
}
