using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Cloudsdale.Models {
    public class Message {
        public DateTime timestamp { get; set; }
        public string content { get; set; }

        public SimpleUser user { get; set; }
        public Topic topic;

        private static readonly Regex _backslashNLB = new Regex("^\\\\n");
        private static readonly Regex _backslashN = new Regex("([^\\\\])\\\\n");
        private static readonly Regex _backslashTLB = new Regex("^\\\\t");
        private static readonly Regex _backslashT = new Regex("([^\\\\])\\\\t");
        public ChatLine[] Split {
            get {
                try {
                    content = _backslashNLB.Replace(content, "\n");
                    content = _backslashN.Replace(content, match => match.Value[0] + "\n");
                    var split = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var lines = new ChatLine[split.Length];
                    for (var i = 0; i < split.Length; ++i) {
                        split[i] = split[i].Trim();
                        split[i] = _backslashTLB.Replace(split[i], "    ");
                        split[i] = _backslashT.Replace(split[i], match => match.Value[0] + "\t");
                        split[i] = split[i].Replace("\\\\", "\\");
                        split[i] = Settings.ChatFilter.Filter(split[i]);
                        lines[i] = new ChatLine {
                            Text = split[i],
                            Color = new SolidColorBrush(split[i].StartsWith(">") ?
                                Color.FromArgb(0xFF, 0x32, 0x82, 0x32) : Colors.Black)
                        };
                    }
                    return lines;
                } catch (Exception e) {
#if DEBUG
                    Debugger.Break();
#endif
                    return new ChatLine[0];
                }
            }
        }
    }

    public class ChatLine {
        public string Text { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class WebMessageResponse {
        public Message[] result;
    }

    public class Topic {
        public string type;
        public string id;
    }
}
