using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Cloudsdale.Controls.Effects;
using Cloudsdale.Managers;

namespace Cloudsdale.Controls {
    public delegate void TextChangedEventHandler(object sender, TextChangedEventArgs args);
    public delegate void LinkClickedEventHandler(object sender, LinkClickedEventArgs args);
    public class TextChangedEventArgs : EventArgs {
        public string NewText { get; set; }
        public string OldText { get; set; }
    }
    public class LinkClickedEventArgs : EventArgs {
        public string LinkValue { get; internal set; }
    }

    public partial class LinkDetectingTextBlock {
        public LinkDetectingTextBlock() {
            EffectHandlers = new List<EffectHandler> {
                Hyperlink, Redacted, Italics, NonAscii
            };
            InitializeComponent();
            RootBlock.DataContext = this;
        }

        public List<EffectHandler> EffectHandlers { get; set; }

        public static DependencyProperty LinkedTextProperty = DependencyProperty.
            Register("LinkedText", typeof(string), typeof(LinkDetectingTextBlock),
            new PropertyMetadata("", PropertyOnLinkedTextChanged));
        private static void PropertyOnLinkedTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ((LinkDetectingTextBlock)dependencyObject).OnLinkedTextChange(
                new TextChangedEventArgs {
                    NewText = (string)dependencyPropertyChangedEventArgs.NewValue,
                    OldText = (string)dependencyPropertyChangedEventArgs.OldValue,
                });
        }

        public event TextChangedEventHandler LinkedTextChanged;
        public event LinkClickedEventHandler LinkClicked;

        public string LinkedText {
            get { return (string)GetValue(LinkedTextProperty); }
            set { SetValue(LinkedTextProperty, value); }
        }

        protected virtual void OnLinkedTextChange(TextChangedEventArgs args) {
            args.NewText = new Regex(@"([ ]+)").Replace(args.NewText, " ");

            if (LinkedTextChanged != null) LinkedTextChanged(this, args);

            RootBlock.Blocks.Clear();
            var block = new Paragraph { FontSize = FontSize, FontFamily = FontFamily };

            var groups = new List<TextGroup> { new TextGroup { Text = args.NewText } };

            foreach (var processor in EffectHandlers) {
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
            foreach (var item in groups) {
                block.Inlines.Add(item.Inline ?? new Run { Text = item.Text });
            }

            RootBlock.Blocks.Add(block);
        }

        IEnumerable<TextGroup> Hyperlink(string input) {
            var matches = Helpers.LinkRegex.Matches(input);
            var lastIndex = 0;

            foreach (Match match in matches) {
                yield return new TextGroup { Text = input.Substring(lastIndex, match.Index - lastIndex) };
                var link = match.Value;
                var hyperlink = new Hyperlink {
                    Inlines = { new Run { Text = link } },
                    Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x88, 0xcc)),
                    TextDecorations = null,
                    MouseOverForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x55, 0x80)),
                };
                hyperlink.Click += (sender, args) => LinkClicked(this, new LinkClickedEventArgs { LinkValue = link });
                yield return new TextGroup { Inline = hyperlink };
                lastIndex = match.Index + match.Length;
            }

            yield return new TextGroup { Text = input.Substring(lastIndex) };
        }

        static IEnumerable<TextGroup> Italics(string input) {
            var matches = Helpers.ItalicsRegex.Matches(input);
            var lastIndex = 0;

            foreach (Match match in matches) {
                yield return new TextGroup { Text = input.Substring(lastIndex, match.Index - lastIndex) };
                yield return new TextGroup { Inline = new Italic { Inlines = { new Run { Text = match.Value.Trim('/') } } } };
                lastIndex = match.Index + match.Length;
            }

            yield return new TextGroup { Text = input.Substring(lastIndex) };
        }

        static IEnumerable<TextGroup> Redacted(string input) {
            var matches = Helpers.RedactedRegex.Matches(input);
            var lastIndex = 0;

            foreach (Match match in matches) {
                yield return new TextGroup { Text = input.Substring(lastIndex, match.Index - lastIndex) };
                yield return new TextGroup {
                    Inline = new Bold {
                        Inlines = { new Run { Text = "[REDACTED]" } },
                        Foreground = new SolidColorBrush(Colors.Red),
                    }
                };
                lastIndex = match.Index + match.Length;
            }

            yield return new TextGroup { Text = input.Substring(lastIndex) };
        }

        static IEnumerable<TextGroup> NonAscii(string input) {
            var matches = Helpers.NonStandardCharRegex.Matches(input);
            var lastIndex = 0;

            foreach (Match match in matches) {
                yield return new TextGroup { Text = input.Substring(lastIndex, match.Index - lastIndex) };
                yield return new TextGroup {
                    Inline = new Run { FontFamily = new FontFamily("Segoe WP"), Text = match.Value }
                };
                lastIndex = match.Index + match.Length;
            }

            yield return new TextGroup { Text = input.Substring(lastIndex) };
        }
    }
}
