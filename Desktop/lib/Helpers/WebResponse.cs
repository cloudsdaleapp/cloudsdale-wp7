using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudsdaleWin7.lib.Helpers
{
    public class JsonContent : StringContent
    {
        /// <summary>
        /// Http content created from a json-formatted string
        /// </summary>
        /// <param name="json">A json-formatted string to be passed to the server</param>
        public JsonContent(string json)
            : base(json)
        {
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        /// <summary>
        /// Http content created from a jtoken, which will be rendered to a string content
        /// </summary>
        /// <param name="json">Json content to be rendered to the server</param>
        public JsonContent(JToken json)
            : this(json.ToString(Formatting.None))
        {
        }

        /// <summary>
        /// Json content rendered from a serializable object
        /// </summary>
        /// <param name="json">Object to be serialized into a string</param>
        public JsonContent(object json)
            : this(JObject.FromObject(json).ToString(Formatting.None))
        {
        }
    }

    /// <summary>
    /// Methods to help with HttpClients
    /// </summary>
    public static class WebHelpers
    {
        /// <summary>
        /// Adds an application/json accept header to the httpclient
        /// </summary>
        /// <param name="client">The client you're working on</param>
        /// <returns>The same client which was passed, for chaining purposes</returns>
        public static HttpClient AcceptsJson(this HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }

    /// <summary>
    /// Web response object for clousdsdale v1 endpoints
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    public class WebResponse<T>
    {
        /// <summary>
        /// Data response
        /// </summary>
        [JsonProperty("result")]
        public T Result;

        /// <summary>
        /// Flash data for rendered errors
        /// </summary>
        [JsonProperty("flash")]
        public FlashData Flash;

        private Error[] _errors;

        /// <summary>
        /// Errors rendered from the server
        /// </summary>
        [JsonProperty("errors")]
        public Error[] Errors
        {
            get
            {
                if (_errors != null)
                    foreach (var error in _errors)
                    {
                        error.Response = this;
                    }
                return _errors;
            }
            set { _errors = value; }
        }

        /// <summary>
        /// Title and message for certain types of rendered cloudsdale errors
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class FlashData
        {
            [JsonProperty("title")]
            public string Title;
            [JsonProperty("message")]
            public string Message;
        }

        /// <summary>
        /// Error for certain types of process errors
        /// that may occur during user data posts
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class Error
        {
            internal WebResponse<T> Response;

            /// <summary>
            /// Property within the data which was invalid
            /// </summary>
            [JsonProperty("ref_node")]
            public string Node;

            /// <summary>
            /// Message pertaining to the problem with the data
            /// </summary>
            [JsonProperty("message")]
            public string Message;

            /// <summary>
            /// The value of the property from the error
            /// </summary>
            public object NodeValue
            {
                get
                {
                    return (from property in typeof(T).GetTypeInfo().DeclaredProperties
                            let attribute = property.GetCustomAttribute<JsonPropertyAttribute>()
                            where attribute != null
                            where attribute.PropertyName == Node
                            select property.GetValue(Response.Result))
                            .FirstOrDefault();
                }
            }
        }
    }
}
