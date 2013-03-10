using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using Cloudsdale.Managers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Cloudsdale.Controls {
    public delegate void TextChangedEventHandler(TextChangedEventArgs args);
    public delegate void LinkClickedEventHandler(LinkClickedEventArgs args);
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
            if (LinkedTextChanged != null) LinkedTextChanged(args);

            var matches = Helpers.LinkRegex.Matches(args.NewText);
            var lastIndex = 0;
            RootBlock.Blocks.Clear();
            var block = new Paragraph { FontSize = FontSize, FontFamily = FontFamily };
            foreach (Match match in matches) {
                var link = match.Value;
                block.Inlines.Add(new Run { Text = args.NewText.Substring(lastIndex, match.Index - lastIndex) });
                var hyperlink = new Hyperlink {
                    Inlines = { new Run { Text = link } },
                    Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x88, 0xCC)),
                    MouseOverForeground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x66, 0x99)),
                };
                hyperlink.Click += (sender, eventArgs) => {
                    if (LinkClicked != null) LinkClicked(new LinkClickedEventArgs { LinkValue = link });
                };
                block.Inlines.Add(hyperlink);
                lastIndex = match.Index + match.Length;
            }
            block.Inlines.Add(new Run { Text = args.NewText.Substring(lastIndex) });
            RootBlock.Blocks.Add(block);
        }

    }
}
