using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CloudsdaleWin7.lib;
using CloudsdaleWin7.lib.CloudsdaleLib;
using CloudsdaleWin7.lib.Models;

namespace CloudsdaleWin7.lib.Providers
{
    /// <summary>
    /// Provides metadata objects to models
    /// </summary>
    public interface IMetadataProvider
    {
        IMetadataObject CreateNew(CloudsdaleModel model);
    }

    /// <summary>
    /// Stores a metadata value
    /// </summary>
    public interface IMetadataObject
    {
        object Value { get; set; }
        CloudsdaleModel Model { get; }
    }

    /// <summary>
    /// Provides access to the metadata providers
    /// </summary>
    public interface IMetadataProviderStore
    {
        IMetadataProvider this[string key] { get; set; }
    }

    internal class MetadataProviderStore : IMetadataProviderStore
    {
        private readonly Dictionary<string, IMetadataProvider> providers = new Dictionary<string, IMetadataProvider>();

        public IMetadataProvider this[string key]
        {
            get
            {
                return providers.ContainsKey(key) ? providers[key] : providers[key] = new DefaultMetadataProvider();
            }
            set { providers[key] = value; }
        }
    }

    internal class DefaultMetadataProvider : IMetadataProvider
    {
        public IMetadataObject CreateNew(CloudsdaleModel model)
        {
            return new DefaultMetadataObject(model);
        }
    }

    internal class DefaultMetadataObject : IMetadataObject, INotifyPropertyChanged
    {
        public DefaultMetadataObject(CloudsdaleModel model)
        {
            Model = model;
        }

        private object _value;
        public object Value
        {
            get { return _value; }
            set
            {
                if (Equals(value, _value)) return;
                _value = value;
                OnPropertyChanged();
            }
        }

        public CloudsdaleModel Model { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
