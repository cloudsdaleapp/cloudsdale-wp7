using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using CloudsdaleWin7.lib.Helpers;

namespace CloudsdaleWin7.lib.CloudsdaleLib {
    public static class CloudsdaleHelpers {
        private static readonly char[] SpecChars = { '\n', '\r', '\t', '\b', '\\' };

        public unsafe static string UnescapeLiteral(this string input) {
            if (input.Length > 1000) {
                return input.Truncate(999).UnescapeLiteral();
            }
            var i = 0;
            var k = 0;
            var n = input.Length;
            fixed (char* x = input) {
                char* y = stackalloc char[n];
                for (; i < n; ++i, ++k) {
                    if (x[i] == '\\' && ++i < n) {
                        y[k] = SpecChar(x[i]);
                    } else {
                        y[k] = x[i];
                    }
                }
                return new string(y, 0, k);
            }
        }

        public unsafe static string EscapeLiteral(this string input) {
            int i = 0, k = 0, n = input.Length;
            fixed (char* x = input) {
                char* y = stackalloc char[n * 2];
                for (; i < n; ++i, ++k) {
                    if (SpecChars.Contains(x[i])) {
                        y[k] = '\\';
                        y[++k] = EscChar(x[i]);
                    } else {
                        y[k] = x[i];
                    }
                }

                return new string(y, 0, k);
            }
        }

        public static string Truncate(this string input, int length) {
            if (string.IsNullOrWhiteSpace(input)) return "";
            return input.Length > length ? input.Substring(0, length) : input;
        }

        static char SpecChar(char c) {
            switch (c) {
                case 'n':
                    return '\n';
                case 't':
                    return '\t';
                case 'r':
                    return '\r';
                case 'b':
                    return '\b';
                default:
                    return c;
            }
        }
        static char EscChar(char c) {
            switch (c) {
                case '\n':
                    return 'n';
                case '\t':
                    return 't';
                case '\r':
                    return 'r';
                case '\b':
                    return 'b';
                default:
                    return c;
            }
        }

        public static readonly Regex LinkRegex = new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string RegexReplace(this string input, string regex, string replacement) {
            return new Regex(regex).Replace(input, replacement);
        }
        public static string MultiReplace(this string input, string r1, string r2, string replacement)
        {
            return input.Replace(r1, replacement).Replace(r2, replacement);
        }
        public static string MultiReplace(this string input, string r1, string r2, string r3, string replacement)
        {
            return input.Replace(r1, replacement).Replace(r2, replacement).Replace(r3, replacement);
        }
        public static Hyperlink OnClickLaunch(this Hyperlink link, string uri)
        {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                uri = "http://" + uri;
            }

            link.Click += delegate
            {
                if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                {
                    MessageBox.Show(uri + " is not a well formed link! Please try another.");
                }
                BrowserHelper.FollowLink(uri);
            };
            return link;
        }
        public static string AssuredLink(this string link)
        {
            if (!Uri.IsWellFormedUriString(link, UriKind.Absolute))
            {
                link = "http://" + link;
            }
            return link;
        }
    }
}
