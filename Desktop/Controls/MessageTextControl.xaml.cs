using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CloudsdaleWin7.Controls
{
    /// <summary>
    /// Interaction logic for MessageTextControl.xaml
    /// </summary>
    public sealed partial class MessageTextControl
    {
        public static readonly Regex GreentextRegex = new Regex(@"^\> ");
        public static readonly Regex OocRegex = new Regex(@"^\//");
        public static readonly Regex LinkRegex = new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", RegexOptions.IgnoreCase);
        public static readonly Regex ItalicsRegex = new Regex(@"\B\/\b([^\/\n]+)\b\/\B");
        public static readonly Regex RedactedRegex = new Regex(@"\[REDACTED\]", RegexOptions.IgnoreCase);
        public static readonly Regex NonAsciiRegex = new Regex(@"[^\x00-\xFF]+");

        public RichTextBox RichTextBlock
        {
            get { return RichText; }
        }

        public MessageTextControl()
        {
            InitializeComponent();

            Parsers =
                new List<Func<string, IEnumerable<TextGroup>>> {
                    Processor(LinkRegex, link => new BindableLink{
                        FontSize = FontSize,
                        NavigateOnClick = link,
                        Cursor = Cursors.Hand,
                        IsEnabled = true,
                        Foreground = new SolidColorBrush(Colors.Blue),
                        Inlines = { new Run { Text = link } }
                    }),
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
        public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register("Messages",
            typeof(string[]), typeof(MessageTextControl),
            new PropertyMetadata(default(string[]), MessagesChanged));


        private static void MessagesChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = (MessageTextControl)dependencyObject;


            control.UpdateContents();
        }


        public string[] Messages
        {
            get { return (string[])GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }
        #endregion


        #region Prefix Inline Property
        public static readonly DependencyProperty PrefixInlineProperty =
            DependencyProperty.Register("PrefixInline", typeof(Inline),
            typeof(MessageTextControl), new PropertyMetadata(default(Inline)));


        public Inline PrefixInline
        {
            get { return (Inline)GetValue(PrefixInlineProperty); }
            set { SetValue(PrefixInlineProperty, value); }
        }
        #endregion


        #region Linebreak Handling Property


        public static readonly DependencyProperty LinebreakHandlingProperty =
            DependencyProperty.Register("LinebreakHandling", typeof(LinebreakHandling), typeof(MessageTextControl), new PropertyMetadata(LinebreakHandling.Mimic));


        public LinebreakHandling LinebreakHandling
        {
            get { return (LinebreakHandling)GetValue(LinebreakHandlingProperty); }
            set { SetValue(LinebreakHandlingProperty, value); }
        }
        #endregion


        #region Text Alignment


        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(MessageTextControl),
            new PropertyMetadata(TextAlignment.Left, AlignmentChanged));


        private static void AlignmentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var control = (MessageTextControl)dependencyObject;
            control.RichText.Document.TextAlignment = (TextAlignment)args.NewValue;
        }


        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }


        #endregion


        #region Update
        private void UpdateContents()
        {
            RichText.Document.Blocks.Clear();
            foreach (var message in Messages)
            {
                var block = new Paragraph
                {
                    Margin = new Thickness(5),
                };
                foreach (var inline in ParseMessage(message))
                {
                    block.Inlines.Add(inline);
                }
                RichText.Document.Blocks.Add(block);
            }
        }
        #endregion


        #region Message Parsing


        public readonly List<Func<string, IEnumerable<TextGroup>>> Parsers;


        private static readonly Regex ExtraSpacesRegex = new Regex("[ ]+");


        private IEnumerable<Inline> ParseMessage(string message)
        {
            var first = true;
            foreach (var line in message.Split('\n').Select(rawline => ExtraSpacesRegex.Replace(rawline, " ")))
            {
                if (first && PrefixInline != null)
                {
                    yield return PrefixInline;
                }
                if (!first && LinebreakHandling == LinebreakHandling.Mimic)
                {
                    yield return new LineBreak();
                }

                Color lineColor;
                if (GreentextRegex.IsMatch(line))
                {
                    lineColor = Colors.LimeGreen;
                }
                else if (OocRegex.IsMatch(line))
                {
                    lineColor = Colors.CadetBlue;
                }
                else
                {
                    lineColor = Colors.Black;
                }

                var span = new Span
                {
                    Foreground = new SolidColorBrush(lineColor)
                };
                foreach (var inline in BuildLine(line))
                {
                    span.Inlines.Add(inline);
                }
                yield return span;


                first = false;
            }
        }


        private IEnumerable<Inline> BuildLine(string line)
        {
            var groups = new List<TextGroup> { new TextGroup { Text = line } };


            foreach (var processor in Parsers)
            {
                var nextList = new List<TextGroup>();
                foreach (var item in groups)
                {
                    if (item.Inline != null)
                    {
                        nextList.Add(item);
                    }
                    else
                    {
                        nextList.AddRange(processor(item.Text));
                    }
                }
                groups = nextList;
            }


            return groups.Select(item => item.Inline ?? new Run { Text = item.Text });
        }


        public Func<string, IEnumerable<TextGroup>> Processor(Regex matcher, Func<string, Inline> processor)
        {
            return input => InternalProcessor(matcher, processor, input);
        }


        private IEnumerable<TextGroup> InternalProcessor(Regex matcher, Func<string, Inline> processor, string input)
        {
            var matches = matcher.Matches(input);
            var lastIndex = 0;


            foreach (Match match in matches)
            {
                yield return new TextGroup { Text = input.Substring(lastIndex, match.Index - lastIndex) };
                yield return new TextGroup { Inline = processor(match.Value) };
                lastIndex = match.Index + match.Length;
            }


            yield return new TextGroup { Text = input.Substring(lastIndex) };
        }
        #endregion
    }


    public class TextGroup
    {
        public string Text;
        public Inline Inline;
    }


    public enum LinebreakHandling
    {
        Mimic, Ignore
    }

}
