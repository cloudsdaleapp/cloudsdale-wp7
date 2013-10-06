using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CloudsdaleLib.Annotations;
using CloudsdaleLib.Models;
using CloudsdaleLib.Providers;
using Cloudsdale_Metro.Controllers;

namespace Cloudsdale_Metro.Models {
    public class UserStatusMetadataProvider : IMetadataProvider {
        public IMetadataObject CreateNew(CloudsdaleModel model) {
            return new UserStatusMetadata(model);
        }

        public class UserStatusMetadata : IMetadataObject, INotifyPropertyChanged {
            private CloudController controller;
            public UserStatusMetadata(CloudsdaleModel model) {
                Model = model;
                CorrectController();

                if (Model is Session) {
                    Model.PropertyChanged += ControllerOnPropertyChanged;
                }
            }

            void CorrectController() {

                if (controller == App.Connection.MessageController.CurrentCloud) return;
                if (controller != null) {
                    controller.PropertyChanged -= ControllerOnPropertyChanged;
                }
                controller = App.Connection.MessageController.CurrentCloud;
                controller.PropertyChanged += ControllerOnPropertyChanged;
            }

            private void ControllerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
                OnPropertyChanged("Value");
            }

            public object Value {
                get {
                    if (Model is Session) {
                        return (Model as Session).PreferredStatus;
                    }
                    return App.Connection.MessageController.CurrentCloud.StatusForUser(((User)Model).Id);
                }
                set { throw new NotSupportedException(); }
            }

            public CloudsdaleModel Model { get; private set; }
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class UserOnlineMetadataProvider : IMetadataProvider {
        public IMetadataObject CreateNew(CloudsdaleModel model) {
            return new UserOnlineMetadata(model);
        }

        public class UserOnlineMetadata : IMetadataObject, INotifyPropertyChanged {
            private CloudController controller;
            public UserOnlineMetadata(CloudsdaleModel model) {
                Model = model;
                CorrectController();
            }

            void CorrectController() {
                if (controller == App.Connection.MessageController.CurrentCloud) return;
                if (controller != null) {
                    
                }
                controller = App.Connection.MessageController.CurrentCloud;
                controller.PropertyChanged += ControllerOnPropertyChanged;
            }

            private void ControllerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
                OnPropertyChanged("Value");
            }

            public object Value {
                get { return (Status)Model.UIMetadata["Status"].Value != Status.offline; }
                set { throw new NotSupportedException(); }
            }

            public CloudsdaleModel Model { get; private set; }
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
