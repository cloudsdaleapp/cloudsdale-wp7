namespace CloudsdaleWin7.lib.Models
{
    public class Subscription
    {
        private static string _modelId { get; set; }
        private static string _subId { get; set; }

        private const string CloudType = "CLOUD";
        private const string UserType = "USER";

        /// <summary>
        /// Gets the subscription type of the model.
        /// </summary>
        public SubscriptionType SubscriptionType { get; set; }

        /// <summary>
        /// Initializes a new subscription model.
        /// </summary>
        public Subscription(){}

        /// <summary>
        /// Initializes a new subscription model with a subId.
        /// </summary>
        /// <param name="subId"></param>
        public Subscription(string subId)
        {
            _subId = subId;
            SubscriptionType = subId.StartsWith(CloudType) ? SubscriptionType.Cloud : SubscriptionType.User;
        }

        /// <summary>
        /// Initializes a new subscription model with a SubscriptionType and modelId.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="modelId"></param>
        public Subscription(SubscriptionType type, string modelId)
        {
            _modelId = modelId;
            SubscriptionType = type;
            SubscriptionId = SubscriptionType == SubscriptionType.Cloud
                             ? "CLOUD:" + modelId
                             : "USER:" + modelId;
        }

        /// <summary>
        /// Sets the subscription Id of the model.
        /// </summary>
        public string SubscriptionId
        {
            get { return _subId; }
            set { _subId = value; }
        }

        /// <summary>
        /// Sets the Id of the specified object.
        /// </summary>
        public string ModelId
        {
            get { return _modelId; }
            set { _modelId = value; }
        }
        
    }
    public enum SubscriptionType
    {
        User,
        Cloud
    }
}
