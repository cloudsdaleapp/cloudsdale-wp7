using System.Text;

namespace Cloudsdale.Settings {
    public class StringParser {
        public static string ParseLiteral(string input) {
            var result = new StringBuilder(input.Length);
            char c;
            for (var i = 0; i < input.Length; ++i) {
                c = input[i];
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
                }
            }
            return result.ToString();
        }
    }
}
