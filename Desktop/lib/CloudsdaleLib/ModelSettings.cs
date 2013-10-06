using System;
using System.Windows.Threading;

namespace CloudsdaleWin7.lib.CloudsdaleLib
{
    public static class ModelSettings
    {
        public static DateTime AppLastSuspended = DateTime.Now;
        public static Dispatcher Dispatcher;
    }
}
