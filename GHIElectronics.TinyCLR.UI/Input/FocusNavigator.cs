using System.Collections;
using GHIElectronics.TinyCLR.UI.Controls;

namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>
    /// PC-style tab order over the logical tree. Call from hardware mappings (e.g. next/previous)
    /// via <see cref="InputProvider.RaiseFocusNavigation"/>.
    /// </summary>
    public static class FocusNavigator {
        /// <summary>
        /// Moves keyboard focus to the next or previous tab stop under <see cref="Application.MainWindow"/>.
        /// </summary>
        /// <returns>True if focus was moved or already on a valid target.</returns>
        public static bool TryMoveFocus(bool forward) {
            var root = GHIElectronics.TinyCLR.UI.Application.Current?.MainWindow;
            if (root == null) {
                return false;
            }

            return TryMoveFocus(forward, root);
        }

        /// <summary>Moves focus within the subtree rooted at <paramref name="scope"/>.</summary>
        public static bool TryMoveFocus(bool forward, UIElement scope) {
            if (scope == null) {
                return false;
            }

            var list = new ArrayList();
            CollectTabStops(scope, list);
            if (list.Count == 0) {
                return false;
            }

            SortTabStops(list);

            var current = Buttons.FocusedElement as Control;
            var idx = -1;
            if (current != null) {
                for (var i = 0; i < list.Count; i++) {
                    if (list[i] == current) {
                        idx = i;
                        break;
                    }
                }
            }

            if (idx < 0) {
                idx = forward ? 0 : list.Count - 1;
            }
            else {
                if (forward) {
                    idx++;
                    if (idx >= list.Count) {
                        idx = 0;
                    }
                }
                else {
                    idx--;
                    if (idx < 0) {
                        idx = list.Count - 1;
                    }
                }
            }

            var next = (Control)list[idx];
            Buttons.Focus(next);
            return true;
        }

        private static void CollectTabStops(UIElement node, ArrayList output) {
            if (node is Control c && c.IsTabStop && c.IsEnabled && c.IsVisible) {
                output.Add(c);
            }

            var kids = node.LogicalChildren;
            if (kids == null) {
                return;
            }

            for (var i = 0; i < kids.Count; i++) {
                CollectTabStops(kids[i], output);
            }
        }

        private static void SortTabStops(ArrayList list) {
            var n = list.Count;
            for (var i = 0; i < n - 1; i++) {
                for (var j = 0; j < n - 1 - i; j++) {
                    var a = (Control)list[j];
                    var b = (Control)list[j + 1];
                    if (a.TabIndex > b.TabIndex) {
                        var t = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = t;
                    }
                }
            }
        }
    }
}
