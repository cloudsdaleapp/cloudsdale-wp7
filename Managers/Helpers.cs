using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cloudsdale.Managers {
    public static class Helpers {
        public static IEnumerable<DependencyObject> AllChildrenMatching(
            this DependencyObject root, Func<DependencyObject, bool> predicate) {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; ++i) {
                var child = VisualTreeHelper.GetChild(root, i);
                if (predicate(child)) yield return child;
                foreach (var descendant in child.AllChildrenMatching(predicate)) {
                    yield return descendant;
                }
            }
        }

        public static IEnumerable<DependencyObject> AllChildrenMatching<T>(
            this DependencyObject root) {
            return root.AllChildrenMatching(child => child is T);
        }

        public static IEnumerable<DependencyObject> AllChildren(
            this DependencyObject root) {
            return root.AllChildrenMatching(child => true);
        } 
    }
}
