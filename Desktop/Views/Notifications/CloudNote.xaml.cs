using System.Windows.Controls;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.Views.Notifications
{
    /// <summary>
    /// Interaction logic for CloudNote.xaml
    /// </summary>
    public partial class CloudNote : Page
    {
        public CloudNote(Message message)
        {
            InitializeComponent();
            NoteTitle.Text = App.Connection.MessageController.CloudControllers[message.PostedOn].Cloud.Name + "-";
            NoteContent.Text = "[" + message.FinalTimestamp + "] @" + message.Author.Username + ": " + message.Content;
        }
    }
}
