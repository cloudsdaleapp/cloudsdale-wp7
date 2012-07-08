﻿using System.Windows;
using Cloudsdale.Models;

namespace Cloudsdale.Managers {
    public class SweetAppleAcres : AppleFarm<Message> {

        public SweetAppleAcres(int capacity)
            : base(capacity) {
        }

        public override void Add(Message item) {
            if (Deployment.Current.Dispatcher.CheckAccess()) {
                InternalAdd(item);
            } else {
                Deployment.Current.Dispatcher.BeginInvoke(() => InternalAdd(item));
            }
        }

        private void InternalAdd(Message item) {
            var count = cache.Count;
            for (var i = 0; i < count; ++i) {
                if (cache[i].id == item.id) return;
            }

            var greatest = 0;
            while (greatest < cache.Count && cache[greatest].timestamp < item.timestamp) greatest++;

            if (greatest > 0 && cache[greatest - 1].user.id == item.user.id) {
                cache[greatest - 1].AddSub(item);
                cache.Trigger(greatest - 1);
                return;
            }

            cache.Insert(greatest, item);

            if (cache.Count > Capacity) {
                cache.RemoveAt(0);
            }
        }
    }
}