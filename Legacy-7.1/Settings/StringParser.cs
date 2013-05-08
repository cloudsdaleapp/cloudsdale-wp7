using System.Collections.Generic;
using System.Text;

namespace Cloudsdale.Settings {
    public class StringParser {
        public static readonly Dictionary<char, string> Escapes = new Dictionary<char, string> {
            {'\r', @"\r"},
            {'\n', @"\n"},
            {'\t', @"\t"},
            {'\\', @"\"}
        };

        public static string ParseLiteral(string input) {
            var result = new StringBuilder(input.Length);
            for (var i = 0; i < input.Length; ++i) {
                var c = input[i];
                if (c != '\\') {
                    result.Append(c);
                    continue;
                }
                if (++i >= input.Length) {
                    result.Append(c);
                    break;
                }
                c = input[i];
                switch (c) {
                    case 'n':
                        result.Append('\n');
                        break;
                    case 'r':
                        result.Append('\r');
                        break;
                    case 't':
                        result.Append('\t');
                        break;
                    case '\\':
                        result.Append('\\');
                        break;
                    default:
                        result.Append('\\').Append(c);
                        break;
                }
            }
            return result.ToString();
        }

        public static string EscapeLiteral(string input) {
            var result = new StringBuilder(input.Length + 10);
            foreach (var c in input) {
                if (Escapes.ContainsKey(c)) {
                    result.Append(Escapes[c]);
                } else {
                    result.Append(c);
                }
            }
            return result.ToString();
        }
    }
}
