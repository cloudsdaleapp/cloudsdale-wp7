using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Cloudsdale.Models;
using Newtonsoft.Json.Linq;

namespace Cloudsdale.Managers {
    public static class Helpers {
        public static readonly Regex LinkRegex = new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static readonly Regex ItalicsRegex = new Regex(@"\B\/\b([^\/\n]+)\b\/\B", RegexOptions.Compiled);
        public static readonly Regex RedactedRegex = new Regex(@"\[REDACTED\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

            return controller.messages.cache.Count - 1;
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> matcher) {
            var i = 0;
            foreach (var item in enumerable) {
                if (matcher(item)) {
                    return i;
                }
                ++i;
            }
            return -1;
        }

        public static void CopyTo<T>(this IEnumerable<T> enumerable, IList<T> list) {
            foreach (var item in enumerable) {
                list.Add(item);
            }
        }

        public static byte[] Serialize(this object o, bool array = false) {
            return Encoding.UTF8.GetBytes(array ? JArray.FromObject(o).ToString() : JObject.FromObject(o).ToString());
        }

        public static string ReplaceRegex(this string input, string regex, string replacement, RegexOptions options = RegexOptions.None) {
            return input.Replace(new Regex(regex, options), replacement);
        }

        public static string ReplaceRegex(this string input, string regex, MatchEvaluator matcher, RegexOptions options = RegexOptions.None) {
            return input.Replace(new Regex(regex, options), matcher);
        }

        public static string Replace(this string input, Regex regex, string replacement) {
            return regex.Replace(input, replacement);
        }

        public static string Replace(this string input, Regex regex, MatchEvaluator matcher) {
            return regex.Replace(input, matcher);
        }

        public static void Shuffle<T>(this IList<T> list) {
            var rng = new Random();
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static string WordJumble(this string input) {
            var i = 0;
            return input.ReplaceRegex(@"[\w]+", match => {
                if (match.Value.Length < 3) return match.Value;
                ++i;
                var substr = match.Value.Substring(1, match.Value.Length - 2);
                var list = new List<char>(substr);
                list.Shuffle();
                return match.Value.First() + new string(list.ToArray()) + match.Value.Last();
            });
        }
    }
}
