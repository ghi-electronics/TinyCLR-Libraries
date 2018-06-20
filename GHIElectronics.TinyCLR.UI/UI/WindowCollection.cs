using System;
using System.Collections;
using System.Diagnostics;

namespace GHIElectronics.TinyCLR.UI {
    #region WindowCollection class

    /// <summary>
    /// WindowCollection can be used to interate over all the windows that have been
    /// opened in the current application.
    /// </summary>
    //CONSIDER: Should this be a sealed class?
    public sealed class WindowCollection : ICollection {
        //------------------------------------------------------
        //
        //   Public Methods
        //
        //------------------------------------------------------
        #region Public Methods
        /// <summary>
        /// Default Constructor
        /// </summary>
        public WindowCollection() => this._list = new ArrayList {
            Capacity = 1
        };

        internal WindowCollection(int count) {
            Debug.Assert(count >= 0, "count must not be less than zero");
            this._list = new ArrayList {
                Capacity = 1
            };
        }

        #endregion Public Methods

        //------------------------------------------------------
        //
        //   Operator overload
        //
        //------------------------------------------------------
        #region Operator overload
        /// <summary>
        /// Overloaded [] operator to access the WindowCollection list
        /// </summary>
        public Window this[int index] => this._list[index] as Window;

        #endregion Operator overload

        //------------------------------------------------------
        //
        //   IEnumerable implementation
        //
        //------------------------------------------------------
        #region IEnumerable implementation
        /// <summary>
        /// GetEnumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator() => this._list.GetEnumerator();

        #endregion IEnumerable implementation

        //--------------------------------------------------------
        //
        //   ICollection implementation (derives from IEnumerable)
        //
        //--------------------------------------------------------
        #region ICollection implementation
        /// <summary>
        /// CopyTo
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        void ICollection.CopyTo(Array array, int index) => this._list.CopyTo(array, index);

        /// <summary>
        /// CopyTo
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(Window[] array, int index) => this._list.CopyTo(array, index);

        /// <summary>
        /// Count property
        /// </summary>
        public int Count => this._list.Count;

        /// <summary>
        /// IsSynchronized
        /// </summary>
        public bool IsSynchronized => this._list.IsSynchronized;

        /// <summary>
        /// SyncRoot
        /// </summary>
        public object SyncRoot => this._list.SyncRoot;

        #endregion ICollection implementation

        //------------------------------------------------------
        //
        //  Internal Methods
        //
        //------------------------------------------------------
        #region Internal Methods
        internal WindowCollection Clone() {
            WindowCollection clone;
            lock (this._list.SyncRoot) {
                clone = new WindowCollection(this._list.Count);
                for (var i = 0; i < this._list.Count; i++) {
                    clone._list.Add(this._list[i]);
                }
            }

            return clone;
        }

        internal void Remove(Window win) => this._list.Remove(win);

        internal int Add(Window win) => this._list.Add(win);

        internal bool HasItem(Window win) {
            lock (this._list.SyncRoot) {
                for (var i = 0; i < this._list.Count; i++) {
                    if (this._list[i] == win) {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Internal Methods

        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------
        #region Private Fields
        private ArrayList _list;
        #endregion Private Fields
    }

    #endregion WindowCollection class
}


