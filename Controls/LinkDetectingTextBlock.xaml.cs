using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
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
            InitializeComponent();
            RootBlock.DataContext = this;
        }

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

            var matches = Helpers.LinkRegex.Matches(args.NewText);
            var lastIndex = 0;
            RootBlock.Blocks.Clear();
            var block = new Paragraph { FontSize = FontSize, FontFamily = FontFamily };
            foreach (Match match in matches) {
                var link = match.Value;
                var nonlink = args.NewText.Substring(lastIndex, match.Index - lastIndex);
                foreach (var inline in Cursive(nonlink)) {
                    block.Inlines.Add(inline);
                }
                var hyperlink = new Hyperlink {
                    Inlines = { new Run { Text = link } },
                    Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x88, 0xCC)),
                    MouseOverForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x66, 0x99)),
                };
                hyperlink.Click += (sender, eventArgs) => {
                    if (LinkClicked != null) LinkClicked(this, new LinkClickedEventArgs { LinkValue = link });
                };
                block.Inlines.Add(hyperlink);
                lastIndex = match.Index + match.Length;
            }
            var lastnonlink = args.NewText.Substring(lastIndex);
            foreach (var inline in Cursive(lastnonlink)) {
                block.Inlines.Add(inline);
            }
            RootBlock.Blocks.Add(block);
        }

        protected virtual IEnumerable<Inline> Cursive(string input) {
            var matches = Helpers.CursiveRegex.Matches(input);
            var lastIndex = 0;

            foreach (Match match in matches) {
                yield return new Run { Text = input.Substring(lastIndex, match.Index - lastIndex) };
                yield return new Italic { Inlines = { new Run { Text = match.Value.Trim('/') } } };
                lastIndex = match.Index + match.Length;
            }

            yield return new Run { Text = input.Substring(lastIndex) };
        }
    }
}
