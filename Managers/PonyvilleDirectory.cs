using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Cloudsdale.Models;

namespace Cloudsdale.Managers {
    public class PonyvilleDirectory {
        private static readonly Dictionary<string, Cloud> Clouds = new Dictionary<string, Cloud>(); 
        public static void RegisterCloud(Cloud cloud) {
            Clouds[cloud.id] = cloud;
        }

        public static Cloud GetCloud(string id) {
            return Clouds[id];
        }
    }
}
