using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using CloudsdaleWin7.lib.Models;
using Newtonsoft.Json;

namespace CloudsdaleWin7.lib.Controllers
{
    public class SubscriptionController
    {
        public ObservableCollection<Subscription> SubscribedUsers { get; set; }
        public ObservableCollection<Subscription> SubscribedClouds { get; set; }

        public SubscriptionController()
        {
            InitializeSubscriptions();
            SubscribedClouds = new ObservableCollection<Subscription>();
            SubscribedUsers = new ObservableCollection<Subscription>();
        }

        /// <summary>
        /// Loads the subscriptions from the settings file.
        /// </summary>
        public void InitializeSubscriptions()
        {
            var subString = App.Settings["subscriptions"];
            if (string.IsNullOrEmpty(subString)) return;
            foreach (var sub in subString.Split('/'))
            {
                var newSub = new Subscription(sub);
                switch (newSub.SubscriptionType)
                {
                    case SubscriptionType.Cloud:
                        SubscribedClouds.Add(newSub);
                        break;
                    case SubscriptionType.User:
                        SubscribedUsers.Add(newSub);
                        break;
                }
            }
        }

        /// <summary>
        /// Saves the subscriptions to file. Advised only for on close.
        /// </summary>
        public void SaveSubscriptions()
        {
            var endObject = "";
            try
            {
                foreach (var sub in SubscribedClouds)
                {
                    endObject += sub.SubscriptionId + "/";
                }
                foreach (var sub in SubscribedUsers)
                {
                    endObject += sub.SubscriptionId + "/";
                }
            }catch(JsonException e)
            {
                Console.WriteLine(e.Message);
            }
            App.Settings.ChangeSetting("subscriptions", endObject);
        }

        /// <summary>
        /// Adds a subscription
        /// </summary>
        /// <param name="type"></param>
        /// <param name="objectId"></param>
        public void AddSubscription(SubscriptionType type, string objectId)
        {
            var sub = new Subscription {SubscriptionType = type, ModelId = objectId};
            switch (type)
            {
                case SubscriptionType.Cloud:
                    if (SubscribedClouds.Contains(sub)) return;
                    SubscribedClouds.Add(sub);
                    break;
                case SubscriptionType.User:
                    if (SubscribedUsers.Contains(sub)) return;
                    SubscribedUsers.Add(sub);
                    break;
            }
        }

        public void AddSubscription(Subscription subscription)
        {
            
            switch (subscription.SubscriptionType)
            {
                case SubscriptionType.Cloud:
                    if (SubscribedClouds.Contains(subscription)) return;
                    SubscribedClouds.Add(subscription);
                    break;
                case SubscriptionType.User:
                    if (SubscribedUsers.Contains(subscription)) return;
                    SubscribedUsers.Add(subscription);
                    break;
            }
        }

        /// <summary>
        /// Removes a subscription
        /// </summary>
        /// <param name="subscription"></param>
        public void RemoveSubscription(Subscription subscription)
        {
            if (SubscribedClouds.Contains(subscription)) SubscribedClouds.Remove(subscription);
            if (SubscribedUsers.Contains(subscription)) SubscribedUsers.Remove(subscription);
        }
    }
}
