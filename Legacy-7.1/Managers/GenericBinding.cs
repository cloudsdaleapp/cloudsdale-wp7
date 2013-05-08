using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Cloudsdale.Managers {
    public class GenericBinding<T> : INotifyPropertyChanged {
        private readonly DependencyProperty property;
        private T val;

        public GenericBinding(DependencyProperty property) {
            this.property = property;
        }

        public void Bind(FrameworkElement bindable) {
            bindable.SetBinding(property, new Binding("Value") { Source = this });
        }

        public T Value {
            get { return val; }
            set {
                val = value;
                OnPropertyChanged("Value");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
