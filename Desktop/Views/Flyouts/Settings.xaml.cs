using System;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.Views.Flyouts
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        private readonly Session _current = App.Connection.SessionController.CurrentSession;
        private readonly Regex _nameRegex = new Regex(@"\b\s[a-z]\b", RegexOptions.IgnoreCase);
        private readonly Regex _usernameRegex = new Regex(@"\b[a-z0-9_]\b", RegexOptions.IgnoreCase);

        public Settings()
        {
            InitializeComponent();
            NameBlock.Text = _current.Name;
            UsernameBlock.Text = _current.Username;
            CheckChanges();
            SkypeBlock.Text = _current.SkypeName;
            AvatarImage.Source = new BitmapImage(_current.Avatar.Normal);
            Status.SelectedItem = _current.Status;
        }
        private void CheckChanges()
        {
            if (_current.CanChangeName()) return;
            UsernameBlock.IsReadOnly = true;
        }

        private void ChangeName(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (!_nameRegex.IsMatch(NameBlock.Text))
            {
                NameBlock.Text = _current.Name;
                return;
            }
           App.Connection.SessionController.PostData("name", NameBlock.Text);
        }
    }
}
