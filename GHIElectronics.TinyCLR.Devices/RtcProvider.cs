using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.Rtc.Provider {
    public interface IRtcControllerProvider {
        DateTime Now { get; set; }
    }

    public sealed class RtcControllerProvider : IRtcControllerProvider {
        private readonly static Hashtable providers = new Hashtable();
        private readonly IntPtr nativeProvider;

        public string Name { get; }

        private RtcControllerProvider(string name) {
            var api = Api.Find(name, ApiType.RtcProvider);

            this.Name = name;
            this.nativeProvider = api.Implementation;

            this.Acquire();
        }

        ~RtcControllerProvider() => this.Release();

        public static IRtcControllerProvider FromId(string id) {
            if (RtcControllerProvider.providers.Contains(id))
                return (IRtcControllerProvider)RtcControllerProvider.providers[id];

            var res = new RtcControllerProvider(id);

            RtcControllerProvider.providers[id] = res;

            return res;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Acquire();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Release();

        public extern DateTime Now {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;

            [MethodImpl(MethodImplOptions.InternalCall)]
            set;
        }
    }
}