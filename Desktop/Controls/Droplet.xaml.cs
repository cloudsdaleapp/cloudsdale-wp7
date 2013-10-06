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
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.Controls
{
    /// <summary>
    /// Interaction logic for Droplet.xaml
    /// </summary>
    public partial class Droplet
    {
        new public Drop Drop { get; set; }
        public Droplet(Drop drop)
        {
            InitializeComponent();
            Drop = drop;
            DropPreview.Source = new BitmapImage(drop.Preview);
            DropPreview.ToolTip = drop.Url;
            DropTitle.Text = drop.Title;
        }

        private void Launch(object sender, MouseButtonEventArgs e)
        {
            BrowserHelper.FollowLink(Drop.Url.ToString());
        }
    }
}
