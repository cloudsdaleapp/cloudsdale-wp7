using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.lib.Providers
{
    public interface IUserProvider
    {
        User GetUser(string userId);
    }

    class DefaultUserProvider : IUserProvider
    {
        public User GetUser(string userId)
        {
            return null;
        }
    }
}