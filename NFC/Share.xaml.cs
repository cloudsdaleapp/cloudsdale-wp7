using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Cloudsdale.Models;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NdefLibrary.Ndef;
using Windows.Networking.Proximity;

namespace Cloudsdale.NFC {
    public partial class Share {
        private readonly Cloud cloud;
        private List<long> messageIds = new List<long>();

        public Share() {
            InitializeComponent();
            DataContext = cloud = Connection.CurrentCloud;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            var device = ProximityDevice.GetDefault();
            if (device == null) return;
            var record = new NdefSpRecord {
                Uri = "cloudsdale://clouds/" + cloud.id,
                NfcAction = NdefSpActRecord.NfcActionType.DoAction
            };
            record.AddTitle(new NdefTextRecord { Text = cloud.name, LanguageCode = "en" });
            var message = new NdefMessage { record };
            messageIds.Add(device.PublishBinaryMessage("NDEF", message.ToByteArray().AsBuffer()));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            var device = ProximityDevice.GetDefault();
            if (device == null) return;
            foreach (var id in messageIds) {
                device.StopPublishingMessage(id);
            }
        }
    }
}