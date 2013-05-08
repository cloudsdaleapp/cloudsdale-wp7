using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Cloudsdale.Models;

namespace Cloudsdale.Managers {
    public class PinkiePieEntertainmentDojo : AppleFarm<Drop> {
        private static readonly Dictionary<string, PinkiePieEntertainmentDojo> Dojos = new Dictionary<string, PinkiePieEntertainmentDojo>(); 

        public static PinkiePieEntertainmentDojo GetForCloud(string cloudId) {
            lock (Dojos) {
                return Dojos.ContainsKey(cloudId) ? Dojos[cloudId] : Dojos[cloudId] = new PinkiePieEntertainmentDojo();
            }
        }

        PinkiePieEntertainmentDojo() : base(10) {
        }

        /// <summary>
        /// Makes it hold MOAR DROPZ
        /// </summary>
        /// <param name="drops"></param>
        public void IncreaseCapacity(IEnumerable<Drop> drops) {
            capacity += 10;
            var enumer = drops.GetEnumerator();
            var x = 0;
            while (x < 10 && cache.Count < capacity && enumer.MoveNext()) {
                Add(enumer.Current);
                ++x;
            }
        }

        public bool CanIncreaseCapacity {
            get { return capacity < 100; }
        }

        /// <summary>
        /// Pre loads the drops
        /// </summary>
        /// <param name="drops">The drops, with the first item appearing first in the list</param>
        public void PreLoad(IEnumerable<Drop> drops) {
            var enumer = drops.GetEnumerator();
            while (cache.Count < capacity && enumer.MoveNext()) {
                Add(enumer.Current);
            }
        }

        public void AddDrop(Drop drop) {
            if (cache.Any(d => d.id == drop.id)) {
                return;
            }

            if (!Deployment.Current.Dispatcher.CheckAccess()) {
                Deployment.Current.Dispatcher.BeginInvoke(() => AddDrop(drop));
                return;
            }

            cache.Insert(0, drop);

            while (cache.Count > Capacity) {
                cache.RemoveAt(cache.Count - 1);
            }
        }
    }
}
