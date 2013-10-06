using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CloudsdaleWin7.lib;
using CloudsdaleWin7.lib.CloudsdaleLib;
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.MVVM
{
    class SlashMe : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && parameter.ToString() == "Inverse")
                return value.ToString().StartsWith("/me ") ? Visibility.Collapsed : Visibility.Visible;
            return value.ToString().StartsWith("/me ") ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class ContentConverter : IValueConverter
    {
        private static readonly Regex LinkRegex = new Regex(@"(?i)\b((?:[a-z][\w-]+:(?:/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))", RegexOptions.IgnoreCase);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var message = (Message) value;
            var rGraph = new Paragraph();

            #region slashme

            if (message.Content.StartsWith("/me "))
                message.Content = message.Content.Replace("/me", message.Author.Name);

            #endregion

            foreach (var word in message.Content.Split(' '))
            {
                rGraph = ProcessContent(word, rGraph);
            }

            return new FlowDocument(rGraph);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static Paragraph ProcessContent(string word, Paragraph rGraph)
        {
            #region italics

            if (word.StartsWith("/") && word.EndsWith("/") && word != "//")
            {
                rGraph.Inlines.Add(new Italic(new Run(word.Replace("/", "") + " ")));
            }
            #endregion
            #region redacted
            else if (word.ToLower() == "[redacted]")
            {
                var redacted = new Run("[REDACTED] ");
                redacted.Foreground = new SolidColorBrush(Colors.Red);
                rGraph.Inlines.Add(new Bold(redacted));
            }
            #endregion
            #region link
            else if (LinkRegex.IsMatch(word))
            {
                var newLink = new Hyperlink();
                newLink.IsEnabled = true;
                newLink.Inlines.Add(word);
                newLink.Foreground = new SolidColorBrush(Colors.Blue);
                newLink.NavigateUri = new Uri(word, UriKind.RelativeOrAbsolute);
                newLink.RequestNavigate += (sender, args) => BrowserHelper.FollowLink(args.Uri.ToString());
                newLink.Cursor = Cursors.Hand;
                rGraph.Inlines.Add(newLink);
                rGraph.Inlines.Add(" ");
            }
            #endregion
            else
            {
                rGraph.Inlines.Add(word + " ");
            }
            return rGraph;
        }
    }

    public class NameFromMessage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((User) value).Name;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Could not convert back!");
        }
    }

    public class ChatColors : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().StartsWith("/me")) return Colors.Green;
            if (value.ToString().StartsWith("//")) return Colors.CadetBlue;
            return CloudsdaleSource.PrimaryText;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CloudsdaleSource.PrimaryText;
        }
    }
}
