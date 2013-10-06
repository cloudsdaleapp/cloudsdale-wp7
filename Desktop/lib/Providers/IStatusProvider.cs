using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.lib.Providers
{
    public interface IStatusProvider
    {
        Status StatusForUser(string userId);
    }

    internal class DefaultStatusProvider : IStatusProvider
    {
        public Status StatusForUser(string userId)
        {
            return Status.Offline;
        }
    }
}