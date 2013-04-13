using System.ComponentModel;
using System.Linq;
using System.Windows;
using Cloudsdale.Models;

namespace Cloudsdale.Managers {
    public class SweetAppleAcres : AppleFarm<Message>, INotifyPropertyChanged {

        public SweetAppleAcres(int capacity)
            : base(capacity) {
        }

        public void AddToEnd(Message item) {
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                InternalAddToEnd(item);
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(() => InternalAddToEnd(item));
            }
        }

        private void InternalAddToEnd(Message item) {
            item.user = PonyvilleCensus.Heartbeat(item.user);

            var count = cache.Count;
            for (var i = 0; i < count; ++i) {
                if (cache[i].id == item.id) return;
                if (cache[i].subs.Any(msg => msg.id == item.id)) return;
            }

            if (cache.Count > 0 && !item.IsSlashMe) {
                var last = cache[cache.Count - 1];
                if (last.user.id == item.user.id) {
                    last.AddSub(item);
                    cache.Trigger(cache.Count - 1);
                } else {
                    cache.Add(item);
                }
            } else {
                cache.Add(item);
            }

            OnPropertyChanged("LastMessage");
        }

        public string LastMessage {
            get {
                if (cache.Count < 1) return "";
                var lastmsg = cache[cache.Count - 1];
                var lines = lastmsg.Split;
                var line = lines[lines.Length - 1];
                return lastmsg.user.name + ": " + line.Text;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
