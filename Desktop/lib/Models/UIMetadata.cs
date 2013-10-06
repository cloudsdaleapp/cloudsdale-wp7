using System;
using System.Collections.Generic;
using CloudsdaleWin7.lib.Providers;
using CloudsdaleWin7.lib.CloudsdaleLib;

namespace CloudsdaleWin7.lib.Models
{
    public class UIMetadata
    {
        private readonly Dictionary<string, IMetadataObject> objects = new Dictionary<string, IMetadataObject>();
        private readonly CloudsdaleModel model;
        public UIMetadata(CloudsdaleModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// The metadata object for this provider key
        /// </summary>
        /// <param name="key">Metadata provider key</param>
        /// <returns>A metadata object</returns>
        public IMetadataObject this[string key]
        {
            get
            {
                return objects.ContainsKey(key)
                           ? objects[key]
                           : objects[key] = Cloudsdale.MetadataProviders[key].CreateNew(model);
            }
        }
    }
}
