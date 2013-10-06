using System;
using System.Collections.ObjectModel;
using CloudsdaleLib.Models;

namespace Cloudsdale_Metro.Models {
    public class ModelCache<T> : ObservableCollection<T> where T : CloudsdaleModel {
        public ModelCache(int capacity) {
            Capacity = capacity;
        }

        public int Capacity { get; set; }

        new public void Add(T item) {
            throw new InvalidOperationException("Use the AddToStart or AddToEnd operations");
        }

        new public void Insert(int index, T item) {
            throw new InvalidOperationException("Use the AddToStart or AddToEnd operations");
        }

        public void AddToStart(T item) {
            IMergable fItem;
            if (Count > 0 && (fItem = this[0] as IMergable) != null && fItem.CanMerge(item)) {
                fItem.Merge(item);
                this[0] = this[0];
            } else {
                base.Insert(0, item);
            }

            while (Count > Capacity) {
                RemoveAt(Count - 1);
            }
        }

        public void AddToEnd(T item) {
            if (item is IPreProcessable) {
                (item as IPreProcessable).PreProcess();
            }

            IMergable fItem;
            if (Count > 0 && (fItem = this[Count - 1] as IMergable) != null && fItem.CanMerge(item)) {
                fItem.Merge(item);
                this[Count - 1] = this[Count - 1];
            } else {
                base.Add(item);
            }

            while (Count > Capacity) {
                RemoveAt(0);
            }
        }
    }
}
