using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CloudsdaleWin7 {
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        private string Name = App.Connection.SessionController.CurrentSession.Name;
        public static Home Instance;
        public Home()
        {
            InitializeComponent();
            Instance = this;
            Welcome.Text = WelcomeMessage(Name);
            Animate();
        }
        private static string WelcomeMessage(string name)
        {
            var r = new Random();
            String message;
            switch(r.Next(0,3))
            {
                case 0:
                    message = "Hi, [:name]!";
                    break;
                case 1:
                    message = "Welcome, [:name]!";
                    break;
                case 2:
                    message = "Welcome back, [:name].";
                    break;
                default:
                    message = "Hi there, [:name]!";
                    break;
            }
            return message.Replace("[:name]", name);
        }
        private void Animate()
        {
            #region Welcome Message
            var board = new Storyboard();
            var animation = new DoubleAnimation(0, 100, new Duration(new TimeSpan(2000000000)));
            board.Children.Add(animation);
            Storyboard.SetTargetName(animation, Welcome.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            animation.EasingFunction = new ExponentialEase();
            board.Begin(this);

            var b1 = new Storyboard();
            var a1 = new DoubleAnimation(0, 450, new Duration(new TimeSpan(0,0,1)), FillBehavior.HoldEnd);
            b1.Children.Add(a1);
            Storyboard.SetTargetName(a1, line1.Name);
            Storyboard.SetTargetProperty(a1, new PropertyPath(WidthProperty));
            a1.EasingFunction = new ExponentialEase();
            b1.Begin(this);

            #endregion
        }
    }
}
