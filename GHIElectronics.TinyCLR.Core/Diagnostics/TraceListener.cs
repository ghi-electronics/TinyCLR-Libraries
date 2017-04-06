namespace System.Diagnostics {
    public abstract class TraceListener : IDisposable {
        public abstract void Write(string message);
        public abstract void WriteLine(string message);

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {

        }
    }
}
