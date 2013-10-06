using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.Views.Notifications
{
    /// <summary>
    /// Interaction logic for UserNote.xaml
    /// </summary>
    public partial class UserNote : Page
    {
        public UserNote(Message message)
        {
            InitializeComponent();
            if (!message.Author.IsSubscribed) return;
        }
    }
}
