using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Text.RegularExpressions;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Cloudsdale.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Message : CloudsdaleItem {
        public Message() {
            subs = new List<Message>();
        }
        [JsonProperty]
        public DateTime timestamp { get; set; }
        [JsonProperty]
        public string content { get; set; }

        public DateTime CorrectedTimestamp {
            get { 
                var tzi = TimeZoneInfo.Local;
                var offset = tzi.BaseUtcOffset + TimeSpan.FromHours(1);
                return timestamp + offset;
            }
        }

        [JsonProperty]
        public SimpleUser user { get; set; }
        [JsonProperty]
        public Topic topic;

        internal readonly List<Message> subs;

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
                    message = Settings.StringParser.ParseLiteral(message);
                    var split = message.Split('\n');
                    var lines = new ChatLine[split.Length];
                    for (var i = 0; i < split.Length; ++i) {
                        if (string.IsNullOrWhiteSpace(split[i])) split[i] = " ";
                        lines[i] = new ChatLine {
                            Text = split[i].StartsWith("/me ") ? '*' + split[i].Substring(4).Trim() + '*' : split[i].Trim(),
                            Color = new SolidColorBrush(split[i].StartsWith(">") ?
                                Color.FromArgb(0xFF, 0x32, 0x82, 0x32) : 
                                split[i].StartsWith("/me ") ? Colors.Purple : Colors.Black)
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
