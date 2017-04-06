using System.Collections;

namespace System.Diagnostics {
    public class TraceListenerCollection : IList {
        private ArrayList list = new ArrayList();

        public TraceListener this[int index] { get => (TraceListener)(this.list[index]); set => this.list[index] = value; }
        public int Count => this.list.Count;
        public IEnumerator GetEnumerator() => this.list.GetEnumerator();
        public int Add(TraceListener value) => this.list.Add(value);
        public void Remove(TraceListener value) => this.list.Remove(value);
        public void Clear() => this.list.Clear();

        object IList.this[int index] { get => this[index]; set => this[index] = (TraceListener)value; }
        int IList.Add(object value) => this.Add((TraceListener)value);
        void IList.Remove(object value) => this.Remove((TraceListener)value);

        bool IList.IsReadOnly => this.list.IsReadOnly;
        bool IList.IsFixedSize => this.list.IsFixedSize;
        object ICollection.SyncRoot => this.list.SyncRoot;
        bool ICollection.IsSynchronized => this.list.IsSynchronized;
        bool IList.Contains(object value) => this.list.Contains(value);
        void ICollection.CopyTo(Array array, int index) => this.list.CopyTo(array, index);
        int IList.IndexOf(object value) => this.list.IndexOf(value);
        void IList.Insert(int index, object value) => this.list.Insert(index, (TraceListener)value);
        void IList.RemoveAt(int index) => this.list.RemoveAt(index);
    }
}
