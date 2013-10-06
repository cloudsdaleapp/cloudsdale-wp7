using System.ComponentModel;
using System.Runtime.CompilerServices;
using CloudsdaleLib.Annotations;
using CloudsdaleLib.Models;

namespace Cloudsdale_Metro.Models {
    public class LoginForm : INotifyPropertyChanged {
        private bool _isAuto;
        private string _password;
        private string _email;
        private Session _session;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Email {
            get { return _email; }
            set {
                if (value == _email) return;
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Password {
            get { return _password; }
            set {
                if (value == _password) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuto {
            get { return _isAuto; }
            set {
                if (value.Equals(_isAuto)) return;
                _isAuto = value;
                OnPropertyChanged();
            }
        }

        public Session Session {
            get { return _session; }
            set {
                _session = value;
                if (value != null) {
                    Email = value.Email;
                    Password = "Empty Password";
                    IsAuto = true;
                } else {
                    Email = "";
                    Password = "";
                    IsAuto = false;
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
