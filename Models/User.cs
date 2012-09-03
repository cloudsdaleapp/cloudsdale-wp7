using System;
using System.ComponentModel;
using System.Windows.Media;
using Cloudsdale.Managers;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace Cloudsdale.Models {

    [JsonObject(MemberSerialization.OptIn)]
    public class ListUser : UserReference {

        [JsonProperty]
        public virtual string name { get; set; }
        [JsonProperty]
        public virtual Avatar avatar { get; set; }

        [JsonIgnore]
        public ListUser AsListUser {
            get { return new ListUser { id = id, name = name, avatar = avatar }; }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SimpleUser : ListUser {
        [JsonProperty]
        public string role;

        [JsonIgnore]
        public string RoleTag {
            get {
                switch (role) {
                    case "founder":
                    case "donor":
                    case "developer":
                        return "[ " + role.ToLower() + " ]";
                }
                return "";
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class User : SimpleUser, INotifyPropertyChanged {

        public User CurrentUser {
            get { return PonyvilleCensus.GetUser(Connection.CurrentCloudsdaleUser.id); }
        }

        [JsonIgnore]
        public bool OwnerOfCurrent {
            get { return Connection.CurrentCloud.Owner == id; }
        }

        [JsonIgnore]
        public bool ModOfCurrent {
            get { return OwnerOfCurrent || Connection.CurrentCloud.Moderators.Contains(id); }
        }

        [JsonIgnore]
        public Brush CloudColor {
            get {
                return OwnerOfCurrent
                           ? new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x00, 0xFF))
                           : ModOfCurrent
                                 ? new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x66, 0xFF))
                                 : new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));
            }
        }

        [JsonIgnore]
        public Brush RoleBrush {
            get {
                switch (role) {
                    case "founder":
                    case "developer":
                        return CloudColor;
                }
                return new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));
            }
        }

        [JsonProperty]
        public string time_zone;

        [JsonProperty]
        public DateTime? member_since { get; set; }
        [JsonProperty]
        public DateTime? suspended_until;
        [JsonProperty]
        public string reason_for_suspension;
        [JsonProperty]
        public bool? is_registered;
        [JsonProperty]
        public bool? is_transient;
        [JsonProperty]
        public bool? is_banned;
        [JsonProperty]
        public bool? is_member_of_a_cloud;
        [JsonProperty]
        public bool? has_an_avatar;
        [JsonProperty]
        public bool? has_read_tnc;
        [JsonProperty]
        public Prosecution[] prosecutions;

        public void CopyTo(User user) {
            if (id != null)
                user.id = id;
            if (name != null)
                user.name = name;
            if (avatar != null) {
                if (avatar.Chat != null)
                    user.avatar.Chat = avatar.Chat;
                if (avatar.Mini != null)
                    user.avatar.Mini = avatar.Mini;
                if (avatar.Normal != null)
                    user.avatar.Normal = avatar.Normal;
                if (avatar.Preview != null)
                    user.avatar.Preview = avatar.Preview;
                if (avatar.Thumb != null)
                    user.avatar.Thumb = avatar.Thumb;
            }
            if (role != null)
                user.role = role;
            if (time_zone != null)
                user.time_zone = time_zone;
            if (member_since != null)
                user.member_since = member_since;
            if (suspended_until != null)
                user.suspended_until = suspended_until;
            if (reason_for_suspension != null)
                user.reason_for_suspension = reason_for_suspension;
            if (is_registered != null)
                user.is_registered = is_registered;
            if (is_transient != null)
                user.is_transient = is_transient;
            if (is_banned != null)
                user.is_banned = is_banned;
            if (is_member_of_a_cloud != null)
                user.is_member_of_a_cloud = is_member_of_a_cloud;
            if (has_an_avatar != null)
                user.has_an_avatar = has_an_avatar;
            if (has_read_tnc != null)
                user.has_read_tnc = has_read_tnc;
            if (prosecutions != null)
                user.prosecutions = prosecutions;
        }

        [JsonIgnore]
        public Visibility ShowSuspended {
            get {
                return suspended_until != null && suspended_until > DateTime.Now ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        [JsonIgnore]
        public string SuspendedUntilMessage {
            get {
                return "Suspended until " + suspended_until;
            }
        }

        [JsonIgnore]
        public string SuspendedReason {
            get {
                return reason_for_suspension ?? "No reason given";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal virtual void OnPropertyChanged(string propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LoggedInUser : User {
        [JsonProperty]
        public string auth_token;
        [JsonProperty]
        public string email;
        [JsonProperty]
        public bool? needs_to_confirm_registration;
        [JsonProperty]
        public bool? needs_password_change;
        [JsonProperty]
        public bool? needs_name_change;

        [JsonIgnore]
        private readonly ObservableCollection<Cloud> _cloudCol = new ObservableCollection<Cloud>();
        [JsonIgnore]
        private Cloud[] _clouds;
        [JsonProperty("clouds")]
        public Cloud[] clouds {
            get {
                return _clouds;
            }
            set {
                _clouds = value;
                if (Deployment.Current.Dispatcher.CheckAccess()) {
                    PopulateClouds();
                } else {
                    Deployment.Current.Dispatcher.BeginInvoke(PopulateClouds);
                }
            }
        }
        [JsonIgnore]
        public ObservableCollection<Cloud> Clouds {
            get { return _cloudCol; }
        }

        void PopulateClouds() {
            _cloudCol.Clear();
            if (_clouds != null)
                foreach (var cloud in _clouds) {
                    _cloudCol.Add(cloud);
                }
        }

        public void CopyTo(LoggedInUser user) {
            base.CopyTo(user);
            if (auth_token != null)
                user.auth_token = auth_token;
            if (email != null)
                user.email = email;
            if (needs_to_confirm_registration != null)
                user.needs_to_confirm_registration = needs_to_confirm_registration;
            if (needs_name_change != null)
                user.needs_name_change = needs_name_change;
            if (clouds != null)
                user.clouds = clouds;
        }
    }
}
