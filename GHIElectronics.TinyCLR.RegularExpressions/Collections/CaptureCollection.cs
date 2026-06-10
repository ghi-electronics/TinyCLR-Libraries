using System;
using System.Collections;

namespace System.Text.RegularExpressions
{
    /* Previous Implementation
    [Serializable]
    public class CaptureCollection : ICollection, IEnumerable
    {
        #region Fields

        internal int _capcount;
        internal Capture[] _captures;
        internal Group _group;

        #endregion

        #region Constructor

        internal CaptureCollection(Group group)
        {
            this._group = group;
            this._capcount = this._group._captures.Count;
        }

        #endregion

        #region Methods

        public void CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int index = arrayIndex;
            IList list = array as IList;
            for (int i = 0, e = this.Count; i < e; i++)
            {
                list[i] = index;
                index++;
            }
        }

        internal Capture GetCapture(int i)
        {
            if ((i == (this._capcount - 1)) && (i >= 0))
            {
                return this._group;
            }
            if ((i >= this._capcount) || (i < 0))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            if (this._captures == null)
            {
                this._captures = new Capture[this._capcount];
                for (int j = 0; j < (this._capcount - 1); j++)
                {
                    int[] positions = ((int[])(this._group._captures[j]));
                    this._captures[j] = new Capture(this._group._origionalString, positions[0], positions[1]);
                }
            }
            return this._captures[i];
        }

        public IEnumerator GetEnumerator()
        {
            return new CaptureEnumerator(this);
        }

        #endregion

        #region Properties 

        /// <summary>
        /// Gets the number of substrings captured by the group. 
        /// </summary>
        public int Count
        {
            get
            {
                return this._capcount;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the collection is read only. 
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether access to the collection is synchronized (thread-safe). 
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an individual member of the collection. 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Capture this[int i]
        {
            get
            {
                return this.GetCapture(i);
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection. 
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return this._group;
            }
        }

        #endregion
    }
    */

    /// <summary>Represents the set of captures made by a single capturing group.</summary>
    [Serializable]
    public class CaptureCollection : ICollection, IEnumerable
    {
        // Fields
        internal int _capcount;
        internal Capture[] _captures;
        internal Group _group;

        // Methods
        internal CaptureCollection(Group group)
        {
            this._group = group;
            this._capcount = this._group._capcount;
        }

        /// <summary>Copies all the elements of the collection to the given array starting at the given index.</summary>
        public void CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            IList list = array as IList;
            for (int i = 0; i < this._capcount; ++i)
            {
                list[i] = this[++arrayIndex];
                //array.SetValue(this[i], index);
                //index++;
            }
        }

        internal Capture GetCapture(int i)
        {
            if ((i == (this._capcount - 1)) && (i >= 0))
            {
                return this._group;
            }
            if ((i >= this._capcount) || (i < 0))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            if (this._captures == null)
            {
                this._captures = new Capture[this._capcount];
                for (int j = 0; j < (this._capcount - 1); j++)
                {
                    this._captures[j] = new Capture(this._group._text, this._group._caps[j * 2], this._group._caps[(j * 2) + 1]);
                }
            }
            return this._captures[i];
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        public IEnumerator GetEnumerator()
        {
            return new CaptureEnumerator(this);
        }

        /// <summary>The number of captures in the collection.</summary>
        // Properties
        public int Count
        {
            get
            {
                return this._capcount;
            }
        }

        /// <summary>Gets a value indicating whether the collection is read-only.</summary>
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>Gets a value indicating whether access to the collection is synchronized (thread-safe).</summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>Gets the capture at the specified index in the collection.</summary>
        public Capture this[int i]
        {
            get
            {
                return this.GetCapture(i);
            }
        }

        /// <summary>An object that can be used to synchronize access to the collection.</summary>
        public object SyncRoot
        {
            get
            {
                return this._group;
            }
        }
    }


}
