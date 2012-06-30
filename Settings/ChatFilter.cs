using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Text.RegularExpressions;

namespace Cloudsdale.Settings {
    public class ChatFilter {
        static ChatFilter() {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            Filter[] tempfilts;
            if (settings.TryGetValue("chat.filters", out tempfilts)) {
                Filters.AddRange(tempfilts);
            }
        }

        public static string Filter(string input) {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            {
                bool usebasicfilter;
                if (!settings.TryGetValue("chat.basicfilter", out usebasicfilter) || usebasicfilter) {
                    using (var reader = new StringReader(Resources.BasicProfanityFilter)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            var regex = new Regex(line, RegexOptions.IgnoreCase);
                            var matches = regex.Matches(input);
                            for (var i = 0; i < matches.Count; ++i) {
                                var repl = new StringBuilder();
                                for (var x = 0; x < matches[i].Length - 1; ++x) repl.Append('*');
                                input = input.Substring(0, matches[i].Index + 1) + repl +
                                        input.Substring(matches[i].Index + matches[i].Length);
                            }
                        }
                    }
                }
            }
            {
// ReSharper disable LoopCanBeConvertedToQuery
                foreach (var filter in Filters) {
                    input = filter.Replace(input);
                }
// ReSharper restore LoopCanBeConvertedToQuery
            }
            return input;
        }

        private static readonly List<Filter> Filters = new List<Filter>();
        public static void Add(Filter filter) {
            Filters.Add(filter);
        }
    }

    public struct Filter {
        public string match;
        public string Replacement;

        public string Replace(string input) {
            var regex = new Regex(match);
                var matches = regex.Matches(input);
                for (var i = 0; i < matches.Count; ++i) {
                    if (Replacement == null) {
                        var repl = new StringBuilder();
                        for (var x = 0; x < matches[i].Length; ++x) repl.Append('*');
                        input = input.Substring(0, matches[i].Index) + repl +
                                input.Substring(matches[i].Index + matches[i].Length);
                    } else {
                        input = input.Substring(0, matches[i].Index) + Replacement +
                                input.Substring(matches[i].Index + matches[i].Length);
                    }
                }
            return input;
        }
    }
}
