using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using CloudsdaleLib.Models;
using Cloudsdale_Metro.Common;
using Cloudsdale_Metro.Helpers;
using Newtonsoft.Json;
using WinRTXamlToolkit.IO.Extensions;

namespace Cloudsdale_Metro.Models {
    [JsonObject(MemberSerialization.OptIn)]
    public class AppSettings : CloudsdaleModel {
        public static readonly AppSettings Settings = new AppSettings();
        private bool _displayNotifications;
        private LayoutAwarePage.ObservableDictionary<string, int> _unreadMessages = 
            new LayoutAwarePage.ObservableDictionary<string, int>();

        private static bool _isSaving = false;

        private AppSettings() {
            PropertyChanged += async delegate { await Save(); };
        }

        [JsonProperty]
        public bool DisplayNotifications {
            get { return _displayNotifications; }
            set {
                if (value.Equals(_displayNotifications)) return;
                _displayNotifications = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty]
        public LayoutAwarePage.ObservableDictionary<string, int> UnreadMessages {
            get { return _unreadMessages; }
            set {
                if (Equals(value, _unreadMessages)) return;
                _unreadMessages = value;
                OnPropertyChanged();
            }
        }

        public static async Task Load() {
            var storage = ApplicationData.Current.RoamingFolder;
            var settingsFolder = await storage.EnsureFolderExists("settings");
            if (!await settingsFolder.FileExists("settings.json")) {
                return;
            }

            var settingsFile = await settingsFolder.GetFileAsync("settings.json");
            var settingsData = await settingsFile.ReadAllText();
            var settings = await JsonConvert.DeserializeObjectAsync<AppSettings>(settingsData);
            if (settings == null) return;
            settings.CopyTo(Settings);
        }

        public static async Task Save() {
            while (_isSaving) {
                await Task.Delay(20);
            }
            _isSaving = true;

            try {
                var storage = ApplicationData.Current.RoamingFolder;
                var settingsFolder = await storage.EnsureFolderExists("settings");
                var settingsFile = await settingsFolder.CreateFileAsync("settings.json", CreationCollisionOption.ReplaceExisting);
                var settingsData = await JsonConvert.SerializeObjectAsync(Settings
#if DEBUG
                    , Formatting.Indented
#endif
                    );
                await settingsFile.SaveAllText(settingsData);
            } finally {
                _isSaving = false;
            }
        }
    }
}
