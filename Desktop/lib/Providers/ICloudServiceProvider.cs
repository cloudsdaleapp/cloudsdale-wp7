using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.lib.Providers
{
    public interface ICloudServicesProvider
    {
        IStatusProvider StatusProvider(string cloudId);
        User GetBackedUser(string userId);
    }

    internal class DefaultCloudServicesProvider : ICloudServicesProvider
    {
        private static readonly DefaultStatusProvider DefaultStatusProvider = new DefaultStatusProvider();

        public IStatusProvider StatusProvider(string cloudId)
        {
            return DefaultStatusProvider;
        }

        public User GetBackedUser(string userId)
        {
            return new User(userId);
        }
    }
}
