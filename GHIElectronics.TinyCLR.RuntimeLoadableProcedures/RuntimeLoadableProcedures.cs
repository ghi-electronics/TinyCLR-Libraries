using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.RuntimeLoadableProcedures {
    /// <summary>
    /// Runtime Loadable Procedures (RLP) — load a compiled ELF blob onto the
    /// device at runtime, look up symbols by name, and call native C functions
    /// from managed code. Useful for shipping hardware-accelerated routines
    /// (DSP, image processing) without rebuilding the firmware.
    /// </summary>
    public static class RuntimeLoadableProcedures {

        /// <summary>Handler signature for <see cref="NativeEvent"/>. The argument is whatever the native code passed to <c>RLP_PostManagedEvent</c>.</summary>
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

        /// <summary>Raised when native RLP code calls <c>RLP_PostManagedEvent</c>. Runs on a managed thread, never in ISR context.</summary>
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


        /// <summary>
        /// A loaded ELF binary. Construct with the ELF bytes, look up symbols by
        /// name, then build a <see cref="NativeFunction"/> from a function-symbol's
        /// address to call it.
        /// </summary>
        public sealed class ElfImage : IDisposable {

            /// <summary>Classification of an ELF symbol.</summary>
            public enum SymbolType {
                /// <summary>Untyped or section symbol.</summary>
                None = 0,
                /// <summary>Data symbol (variable, array).</summary>
                Object = 1,
                /// <summary>Code symbol (function).</summary>
                Function = 2,
                /// <summary>Section symbol.</summary>
                Section = 3,
            }

#pragma warning disable 0414
            private byte[] imageData;
            private uint address;
            private uint size;
            private uint regionCount;
            private bool disposed;
#pragma warning restore 0414

            /// <summary>Start address of the loaded image in target memory.</summary>
            public uint Address => this.address;
            /// <summary>Total size of the loaded image.</summary>
            public uint Size => this.size;
            /// <summary>Number of distinct ELF regions loaded.</summary>
            public uint RegionCount => this.regionCount;

            /// <summary>Loads an ELF image into device memory.</summary>
            /// <param name="elfImageData">ELF binary bytes.</param>
            public ElfImage(byte[] elfImageData) {
                if (elfImageData == null) throw new ArgumentNullException(nameof(elfImageData));

                this.imageData = elfImageData;
                this.address = 0;
                this.size = 0;
                this.regionCount = 0;

                this.NativeLoadElf(this.imageData);
            }

            /// <summary>Finalizer; ensures the image is unloaded.</summary>
            ~ElfImage() => this.Dispose(false);

            /// <summary>Unloads the image from device memory.</summary>
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

            /// <summary>Looks up a symbol by name and type.</summary>
            /// <param name="name">Symbol name.</param>
            /// <param name="type">Symbol classification.</param>
            /// <returns>Symbol's runtime address, or 0 if not found.</returns>
            public uint FindSymbolAddress(string name, SymbolType type) {
                if (name == null) throw new ArgumentNullException(nameof(name));

                return this.NativeFindSymbolAddress(this.imageData, name, type);
            }

            /// <summary>Locates a function symbol and wraps it in a <see cref="NativeFunction"/>.</summary>
            public NativeFunction FindFunction(string name) {
                if (name == null) throw new ArgumentNullException(nameof(name));

                return new NativeFunction(this.FindSymbolAddress(name, SymbolType.Function));
            }

            /// <summary>Zero-initializes the loaded image's BSS region using the standard <c>__bss_start__</c>/<c>__bss_end__</c> symbols.</summary>
            public void InitializeBssRegion() => this.InitializeBssRegion("__bss_start__", "__bss_end__");

            /// <summary>Zero-initializes the region delimited by two symbols.</summary>
            /// <param name="startSymbolName">Symbol marking the start of the region.</param>
            /// <param name="endSymbolName">Symbol marking the end of the region.</param>
            public void InitializeBssRegion(string startSymbolName, string endSymbolName) {
                if (startSymbolName == null) throw new ArgumentNullException(nameof(startSymbolName));
                if (endSymbolName == null) throw new ArgumentNullException(nameof(endSymbolName));

                var start = this.FindSymbolAddress(startSymbolName, SymbolType.None);
                var end = this.FindSymbolAddress(endSymbolName, SymbolType.None);

                this.ZeroRegion(start, end - start);
            }

            /// <summary>Zero-fills a span of <paramref name="length"/> bytes starting at <paramref name="address"/>.</summary>
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


        /// <summary>
        /// Wraps a native function located at a known address. The first call to
        /// <see cref="Invoke"/> fixes the argument count and types; subsequent calls
        /// must match. Supported argument types: 8/16/32/64-bit integers (signed and
        /// unsigned), float, double, bool, and arrays of those types.
        /// </summary>
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

            /// <summary>Runtime address of the function.</summary>
            public uint Address => this.address;

            /// <summary>Constructs a <see cref="NativeFunction"/> pointing at the given address.</summary>
            public NativeFunction(uint address) {
                this.address = address;
                this.sizeSet = false;
                this.argumentCount = 0;
                this.nativeParameterPool = 0;
                this.nativeParameterList = 0;
                this.nativeIndex = 0;
            }

            /// <summary>Finalizer; releases interop resources.</summary>
            ~NativeFunction() => this.Dispose(false);

            /// <summary>Releases interop resources associated with this function.</summary>
            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing) {
                if (this.disposed) return;

                this.NativeDispose();
                this.disposed = true;
            }

            /// <summary>
            /// Calls the native function with the given arguments and returns its
            /// 32-bit return value. The first invocation locks the argument count;
            /// subsequent calls must pass the same number of arguments.
            /// </summary>
            /// <param name="arguments">Arguments to pass. Supported types: integer (any width, signed or unsigned), float, double, bool, and arrays of those.</param>
            /// <returns>The function's int return value.</returns>
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
