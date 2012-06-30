using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cloudsdale.Controls {
    public partial class CountDisplay : UserControl {
        public CountDisplay() {
            InitializeComponent();
        }

        private Managers.GenericBinding<String> binding; 
        public Managers.GenericBinding<String> Binding {
            get { return binding; }
            set {
                binding = value;
                binding.Bind(text);
                binding.PropertyChanged += (sender, args) => {
                    if (Dispatcher.CheckAccess()) {
                        Update();
                    } else {
                        Dispatcher.BeginInvoke(Update);
                    }
                };
            }
        }
        private void Update() {
            if (binding.Value.Length == 0) {
                Visibility = Visibility.Collapsed;
            } else if (Visibility == Visibility.Collapsed) {
                Visibility = Visibility.Visible;
            }
            text.FontSize = 28 - binding.Value.Length * 4;
            text.Margin = new Thickness(Math.Max(11 - binding.Value.Length * 3, 2), -1 + binding.Value.Length * 2.8, 0, 0);
        }
    }
}
