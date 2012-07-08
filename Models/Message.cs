using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Cloudsdale.Models {
    public class Message : CloudsdaleItem {
        public Message() {
            subs = new List<Message>();
        }

        public DateTime timestamp { get; set; }
        public string content { get; set; }

        public DateTime CorrectedTimestamp {
            get { 
                var tzi = TimeZoneInfo.Local;
                var offset = tzi.BaseUtcOffset + TimeSpan.FromHours(1);
                return timestamp + offset;
            }
        }

        public SimpleUser user { get; set; }
        public Topic topic;

        internal readonly List<Message> subs;

        private static readonly Regex _backslashNLB = new Regex("^\\\\n");
        private static readonly Regex _backslashN = new Regex("([^\\\\])\\\\n");
        private static readonly Regex _backslashTLB = new Regex("^\\\\t");
        private static readonly Regex _backslashT = new Regex("([^\\\\])\\\\t");
        public ChatLine[] Split {
            get {
                try {
                    string message;
                    if (subs.Count < 1 || subs[0].timestamp > timestamp) {
                        message = content;
                        foreach (var msg in subs) {
                            message += '\n' + msg.content;
                        }
                    } else {
                        var inserted = false;
                        message = subs[0].content;
                        for (var i = 1; i < subs.Count; ++i) {
                            message += '\n' + subs[i].content;
                            if (!inserted && timestamp > subs[i].timestamp) {
                                message += '\n' + content;
                                inserted = true;
                            }
                        }
                    }
                    message = _backslashNLB.Replace(message, "\n");
                    message = _backslashN.Replace(message, match => match.Value[0] + "\n");
                    var split = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
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

        public void AddSub(Message item) {
            foreach (var sub in subs) {
                if (sub.id == item.id) return;
            }
            var greatest = 0;
            while (greatest < subs.Count && subs[greatest].timestamp < item.timestamp) greatest++;
            subs.Insert(greatest, item);
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
