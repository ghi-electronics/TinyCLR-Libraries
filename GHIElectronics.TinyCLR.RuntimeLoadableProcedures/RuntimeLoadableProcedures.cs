using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.RuntimeLoadableProcedures {
    public static class RuntimeLoadableProcedures {

        public delegate void NativeEventHandler(uint data);

        // Native code posts events via RLP_PostManagedEvent(data); the
        // firmware-side helper marshals each payload into a NativeEventDispatcher
        // keyed by this string. We translate the dispatcher's generic
        // (string, long, long, long, IntPtr, DateTime) shape into the simple
        // (uint) signature consumers expect.
        private const string DispatcherName =
            "GHIElectronics.TinyCLR.NativeEventNames.RuntimeLoadableProcedures.OnEvent";

        private static NativeEventDispatcher s_dispatcher;
        private static NativeEventHandler s_userEvent;

        public static event NativeEventHandler NativeEvent {
            add {
                if (s_dispatcher == null) {
                    s_dispatcher = NativeEventDispatcher.GetDispatcher(DispatcherName);
                    s_dispatcher.OnInterrupt += OnDispatcher;
                }
                s_userEvent += value;
            }
            remove {
                s_userEvent -= value;
            }
        }

        // Bridge from the Native-package event signature to the user-facing
        // `NativeEventHandler(uint data)` shape. Runs on a managed thread, never
        // in interrupt context.
        private static void OnDispatcher(string api, long d0, long d1, long d2, IntPtr d3, DateTime ts)
            => s_userEvent?.Invoke((uint)d0);


        public sealed class ElfImage : IDisposable {

            public enum SymbolType {
                None = 0,
                Object = 1,
                Function = 2,
                Section = 3,
            }

#pragma warning disable 0414
            private byte[] imageData;
            private uint address;
            private uint size;
            private uint regionCount;
            private bool disposed;
#pragma warning restore 0414

            public uint Address => this.address;
            public uint Size => this.size;
            public uint RegionCount => this.regionCount;

            public ElfImage(byte[] elfImageData) {
                if (elfImageData == null) throw new ArgumentNullException(nameof(elfImageData));

                this.imageData = elfImageData;
                this.address = 0;
                this.size = 0;
                this.regionCount = 0;

                this.NativeLoadElf(this.imageData);
            }

            ~ElfImage() => this.Dispose(false);

            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing) {
                if (this.disposed) return;

                this.NativeUnloadElf();
                this.imageData = null;
                this.disposed = true;
            }

            public uint FindSymbolAddress(string name, SymbolType type) {
                if (name == null) throw new ArgumentNullException(nameof(name));

                return this.NativeFindSymbolAddress(this.imageData, name, type);
            }

            public NativeFunction FindFunction(string name) {
                if (name == null) throw new ArgumentNullException(nameof(name));

                return new NativeFunction(this.FindSymbolAddress(name, SymbolType.Function));
            }

            public void InitializeBssRegion() => this.InitializeBssRegion("__bss_start__", "__bss_end__");

            public void InitializeBssRegion(string startSymbolName, string endSymbolName) {
                if (startSymbolName == null) throw new ArgumentNullException(nameof(startSymbolName));
                if (endSymbolName == null) throw new ArgumentNullException(nameof(endSymbolName));

                var start = this.FindSymbolAddress(startSymbolName, SymbolType.None);
                var end = this.FindSymbolAddress(endSymbolName, SymbolType.None);

                this.ZeroRegion(start, end - start);
            }

            public void ZeroRegion(uint address, uint length) => this.NativeZeroRegion(address, length);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeLoadElf(byte[] image);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeUnloadElf();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern uint NativeFindSymbolAddress(byte[] image, string name, SymbolType type);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeZeroRegion(uint address, uint length);
        }


        public sealed class NativeFunction : IDisposable {

#pragma warning disable 0414
            private uint address;
            private bool sizeSet;
            private int argumentCount;
            private uint nativeParameterPool;
            private uint nativeParameterList;
            private uint nativeIndex;
            private bool disposed;
#pragma warning restore 0414

            public uint Address => this.address;

            public NativeFunction(uint address) {
                this.address = address;
                this.sizeSet = false;
                this.argumentCount = 0;
                this.nativeParameterPool = 0;
                this.nativeParameterList = 0;
                this.nativeIndex = 0;
            }

            ~NativeFunction() => this.Dispose(false);

            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing) {
                if (this.disposed) return;

                this.NativeDispose();
                this.disposed = true;
            }

            public int Invoke(params object[] arguments) {
                if (arguments == null) arguments = new object[0];

                if (!this.sizeSet) {
                    this.NativeSetSize(arguments.Length);
                    this.argumentCount = arguments.Length;
                    this.sizeSet = true;
                }
                else if (this.argumentCount != arguments.Length) {
                    throw new InvalidOperationException("Argument count must match the first invocation.");
                }

                for (var i = 0; i < arguments.Length; i++) {
                    var argument = arguments[i];
                    var type = argument.GetType();

                    if (type == typeof(byte)) this.NativeAddArgument((byte)argument);
                    else if (type == typeof(sbyte)) this.NativeAddArgument((sbyte)argument);
                    else if (type == typeof(ushort)) this.NativeAddArgument((ushort)argument);
                    else if (type == typeof(short)) this.NativeAddArgument((short)argument);
                    else if (type == typeof(uint)) this.NativeAddArgument((uint)argument);
                    else if (type == typeof(int)) this.NativeAddArgument((int)argument);
                    else if (type == typeof(ulong)) this.NativeAddArgument((ulong)argument);
                    else if (type == typeof(long)) this.NativeAddArgument((long)argument);
                    else if (type == typeof(float)) this.NativeAddArgument((float)argument);
                    else if (type == typeof(double)) this.NativeAddArgument((double)argument);
                    else if (type == typeof(bool)) this.NativeAddArgumentBool((bool)argument);
                    else if (type == typeof(byte[])) this.NativeAddArgument((byte[])argument);
                    else if (type == typeof(sbyte[])) this.NativeAddArgument((sbyte[])argument);
                    else if (type == typeof(ushort[])) this.NativeAddArgument((ushort[])argument);
                    else if (type == typeof(short[])) this.NativeAddArgument((short[])argument);
                    else if (type == typeof(uint[])) this.NativeAddArgument((uint[])argument);
                    else if (type == typeof(int[])) this.NativeAddArgument((int[])argument);
                    else if (type == typeof(ulong[])) this.NativeAddArgument((ulong[])argument);
                    else if (type == typeof(long[])) this.NativeAddArgument((long[])argument);
                    else if (type == typeof(float[])) this.NativeAddArgument((float[])argument);
                    else if (type == typeof(double[])) this.NativeAddArgument((double[])argument);
                    else if (type == typeof(bool[])) this.NativeAddArgumentBool((bool[])argument);
                    else throw new ArgumentException("Unsupported argument type: " + type.FullName);
                }

                return this.NativeInvoke();
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeDispose();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int NativeInvoke();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeSetSize(int size);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(byte argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(sbyte argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(ushort argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(short argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(uint argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(int argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(ulong argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(long argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(float argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(double argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgumentBool(bool argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(byte[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(sbyte[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(ushort[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(short[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(uint[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(int[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(ulong[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(long[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(float[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgument(double[] argument);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void NativeAddArgumentBool(bool[] argument);
        }
    }
}
