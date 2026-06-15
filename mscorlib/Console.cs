using System.Diagnostics;

namespace System {
    public static class Console {
        public static void Write(string value) {
            foreach (var listener in Trace.Listeners)
                ((TraceListener)listener).Write(value);
        }

        public static void WriteLine() {
            foreach (var listener in Trace.Listeners)
                ((TraceListener)listener).WriteLine(string.Empty);
        }

        public static void WriteLine(string value) {
            foreach (var listener in Trace.Listeners)
                ((TraceListener)listener).WriteLine(value);
        }

        public static void Write(object value) => Write(value == null ? string.Empty : value.ToString());
        public static void WriteLine(object value) => WriteLine(value == null ? string.Empty : value.ToString());

        public static void Write(char value) => Write(value.ToString());
        public static void Write(bool value) => Write(value.ToString());
        public static void Write(int value) => Write(value.ToString());
        [CLSCompliant(false)]
        public static void Write(uint value) => Write(value.ToString());
        public static void Write(long value) => Write(value.ToString());
        [CLSCompliant(false)]
        public static void Write(ulong value) => Write(value.ToString());
        public static void Write(float value) => Write(value.ToString());
        public static void Write(double value) => Write(value.ToString());

        public static void WriteLine(char value) => WriteLine(value.ToString());
        public static void WriteLine(bool value) => WriteLine(value.ToString());
        public static void WriteLine(int value) => WriteLine(value.ToString());
        [CLSCompliant(false)]
        public static void WriteLine(uint value) => WriteLine(value.ToString());
        public static void WriteLine(long value) => WriteLine(value.ToString());
        [CLSCompliant(false)]
        public static void WriteLine(ulong value) => WriteLine(value.ToString());
        public static void WriteLine(float value) => WriteLine(value.ToString());
        public static void WriteLine(double value) => WriteLine(value.ToString());
    }
}
