using System;
using System.IO;
using Newtonsoft.Json.Linq;
namespace CloudsdaleWin7.lib.CloudsdaleLib
{
    public class Settings
    {
        /// <summary>
        /// The settings class fetches the settings.json from the local installation folder.
        /// It only reads the file on startup and writes on close. 
        /// If you plan on editing the Settings file, edit '_settings' directly.
        /// </summary>
        private static readonly string SettingsFile = CloudsdaleSource.SettingsFile;
        private static JObject _settings = new JObject();
        public Settings()
        {
            Initialize();
            _settings = new JObject(SettingsObject);
        }
        public string this[string key]
        {
            get
            {
                if (_settings[key] != null) return (String) _settings[key];
                _settings.Add(key, key);
                return (String) _settings[key];
            }
        }
        
        /// <summary>
        /// Creates the .JSON file if it doesn't exist already.
        /// </summary>
        private static void Initialize()
        {
            if (File.Exists(SettingsFile)) return;
            var jo = new JObject();
            jo["settings"] = new JObject();
            File.WriteAllText(SettingsFile, jo.ToString());
        }

        /// <summary>
        /// Adds the specified key to the settings JObject.
        /// </summary>
        /// <param name="tokenKey"></param>
        /// <param name="value"></param>
        public void AddSetting(string tokenKey, string value)
        {
            _settings.Add(tokenKey, value);
        }
        /// <summary>
        /// Adds the specified bool value to the settings JObject.
        /// </summary>
        /// <param name="tokenKey"></param>
        /// <param name="value"></param>
        public void AddSetting(string tokenKey, bool value)
        {
            _settings.Add(tokenKey, value);
        }
        /// <summary>
        /// Changes the string value of the specified key.
        /// If the key doesn't exist, it will create it.
        /// </summary>
        /// <param name="tokenKey"></param>
        /// <param name="value"></param>
        public void ChangeSetting(string tokenKey, string value)
        {
            if (_settings[tokenKey] != null)
            {
                _settings[tokenKey].Replace(value);
                return;
            }
            _settings.Add(tokenKey, value);
        }

        /// <summary>
        /// Changes the bool value of the specified key.
        /// If the key doesn't exist, it will create it.
        /// </summary>
        /// <param name="tokenKey"></param>
        /// <param name="value"></param>
        public void ChangeSetting(string tokenKey, bool value)
        {
            if (_settings[tokenKey] != null)
            {
                _settings[tokenKey].Replace(value);
                return;
            }
            _settings.Add(tokenKey, value);
        }
        /// <summary>
        /// Removes the setting from the settings JObject.
        /// </summary>
        /// <param name="tokenKey"></param>
        public void RemoveSetting(string tokenKey)
        {
            _settings[tokenKey].Remove();
        }

        /// <summary>
        /// Saves the settings JObject to the file.
        /// </summary>
        public void Save()
        {
            var saveObject = new JObject();
            saveObject["settings"] = new JObject(_settings);
            File.WriteAllText(SettingsFile, saveObject.ToString());
        }
        /// <summary>
        /// Fetches the text from the JSON file and parses it to 
        /// the settings JObject.
        /// </summary>
        private static JObject SettingsObject
        {
            get
            {
                var o = JObject.Parse(File.ReadAllText(SettingsFile));
                return (JObject)o["settings"];
            }
        }
    }
}
