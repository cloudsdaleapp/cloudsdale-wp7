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
    /// Interaction logic for ClientNote.xaml
    /// </summary>
    public partial class ClientNote : Page
    {
        public ClientNote(Message message)
        {
            InitializeComponent();
            Block.Text = message.Content;
        }
    }
}
