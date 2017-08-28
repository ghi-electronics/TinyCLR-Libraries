namespace System.Diagnostics {
    public class DefaultTraceListener : TraceListener {
        public override void Write(string message) => Debugger.Log(0, string.Empty, message);
        public override void WriteLine(string message) => Debugger.Log(0, string.Empty, message + Environment.NewLine);
    }
}
