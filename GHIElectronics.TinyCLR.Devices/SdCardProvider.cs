using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.SdCard.Provider {
    public interface ISdCardProvider {
        ISdCardControllerProvider[] GetControllers();
    }

    public interface ISdCardControllerProvider {
        IntPtr Hdc { get; }

        int ControllerIndex { get; }
    }

    public interface ISdCardLowLevelController {
        int ReadSectors(int sector, int count, byte[] buffer, int offset, int timeout);
        int WriteSectors(int sector, int count, byte[] buffer, int offset, int timeout);
        int EraseSectors(int sector, int count, int timeout);
        void GetSectorMap(out int[] sizes, out int count, out bool uniform);
    }

    public class SdCardProvider : ISdCardProvider {
        private ISdCardControllerProvider[] controllers;

        private static Hashtable providers = new Hashtable();

        public string Name { get; }

        public ISdCardControllerProvider[] GetControllers() => this.controllers;

        private SdCardProvider(string name) {

            this.Name = name;

            var api = Api.Find(this.Name, ApiType.SdCardProvider);

            this.controllers = new ISdCardControllerProvider[DefaultSdCardControllerProvider.GetControllerCount(api.Implementation)];

            for (var i = 0; i < this.controllers.Length; i++)

                this.controllers[i] = new DefaultSdCardControllerProvider(api.Implementation, i);
        }

        public static ISdCardProvider FromId(string id) {

            if (SdCardProvider.providers.Contains(id))
                return (ISdCardProvider)SdCardProvider.providers[id];

            var res = new SdCardProvider(id);

            SdCardProvider.providers[id] = res;

            return res;
        }

    }

    internal class DefaultSdCardControllerProvider : ISdCardControllerProvider, ISdCardLowLevelController {

#pragma warning disable CS0169
        private readonly IntPtr nativeProvider;
        private readonly int idx;
#pragma warning restore CS0169

        internal DefaultSdCardControllerProvider(IntPtr nativeProvider, int idx) {
            this.nativeProvider = nativeProvider;
            this.idx = idx;
        }

        public IntPtr Hdc => this.nativeProvider;
        public int ControllerIndex => this.idx;

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern static int GetControllerCount(IntPtr nativeProvider);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int ReadSectors(int sector, int count, byte[] buffer, int offset, int timeout);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int WriteSectors(int sector, int count, byte[] buffer, int offset, int timeout);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int EraseSectors(int sector, int count, int timeout);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void GetSectorMap(out int[] sizes, out int count, out bool uniform);

    }
}
