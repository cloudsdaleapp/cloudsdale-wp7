using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Cloudsdale.Models;

namespace Cloudsdale.Managers {
    public static class BanDifferentiation {
        public static IEnumerable<BanDiff> DifferentiateBans(Ban[] oldbans, Ban[] newbans) {
            var CheckedIDs = new List<string>();
            foreach (var ban in oldbans) {
                CheckedIDs.Add(ban.id);
                var counterpart = newbans.FirstOrDefault(nban => nban.id == ban.id);
                if (counterpart == null) {
                    yield return new BanDiff {
                        id = ban.id,
                        cloud = ban.jurisdiction_id,
                        isgone = true
                    };
                    continue;
                }
                var bandiff = new BanDiff {
                    id = ban.id,
                    cloud = ban.jurisdiction_id
                };
                var haschange = false;
                if (ban.due != counterpart.due) {
                    bandiff.due = counterpart.due;
                    haschange = true;
                }
                if (ban.reason != counterpart.reason) {
                    bandiff.reason = counterpart.reason;
                    haschange = true;
                }
                if (ban.is_active != counterpart.is_active) {
                    bandiff.active = counterpart.is_active;
                    haschange = true;
                }
                if (ban.revoke != counterpart.revoke) {
                    bandiff.revoked = counterpart.revoke;
                    haschange = true;
                }

                if (haschange) yield return bandiff;
            }
            foreach (var ban in newbans.Where(nban => !CheckedIDs.Contains(nban.id))) {
                yield return new BanDiff {
                    id = ban.id,
                    cloud = ban.jurisdiction_id,
                    isnew = true,
                    active = ban.is_active,
                    revoked = ban.revoke,
                    reason = ban.reason,
                    due = ban.due,
                };
            }
        }
    }
    
    public struct BanDiff {
        public string id;
        public string cloud;
        public bool isnew;
        public bool isgone;
        public bool? active;
        public bool? revoked;
        public string reason;
        public DateTime? due;
    }
}
