using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.Can.Provider {
    public interface ICanProvider {
        ICanControllerProvider[] GetControllers();
    }

    public interface ICanControllerProvider : IDisposable {
        void Reset();
        int ReadMessages(CanMessage[] messages, int offset, int count);
        int WriteMessages(CanMessage[] messages, int offset, int count);
        void SetBitTiming(CanBitTiming bitTiming);
        void SetExplicitFilters(uint[] filters);
        void SetGroupFilters(uint[] lowerBounds, uint[] upperBounds);
        void ClearReadBuffer();
        void SetReadBufferSize(int readBufferSize);

        int UnreadMessageCount { get; }
        bool IsWritingAllowed { get; }
        int ReadErrorCount { get; }
        int WriteErrorCount { get; }
        uint SourceClock { get; }
    }

    public class CanProvider : ICanProvider {
        private readonly ICanControllerProvider[] controllers;
        private static Hashtable providers = new Hashtable();

        public string Name { get; }

        public ICanControllerProvider[] GetControllers() => this.controllers;

        private CanProvider(string name) {
            var api = Api.Find(name, ApiType.CanProvider);

            this.Name = name;
            this.controllers = new ICanControllerProvider[api.Count];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultCanControllerProvider(api.Implementation[i]);
        }

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

        internal DefaultCanControllerProvider(IntPtr nativeProvider) {
            this.nativeProvider = nativeProvider;

            this.NativeAcquire();
        }

        ~DefaultCanControllerProvider() => this.Dispose();

        public void Dispose() {
            if (!this.disposed) {
                this.NativeRelease();
                GC.SuppressFinalize(this);
                this.disposed = true;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAcquire();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRelease();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Reset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int ReadMessages(CanMessage[] messages, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int WriteMessages(CanMessage[] messages, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetBitTiming(CanBitTiming bitTiming);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetExplicitFilters(uint[] filters);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetGroupFilters(uint[] lowerBounds, uint[] upperBounds);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void ClearReadBuffer();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetReadBufferSize(int readBufferSize);

        public extern int UnreadMessageCount {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern bool IsWritingAllowed {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern int ReadErrorCount {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern int WriteErrorCount {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern uint SourceClock {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
