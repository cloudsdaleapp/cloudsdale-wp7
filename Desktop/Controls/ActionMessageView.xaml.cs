using System.Windows;

namespace CloudsdaleWin7.Controls
{
    /// <summary>
    /// Interaction logic for ActionMessageView.xaml
    /// </summary>
    public sealed partial class ActionMessageView
    {
        public ActionMessageView()
        {
            InitializeComponent();
        }

        private void ActionMessageView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Separator.Width = e.NewSize.Width;
        }
    }
}
