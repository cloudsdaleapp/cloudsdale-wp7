using System;
using CloudsdaleWin7.lib.Providers;

namespace CloudsdaleWin7.lib.Models.Client
{
    public class CloudControllerMetadataProvider : IMetadataProvider
    {
        public IMetadataObject CreateNew(CloudsdaleModel model)
        {
            return new CloudControllerMetadata(model);
        }

        public class CloudControllerMetadata : IMetadataObject
        {
            public CloudControllerMetadata(CloudsdaleModel model)
            {
                Model = model;
            }

            public object Value
            {
                get { return App.Connection.MessageController[(Cloud)Model]; }
                set { throw new InvalidOperationException("Cannot set a cloud controller to an instance of a cloud!"); }
            }
            public CloudsdaleModel Model { get; private set; }
        }
    }
}
