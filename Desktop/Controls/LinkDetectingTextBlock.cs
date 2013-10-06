using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using CloudsdaleWin7.lib.CloudsdaleLib;

namespace CloudsdaleWin7.Controls {

    public delegate void TextChangedEventHandler(TextChangedEventArgs args);
    public class TextChangedEventArgs : EventArgs {
        public string NewText { get; set; }
        public string OldText { get; set; }
    }

    public class LinkDetectingTextBlock : TextBlock {
        static LinkDetectingTextBlock() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkDetectingTextBlock), new FrameworkPropertyMetadata(typeof(LinkDetectingTextBlock)));
        }
        public static DependencyProperty LinkedTextProperty = DependencyProperty.Register(
            "LinkedText", typeof (string), typeof (LinkDetectingTextBlock),
            new PropertyMetadata("", PropertyOnLinkedTextChanged));
        private static void PropertyOnLinkedTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            ((LinkDetectingTextBlock)dependencyObject).OnLinkedTextChange(
                new TextChangedEventArgs {
                    NewText = (string) dependencyPropertyChangedEventArgs.NewValue,
                    OldText = (string) dependencyPropertyChangedEventArgs.OldValue,
                });
        }

        public TextChangedEventHandler LinkedTextChanged;

        public string LinkedText {
            get { return (string) GetValue(LinkedTextProperty); }
            set { SetValue(LinkedTextProperty, value); }
        }

        protected virtual void OnLinkedTextChange(TextChangedEventArgs args) {
            if (LinkedTextChanged != null) LinkedTextChanged(args);

            var matches = CloudsdaleHelpers.LinkRegex.Matches(args.NewText);
            var lastIndex = 0;
            Inlines.Clear();
            foreach (Match match in matches) {
                Inlines.Add(new Run(args.NewText.Substring(lastIndex, match.Index - lastIndex)));
                //var hyperlink = new Hyperlink(new Run(match.Value));
                //var link = match.Value;
                //hyperlink.Click += (sender, eventArgs) => {
                //    if (match.ToString().Contains("www.cloudsdale.org/clouds/"))
                //    {
                //        //Adds the clicked cloud link to the cloud list and subscribes the user to the channel.
                //        var shortLink = link.StartsWith("http://") ? link.Split('/')[4] : link.Split('/')[3];
                //        RestateMap.RestateCloudList(shortLink);

                //    }else
                //    {
                //        MainWindow.Instance.CloudList.SelectedIndex = -1;
                //        MainWindow.Instance.Frame.Navigate(new Browser());
                //        Browser.Instance.Width = MainWindow.Instance.Width;
                //        Browser.Instance.WebBrowser.Navigate(link.StartsWith("http://") ? link : "http://" + link);
                //        Browser.Instance.WebAddress.Text = link;
                //    }
                    
                //};
                //Inlines.Add(hyperlink);
                lastIndex = match.Index + match.Length;
            }
            Inlines.Add(new Run(args.NewText.Substring(lastIndex)));
        }
    }
}
