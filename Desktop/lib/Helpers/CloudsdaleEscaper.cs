using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudsdaleWin7.lib.Helpers
{
    public static class CloudsdaleEscaper
    {
        private static readonly Dictionary<char, string> EscapeMappings = new Dictionary<char, string> {
            {'\\', "\\\\"}, {'\n', "\\n"}, {'\r', ""}, {'\t', "\\t"}
        };
        private static readonly Dictionary<char, char> ParseMappings = new Dictionary<char, char> {
            {'\\', '\\'}, {'n', '\n'}, {'r', '\r'}, {'t', '\t'}
        };

        /// <summary>
        /// Escape a message before it is sent to the server
        /// </summary>
        /// <param name="message">Message to escape</param>
        /// <returns>Escaped version of message</returns>
        public static string EscapeMessage(this string message)
        {
            return EscapeMappings.Aggregate(message, (current, mapping) =>
                current.Replace(mapping.Key.ToString(), mapping.Value));
        }

        /// <summary>
        /// Parses a message sent down from the server
        /// </summary>
        /// <param name="message">Message to parse</param>
        /// <returns>Parsed message</returns>
        public static string ParseMessage(this string message)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < message.Length; ++i)
            {
                if (message[i] == '\\')
                {
                    if (++i < message.Length)
                    {
                        if (ParseMappings.ContainsKey(message[i]))
                        {
                            builder.Append(ParseMappings[message[i]]);
                        }
                        else
                        {
                            builder.Append('\\').Append(message[i]);
                        }
                    }
                    else
                    {
                        builder.Append('\\');
                    }
                }
                else
                {
                    builder.Append(message[i]);
                }
            }

            return builder.ToString();
        }
    }
}
