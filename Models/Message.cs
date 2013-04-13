using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.ComponentModel;
using System.Text;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Linq;

namespace Cloudsdale.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class Message : CloudsdaleItem, IComparable<Message>, INotifyPropertyChanged {
        public Message() {
            subs = new List<Message>();
        }
        [JsonProperty]
        public DateTime timestamp { get; set; }
        [JsonProperty]
        public string content { get; set; }
        [JsonProperty]
        public Drop[] drops;

        [JsonProperty]
        public string device;

        public string DeviceStub {
            get {
                switch (device) {
                    case "mobile":
                        return "📱";
                    case "robot":
                        return "⚙";
                    default:
                        return "";
                }
            }
        }

        public string CorrectedTimestamp {
            get {
                var datestring = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                var time = (subs.Any() ? subs.Last() : this).timestamp;
                return time > DateTime.Now.AddDays(-1) ? time.ToString("HH:mm:ss") :
                    time.ToString(datestring + " HH:mm:ss");
            }
        }

        [JsonProperty("author")]
        public SimpleUser user { get; set; }
        [JsonProperty]
        public Topic topic;

        [JsonProperty]
        public string client_id;

        internal readonly List<Message> subs;

        public ChatLine[] Split {
            get {
                try {
                    var message = subs.Aggregate(new StringBuilder(content.Trim()), (builder, msg) => 
                        builder.Append('\n').Append(msg.content.Trim())).ToString();
                    message = Settings.StringParser.ParseLiteral(message);
                    var split = message.Replace("\r\n", "\n").Split(new[] { '\n', '\r' });
                    var lines = new ChatLine[split.Length];
                    for (var i = 0; i < split.Length; ++i) {
                        if (string.IsNullOrWhiteSpace(split[i])) split[i] = " ";
                        lines[i] = new ChatLine {
                            Text = split[i].Trim(),
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

        public bool IsSlashMe {
            get { return content.StartsWith("/me"); }
        }
        public string SlashMeForm {
            get { return user.name + content.Substring(3); }
        }

        public Drop[] Drops {
            get {
                return subs.Select(sub => sub.drops)
                    .Aggregate((IEnumerable<Drop>)(drops ?? new Drop[0]),
                    (all, item) => all.Concat(item ?? new Drop[0])).ToArray();
            }
        }

        public void AddSub(Message item) {
            if (subs.Any(sub => sub.id == item.id)) return;
            subs.Add(item);
            OnPropertyChanged("Split");
            OnPropertyChanged("Drops");
            OnPropertyChanged("CorrectedTimestamp");
        }

        public int CompareTo(Message other) {
            return timestamp.CompareTo(other.timestamp);
        }

        public static bool operator >(Message m1, Message m2) {
            return m1.timestamp > m2.timestamp;
        }

        public static bool operator <(Message m1, Message m2) {
            return m1.timestamp < m2.timestamp;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
