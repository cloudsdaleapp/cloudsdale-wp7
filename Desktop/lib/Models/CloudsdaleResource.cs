using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using CloudsdaleWin7.lib.Helpers;
using CloudsdaleWin7.lib.CloudsdaleLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudsdaleWin7.lib.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CloudsdaleModel : INotifyPropertyChanged
    {
        public CloudsdaleModel()
        {
            UIMetadata = new UIMetadata(this);
        }

        /// <summary>
        /// Copies all the non-null properties of this model 
        /// marked with JsonProperty attributes to another model
        /// </summary>
        /// <param name="other"></param>
        public virtual void CopyTo(CloudsdaleModel other)
        {
            var properties = GetType().GetRuntimeProperties();
            var targetType = other.GetType();
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (attribute == null) continue;

                var value = property.GetValue(this);
                if (value != null)
                {
                    var targetProperty = targetType.GetRuntimeProperty(property.Name);
                    targetProperty.SetValue(other, value);
                }
            }
        }

        /// <summary>
        /// Metadata useful for UI display, provided by a MetadataProvider
        /// </summary>
        public UIMetadata UIMetadata { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected internal virtual void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            var handler = PropertyChanged;
            if (handler == null) return;
            if (ModelSettings.Dispatcher != null && !ModelSettings.Dispatcher.Thread.IsAlive)
            {
                ModelSettings.Dispatcher.InvokeAsync(() => handler(this, new PropertyChangedEventArgs(propertyname)),
                                                     DispatcherPriority.Normal);
            }
            else
            {
                handler(this, new PropertyChangedEventArgs(propertyname));
            }
        }
    }

    /// <summary>
    /// A cloudsdale resource identified by a unique ID,
    /// which may be able to be updated directly
    /// from a cloudsdale endpoint
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CloudsdaleResource : CloudsdaleModel
    {
        [JsonConstructor]
        public CloudsdaleResource(string id)
        {
            LastUpdated = DateTime.Now;
            Id = id;
        }

        /// <summary>
        /// The unique ID of the resource
        /// </summary>
        [JsonProperty("id")]
        public readonly string Id;

        /// <summary>
        /// A boolean value determining if the resource's data could be invalid
        /// </summary>
        public bool Invalidated
        {
            get { return LastUpdated < ModelSettings.AppLastSuspended; }
        }

        /// <summary>
        /// Determines whether the model is able to
        /// be validated and updated at this time
        /// </summary>
        /// <returns>Whether the model can be upated</returns>
        public virtual bool CanValidate()
        {
            return true;
        }

        /// <summary>
        /// Asyncronously validates the model
        /// </summary>
        /// <returns>Whether the resource is now validated</returns>
        public Task<bool> Validate()
        {
            return Validate(false);
        }
        /// <summary>
        /// Asyncronously validates the model,
        /// ignoring whether it is considered invalid
        /// </summary>
        /// <returns>Whether validation succeded</returns>
        public Task<bool> ForceValidate()
        {
            return Validate(true);
        }
        /// <summary>
        /// Asyncronously validates the model
        /// </summary>
        /// <param name="force">Whether validation state is ignored</param>
        /// <returns>Validation success</returns>
        public async Task<bool> Validate(bool force)
        {
            if (!Invalidated && !force) return true;
            if (!CanValidate()) return false;

            var modelType = GetType().GetTypeInfo();
            var attribute = modelType.GetCustomAttribute<ResourceEndpointAttribute>();

            var requestUrl = attribute.Endpoint.Replace("[:id]", Id);
            var client = new HttpClient
            {
                DefaultRequestHeaders = {
                        {"Accept", "application/json"}
                    }
            };
            var response = await ValidationRequest(client, requestUrl);

            var responseObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var responseModel = ObjectFromWebResult(responseObject).ToObject<CloudsdaleModel>();
            responseModel.CopyTo(this);
            return true;
        }

        /// <summary>
        /// A virtual method which returns the token out of
        /// the path for the web response (See: session responses)
        /// </summary>
        /// <param name="response">The full web response</param>
        /// <returns>The expected return object</returns>
        protected virtual JToken ObjectFromWebResult(JToken response)
        {
            return response["result"];
        }

        /// <summary>
        /// Performs the http method, adding in neccesary headers
        /// and other models neccesary to fully retrieve an updated
        /// model
        /// </summary>
        /// <param name="client">The httpclient to perform the request on</param>
        /// <param name="uri">The uri to request from the server</param>
        /// <returns>The response object returned from the server</returns>
        protected virtual async Task<HttpResponseMessage> ValidationRequest(HttpClient client, string uri)
        {
            if (Cloudsdale.SessionProvider.CurrentSession != null)
            {
                client.DefaultRequestHeaders.Add("X-Auth-Token", Cloudsdale.SessionProvider.CurrentSession.AuthToken);
            }

            return await client.GetAsync(uri);
        }

        /// <summary>
        /// Helper method to object the type of rest model used to update the resource
        /// </summary>
        public string RestModelType
        {
            get { return GetType().GetTypeInfo().GetCustomAttribute<ResourceEndpointAttribute>().RestModelType; }
        }

        [JsonIgnore]
        protected DateTime LastUpdated { get; set; }

        /// <summary>
        /// Copies the properties from this resource to another
        /// </summary>
        /// <param name="other">The other resource</param>
        public override void CopyTo(CloudsdaleModel other)
        {
            base.CopyTo(other);
            LastUpdated = DateTime.Now;
        }
    }

    /// <summary>
    /// An attribute for derived types of CloudsdaleResources
    /// which provides information for how to validate and
    /// update the resources
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ResourceEndpointAttribute : Attribute
    {
        public ResourceEndpointAttribute(string endpoint)
        {
            Endpoint = endpoint;
            UpdateEndpoint = endpoint;
        }

        /// <summary>
        /// Endpoint to validate the model at, and used
        /// for updates if the UpdateEndpoint is not set
        /// </summary>
        public string Endpoint { get; private set; }
        /// <summary>
        /// Endpoint to use for updating the model
        /// </summary>
        public string UpdateEndpoint { get; set; }
        /// <summary>
        /// The REST wrapper property for updates to the model
        /// </summary>
        public string RestModelType { get; set; }
    }
}
