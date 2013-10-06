using CloudsdaleWin7.lib.Providers;

namespace CloudsdaleWin7.lib.CloudsdaleLib
{
    /// <summary>
    /// Provides static access to all the providers of cloudsdale
    /// </summary>
    public class Cloudsdale
    {
        private static readonly ISessionProvider DefaultSessionProvider = new DefaultSessionProvider();
        private static readonly ICloudServicesProvider DefaultCloudServicesProvider = new DefaultCloudServicesProvider();
        private static readonly IModelErrorProvider DefaultModelErrorProvider = new DefaultModelErrorProvider();
        private static readonly IUserProvider DefaultUserProvider = new DefaultUserProvider();
        private static readonly ICloudProvider DefaultCloudProvider = new DefaultCloudProvider();

        private static ISessionProvider _sessionProvider;
        private static ICloudServicesProvider _cloudServicesProvider;
        private static IModelErrorProvider _modelErrorProvider;
        private static IMetadataProviderStore _metadataProviders = new MetadataProviderStore();
        private static IUserProvider _userProvider;
        private static ICloudProvider _cloudProvider;

        public static ISessionProvider SessionProvider
        {
            get { return _sessionProvider ?? DefaultSessionProvider; }
            set { _sessionProvider = value; }
        }

        public static ICloudServicesProvider CloudServicesProvider
        {
            get { return _cloudServicesProvider ?? DefaultCloudServicesProvider; }
            set { _cloudServicesProvider = value; }
        }

        public static IModelErrorProvider ModelErrorProvider
        {
            get { return _modelErrorProvider ?? DefaultModelErrorProvider; }
            set { _modelErrorProvider = value; }
        }

        public static IMetadataProviderStore MetadataProviders
        {
            get { return _metadataProviders; }
            set { _metadataProviders = value; }
        }

        public static IUserProvider UserProvider
        {
            get { return _userProvider ?? DefaultUserProvider; }
            set { _userProvider = value; }
        }

        public static ICloudProvider CloudProvider
        {
            get { return _cloudProvider ?? DefaultCloudProvider; }
            set { _cloudProvider = value; }
        }
    }
}
