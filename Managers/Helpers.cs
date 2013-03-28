using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    public static class Helpers {
        public static readonly Regex LinkRegex = new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex CursiveRegex = new Regex(@"\/\b(.*?)\b\/", RegexOptions.Compiled);

        public static IEnumerable<DependencyObject> AllChildrenMatching(
            this DependencyObject root, Func<DependencyObject, bool> predicate) {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; ++i) {
                var child = VisualTreeHelper.GetChild(root, i);
                if (predicate(child)) yield return child;
                foreach (var descendant in child.AllChildrenMatching(predicate)) {
                    yield return descendant;
                }
            }
        }

        public static IEnumerable<DependencyObject> AllChildrenMatching<T>(
            this DependencyObject root) {
            return root.AllChildrenMatching(child => child is T);
        }

        public static IEnumerable<DependencyObject> AllChildren(
            this DependencyObject root) {
            return root.AllChildrenMatching(child => true);
        }

        public static string UppercaseFirst(this string str) {
            if (string.IsNullOrWhiteSpace(str)) {
                return "";
            }

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static void WriteLine(this MemoryStream memory, string line = "") {
            var data = Encoding.UTF8.GetBytes(line + "\r\n");
            memory.Write(data, 0, data.Length);
        }

        public static int IndexOf(this DerpyHoovesMailCenter controller, Message message) {
            lock (controller) {
                for (var i = 0; i < controller.messages.cache.Count; ++i) {
                    var pmessage = controller.messages.cache[i];
                    if (pmessage.id == message.id) return i;
                    if (pmessage.subs.Any(sub => sub.id == message.id)) {
                        return i;
                    }
                }
            }

            return controller.messages.cache.Count -1;
        }
    }
}
