using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudsdaleWin7.Exceptions
{
    class NotInstalledCorrectly : DllNotFoundException
    {
        public override string HelpLink
        {
            get { return "http://www.cloudsdale.org/clouds/ask-the-staff"; }
            set
            {
                base.HelpLink = value;
            }
        }
        public override string Message
        {
            get { return "It appears either Cloudsdale wasn't installed correctly. Try reinstalling the fix the error. If this error persists, visit " + HelpLink; }
        }
    }
}
