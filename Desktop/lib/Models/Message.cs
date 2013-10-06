using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CloudsdaleWin7.lib.CloudsdaleLib;
using CloudsdaleWin7.lib.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudsdaleWin7.lib.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Message : CloudsdaleModel, IMergable, IPreProcessable
    {
        /// <summary>
        /// Regular expression to determine if a message is of the action format
        /// </summary>
        public static readonly Regex SlashMeFormat = new Regex(@"^/me");

        public Message()
        {
            _drops = new Drop[0];
            _timestamp = DateTime.Now;
        }
        
        private string _id;
        private User _author;
        private Drop[] _drops;
        private string _authorId;
        private string _device;
        private string _clientId;
        private string _content;
        private DateTime _timestamp;
        public string PostedOn { get; set; }

        public Message RawMessage
        {
            get { return this; }
        }

        private readonly List<Message> _messages = new List<Message>();

        /// <summary>
        /// ID of the message to distinguish it from others
        /// </summary>
        [JsonProperty("id")]
        public string Id
        {
            get { return _id; }
            set
            {
                if (value == _id) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The time at which the message was sent
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                OnPropertyChanged();
                OnPropertyChanged("FinalTimestamp");
            }
        }

        /// <summary>
        /// Content of the message
        /// </summary>
        [JsonProperty("content")]
        public string Content
        {
            get { return _content; }
            set
            {
                if (value == _content) return;
                _content = value;
                OnPropertyChanged();
                OnPropertyChanged("Messages");
            }
        }

        /// <summary>
        /// The ID of the client which sent the message
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId
        {
            get { return _clientId; }
            set
            {
                if (value == _clientId) return;
                _clientId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The type of device from which the message was sent.
        /// Valid values include "desktop", "mobile", and "robot"
        /// </summary>
        [JsonProperty("device")]
        public string Device
        {
            get { return _device; }
            set
            {
                if (value == _device) return;
                _device = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The ID of the user which sent the message
        /// </summary>
        [JsonProperty("author_id")]
        public string AuthorId
        {
            get { return _authorId; }
            set
            {
                if (value == _authorId) return;
                _authorId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Drops detected within the message
        /// </summary>
        [JsonProperty("drops")]
        public Drop[] Drops
        {
            get { return _drops; }
            set
            {
                if (Equals(value, _drops)) return;
                _drops = value;
                OnPropertyChanged();
                OnPropertyChanged("AllDrops");
            }
        }

        /// <summary>
        /// A limited amount of information about
        /// the user who sent the message
        /// </summary>
        [JsonProperty("author")]
        public User Author
        {
            get { return _author; }
            set
            {
                if (Equals(value, _author)) return;
                _author = value;
                OnPropertyChanged();
                OnPropertyChanged("User");
            }
        }

        /// <summary>
        /// Collation of the contents of all the messages which
        /// have been merged with this one
        /// </summary>
        public string[] Messages
        {
            get
            {
                var messages = new string[_messages.Count + 1];
                messages[0] = Content;
                for (var i = 0; i < _messages.Count; ++i)
                {
                    messages[i + 1] = _messages[i].Content;
                }
                return messages;
            }
        }

        /// <summary>
        /// A collection of the drops from all of the
        /// messages which have been merged with this one
        /// </summary>
        public IEnumerable<Drop> AllDrops
        {
            get
            {
                return _messages.Aggregate(new List<Drop>(Drops), (list, message) =>
                {
                    list.AddRange(message.Drops);
                    return list;
                });
            }
        }

        /// <summary>
        /// The cache-backed user object for the 
        /// user which sent this message
        /// </summary>
        public User User
        {
            get { return Cloudsdale.CloudServicesProvider.GetBackedUser(Author.Id); }
        }

        /// <summary>
        /// The timestamp of the last message which was merged with this one
        /// </summary>
        public DateTime FinalTimestamp
        {
            get
            {
                if (_messages.Count < 1)
                {
                    return Timestamp;
                }
                return _messages.Last().Timestamp;
            }
        }

        public void Merge(CloudsdaleModel other)
        {
            _messages.Add((Message)other);
            OnPropertyChanged("Messages");
            OnPropertyChanged("AllDrops");
            OnPropertyChanged("FinalTimestamp");
            

            other.PropertyChanged += (sender, args) => OnPropertyChanged(args.PropertyName);
        }

        public bool CanMerge(CloudsdaleModel other)
        {
            var otherMessage = (Message)other;
            return User.Id == otherMessage.User.Id && !SlashMeFormat.IsMatch(Content) && !SlashMeFormat.IsMatch(otherMessage.Content);
        }

        public void PreProcess()
        {
           Content = Content.ParseMessage();
        }
    }
}
