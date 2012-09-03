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
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Managers {
    public class PonyvilleDirectory {
        private static readonly Dictionary<string, Cloud> Clouds = new Dictionary<string, Cloud>(); 
        public static Cloud RegisterCloud(Cloud cloud) {
            lock (Clouds) {
                if (Clouds.ContainsKey(cloud.id)) {
                    Clouds[cloud.id].CopyFrom(cloud);
                    return Clouds[cloud.id];
                }
                return Clouds[cloud.id] = cloud;
            }
        }

        public static Cloud GetCloud(string id) {
            return Clouds[id];
        }
    }
}
