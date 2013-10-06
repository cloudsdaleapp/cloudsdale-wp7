using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Cloudsdale_Metro.Helpers;

namespace Cloudsdale_Metro.Views.Controls {
    public sealed partial class MessageTextControl {
        private static readonly Regex GreentextRegex = new Regex(@"^\>");
        private static readonly Regex LinkRegex = new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", RegexOptions.IgnoreCase);
        private static readonly Regex ItalicsRegex = new Regex(@"\B\/\b([^\/\n]+)\b\/\B");
        private static readonly Regex RedactedRegex = new Regex(@"\[REDACTED\]", RegexOptions.IgnoreCase);
        private static readonly Regex NonAsciiRegex = new Regex(@"[^\x00-\xFF]+");
        private static readonly Regex ExtraSpacesRegex = new Regex("[ ]+");

        public MessageTextControl() {
            InitializeComponent();
            _parsers =
                new List<Func<string, IEnumerable<TextGroup>>> {
                    Processor(LinkRegex, link => new Hyperlink {
                        FontSize = FontSize,
                        Inlines = { new Run { Text = link } }
                    }.OnClickLaunch(link)),
                    Processor(ItalicsRegex, italics => new Italic {
                        Inlines = { new Run { Text = italics.Substring(1, italics.Length - 2) } }
                    }),
                    Processor(RedactedRegex, redacted => new Bold {
                        Foreground = new SolidColorBrush(Colors.Red),
                        Inlines = { new Run { Text = "[REDACTED]" } }
                    }),
                    Processor(NonAsciiRegex, nonascii => new Run {
                        Text = nonascii,
                        FontFamily = new FontFamily("Segoe UI")
                    })
                };
        }

        #region Messages Property

        private static readonly DependencyProperty MessagesProperty = DependencyProperty.Register("Messages",
            typeof(string[]), typeof(MessageTextControl),
            new PropertyMetadata(default(string[]), MessagesChanged));

        private static void MessagesChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var control = (MessageTextControl)dependencyObject;

            control.UpdateContents();
        }

        public string[] Messages {
            private get { return (string[])GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }
        #endregion

        #region Prefix Inline Property

        private static readonly DependencyProperty PrefixInlineProperty =
            DependencyProperty.Register("PrefixInline", typeof(Inline),
            typeof(MessageTextControl), new PropertyMetadata(default(Inline)));

        public Inline PrefixInline {
            private get { return (Inline)GetValue(PrefixInlineProperty); }
            set { SetValue(PrefixInlineProperty, value); }
        }
        #endregion

        #region Linebreak Handling Property

        private static readonly DependencyProperty LinebreakHandlingProperty =
            DependencyProperty.Register("LinebreakHandling", typeof(LinebreakHandling), typeof(MessageTextControl), new PropertyMetadata(LinebreakHandling.Mimic));

        public LinebreakHandling LinebreakHandling {
            private get { return (LinebreakHandling)GetValue(LinebreakHandlingProperty); }
            set { SetValue(LinebreakHandlingProperty, value); }
        }
        #endregion

        #region Text Alignment

        private static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(MessageTextControl),
            new PropertyMetadata(TextAlignment.Left, AlignmentChanged));

        private static void AlignmentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) {
            var control = (MessageTextControl)dependencyObject;
            control.RichText.TextAlignment = (TextAlignment)args.NewValue;
        }

        public TextAlignment TextAlignment {
// ReSharper disable UnusedMember.Global
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
// ReSharper restore UnusedMember.Global
        }

        #endregion

        #region Update
        private void UpdateContents() {
            RichText.Blocks.Clear();
            foreach (var message in Messages) {
                var block = new Paragraph {
                    Margin = new Thickness(5),
                };
                foreach (var inline in ParseMessage(message)) {
                    block.Inlines.Add(inline);
                }
                RichText.Blocks.Add(block);
            }
        }
        #endregion

        #region Message Parsing

        private readonly List<Func<string, IEnumerable<TextGroup>>> _parsers;

        private IEnumerable<Inline> ParseMessage(string message) {
            var first = true;
            foreach (var line in message.Split('\n').Select(rawline => ExtraSpacesRegex.Replace(rawline, " "))) {
                if (first && PrefixInline != null) {
                    yield return PrefixInline;
                }
                if (!first && LinebreakHandling == LinebreakHandling.Mimic) {
                    yield return new LineBreak();
                }

                var span = new Span {
                    Foreground = new SolidColorBrush(
                        GreentextRegex.IsMatch(line)
                            ? Colors.MediumSeaGreen
                            : Colors.Black)
                };
                foreach (var inline in BuildLine(line)) {
                    span.Inlines.Add(inline);
                }
                yield return span;

                first = false;
            }
        }

        private IEnumerable<Inline> BuildLine(string line) {
            var groups = new List<TextGroup> { new TextGroup { Text = line } };

            foreach (var processor in _parsers) {
                var nextList = new List<TextGroup>();
                foreach (var item in groups) {
                    if (item.Inline != null) {
                        nextList.Add(item);
                    } else {
                        nextList.AddRange(processor(item.Text));
                    }
                }
                groups = nextList;
            }

            return groups.Select(item => item.Inline ?? new Run { Text = item.Text });
        }

        private static Func<string, IEnumerable<TextGroup>> Processor(Regex matcher, Func<string, Inline> processor) {
            return input => InternalProcessor(matcher, processor, input);
        }

        private static IEnumerable<TextGroup> InternalProcessor(Regex matcher, Func<string, Inline> processor, string input) {
            var matches = matcher.Matches(input);
            var lastIndex = 0;

            foreach (Match match in matches) {
                yield return new TextGroup { Text = input.Substring(lastIndex, match.Index - lastIndex) };
                yield return new TextGroup { Inline = processor(match.Value) };
                lastIndex = match.Index + match.Length;
            }

            yield return new TextGroup { Text = input.Substring(lastIndex) };
        }
        #endregion
    }

    public class TextGroup {
        public string Text;
        public Inline Inline;
    }

    public enum LinebreakHandling {
        Mimic, Ignore
    }
}
