using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Cloudsdale.Models;

namespace Cloudsdale.Managers {

    public class AppleFarm<T> where T : CloudsdaleItem {
        protected int capacity;
        protected internal readonly CollectionThatLetsMeForceAnUpdateForTheLastItem cache;
        protected readonly bool reverse;

        public AppleFarm() {
            capacity = 50;
            reverse = false;
            cache = new CollectionThatLetsMeForceAnUpdateForTheLastItem();
        } 

        public AppleFarm(int capacity, bool reverse = false) {
            this.capacity = capacity;
            this.reverse = reverse;
            cache = new CollectionThatLetsMeForceAnUpdateForTheLastItem();
        }

        public int Capacity {
            get { return capacity; }
            set {
                while (cache.Count > value) {
                    cache.RemoveAt(0);
                }
                capacity = value;
            }
        }

        public int Count {
            get { return cache.Count; }
        }

        public T LastItem {
            get { return cache[cache.Count - 1]; }
            set { cache[cache.Count - 1] = value; }
        }

        public void Clear() {
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                cache.Clear();
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(() => cache.Clear());
            }
        }

        public void RemoveFirst() {
            cache.RemoveAt(0);
        }

        public virtual void Add(T item) {
            if (cache.Any(ai => ai.id == item.id)) {
                return;
            }
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                if (reverse) {
                    if (cache.Count >= capacity) {
                        cache.RemoveAt(cache.Count - 1);
                    }
                    cache.Insert(0, item);
                } else {
                    if (cache.Count >= capacity) {
                        cache.RemoveAt(0);
                    }
                    cache.Add(item);
                }
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    if (reverse) {
                        if (cache.Count >= capacity) {
                            cache.RemoveAt(cache.Count - 1);
                        }
                        cache.Insert(0, item);
                    } else {
                        if (cache.Count >= capacity) {
                            cache.RemoveAt(0);
                        }
                        cache.Add(item);
                    }
                });
            }
        }

        protected internal class CollectionThatLetsMeForceAnUpdateForTheLastItem : ObservableCollection<T> {
            internal void Trigger(int index) {
                if (Deployment.Current.Dispatcher.CheckAccess()) {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, this[index], this[index], index));
                } else {
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () => OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace, this[index], this[index], index)));
                }
            }
        }

        public ObservableCollection<T> Cache {
            get { return cache; }
        }
    }
}
