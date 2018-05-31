using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.Can.Provider {
    public interface ICanProvider {
        ICanControllerProvider GetController(int idx);
    }

    public interface ICanControllerProvider : IDisposable {
        void Reset(int controller);
        int ReadMessages(int controller, CanMessage[] messages, int offset, int count);
        int WriteMessages(int controller, CanMessage[] messages, int offset, int count);
        void SetBitTiming(int controller, CanBitTiming bitTiming);
        void SetExplicitFilters(int controller, uint[] filters);
        void SetGroupFilters(int controller, uint[] lowerBounds, uint[] upperBounds);
        void ClearReadBuffer(int controller);
        void ClearWriteBuffer(int controller);

        int UnreadMessageCount(int controller);
        int UnwrittenMessageCount(int controller);
        bool IsWritingAllowed(int controller);
        int ReadErrorCount(int controller);
        int WriteErrorCount(int controller);
        uint SourceClock(int controller);
    }

    public class CanProvider : ICanProvider {
        private ICanControllerProvider controllers;
        private static Hashtable providers = new Hashtable();

        public string Name { get; }

        public ICanControllerProvider GetController(int idx) {
            var api = Api.Find(this.Name, ApiType.CanProvider);

            if (idx >= api.Count)
                throw new ArgumentException("Controller id is out of array");

            this.controllers = new DefaultCanControllerProvider(api.Implementation[0], idx);

            return this.controllers;
        }

        private CanProvider(string name) => this.Name = name;

        public static ICanProvider FromId(string id) {
            if (CanProvider.providers.Contains(id))
                return (ICanProvider)CanProvider.providers[id];

            var res = new CanProvider(id);

            CanProvider.providers[id] = res;

            return res;
        }
    }

    internal class DefaultCanControllerProvider : ICanControllerProvider {
#pragma warning disable CS0169
        private readonly IntPtr nativeProvider;
#pragma warning restore CS0169
        private bool disposed = false;

        int controllerId;

        internal DefaultCanControllerProvider(IntPtr nativeProvider, int controllerId) {
            this.nativeProvider = nativeProvider;
            this.controllerId = controllerId;

            this.NativeAcquire(controllerId);
        }

        ~DefaultCanControllerProvider() => this.Dispose();

        public void Dispose() {
            if (!this.disposed) {
                this.NativeRelease(this.controllerId);
                GC.SuppressFinalize(this);
                this.disposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAcquire(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRelease(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Reset(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int ReadMessages(int controller, CanMessage[] messages, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int WriteMessages(int controller, CanMessage[] messages, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetBitTiming(int controller, CanBitTiming bitTiming);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetExplicitFilters(int controller, uint[] filters);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetGroupFilters(int controller, uint[] lowerBounds, uint[] upperBounds);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void ClearReadBuffer(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void ClearWriteBuffer(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int UnreadMessageCount(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int UnwrittenMessageCount(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern bool IsWritingAllowed(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int ReadErrorCount(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int WriteErrorCount(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern uint SourceClock(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern uint GetReadBufferSize(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetReadBufferSize(int controller, int size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern uint GetWriteBufferSize(int controller);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetWriteBufferSize(int controller, int size);
    }
}
