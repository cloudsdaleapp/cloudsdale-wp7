using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Cloudsdale.Models;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Cloudsdale.Managers {
    public class PonyvilleDirectory {
        private static readonly Dictionary<string, Cloud> Clouds = new Dictionary<string, Cloud>();
        public static Cloud RegisterCloud(Cloud cloud) {
            lock (Clouds) {
                if (Clouds.ContainsKey(cloud.id)) {
                    Clouds[cloud.id].CopyFrom(cloud);
                    return Clouds[cloud.id];
                }
                return Clouds[cloud.id] = cloud;
            }
        }

        public static Cloud GetCloud(string id) {
            return Clouds[id];
        }

        private static readonly List<string> CloudOrder = new List<string>();

        static PonyvilleDirectory() {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            if (storage.FileExists("cloudOrder")) {
                LoadCloudOrder(storage);
            }
        }

        public static void SaveCloudOrder() {
            var storage = IsolatedStorageFile.GetUserStoreForApplication();
            if (storage.FileExists("cloudOrder")) {
                storage.DeleteFile("cloudOrder");
            }

            var jObject = JArray.FromObject(CloudOrder);
            var dataString = jObject.ToString();

            using (var stream = storage.CreateFile("cloudOrder"))
            using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
                writer.Write(dataString);
            }
        }

        public static void LoadCloudOrder(IsolatedStorageFile storage) {
            string data;
            using (var stream = storage.OpenFile("cloudOrder", FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8)) {
                data = reader.ReadToEnd();
            }

            var order = JArray.Parse(data);
            lock (CloudOrder) {
                CloudOrder.Clear();
                CloudOrder.AddRange(from cloudId in order select (string) cloudId);
            }
        }

        public static IComparer<Cloud> GetUserCloudListComparer() {
            return new UserCloudComparer();
        }

        public static int CloudIndex(string id) {
            lock (CloudOrder) {
                if (!CloudOrder.Contains(id)) {
                    CloudOrder.Add(id);
                }

                return CloudOrder.IndexOf(id);
            }
        }

        public static void MoveItem(string id, int amount) {
            lock (CloudOrder) {
                lock (CloudOrder) {
                    var tempList = new List<string>(CloudOrder);
                    tempList.Sort(new UserCloudComparer());

                    CloudOrder.Clear();
                    CloudOrder.AddRange(tempList);
                }

                if (!CloudOrder.Contains(id)) {
                    CloudOrder.Add(id);
                }

                var index = CloudOrder.IndexOf(id);
                CloudOrder.RemoveAt(index);
                index += amount;
                index = Math.Max(0, Math.Min(CloudOrder.Count, index));
                CloudOrder.Insert(index, id);

                SaveCloudOrder();
            }
        }

        private class UserCloudComparer : IComparer<Cloud>, IComparer<String> {

            #region Implementation of IComparer<Cloud>

            public int Compare(Cloud x, Cloud y) {

                return Compare(x.id, y.id);
            }

            private static bool ActiveCloud(string id) {
                return Connection.CurrentCloudsdaleUser == null || Connection.CurrentCloudsdaleUser.clouds.Any(cloud => cloud.id == id);
            }

            #endregion

            #region Implementation of IComparer<string>

            public int Compare(string x, string y) {
                lock (CloudOrder) {

                    if (!ActiveCloud(x)) {
                        return ActiveCloud(y) ? int.MaxValue : 0;
                    }

                    if (CloudOrder.Contains(x)) {
                        if (!CloudOrder.Contains(y)) {
                            CloudOrder.Add(y);
                        }
                    } else if (CloudOrder.Contains(y)) {
                        CloudOrder.Add(x);
                    } else {
                        CloudOrder.Add(x);
                        CloudOrder.Add(y);
                    }

                    return CloudOrder.IndexOf(x) - CloudOrder.IndexOf(y);
                }
            }

            #endregion
        }
    }
}
