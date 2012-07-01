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

        private readonly Regex _backslashN = new Regex("([^\\\\]|^)\\\\n");
        private readonly Regex _backslashT = new Regex("([^\\\\]|^)\\\\t");
        public ChatLine[] Split {
            get {
                try {

                    content = _backslashN.Replace(content, "\n");
                    var split = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var lines = new ChatLine[split.Length];
                    for (var i = 0; i < split.Length; ++i) {
                        split[i] = split[i].Trim();
                        split[i] = _backslashN.Replace(split[i], "\n");
                        split[i] = _backslashT.Replace(split[i], "    ");
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
