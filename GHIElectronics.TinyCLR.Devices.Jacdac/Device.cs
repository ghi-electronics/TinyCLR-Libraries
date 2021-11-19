using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.Jacdac {
    public class Device {
        byte[] services;
        int lastSeen;
        int lastServiceUpdate;
        byte[] currentReading;
        string shortId;
        string deviceId;
        public Device() {

        }

        public uint ServiceAt(uint idx) {
            if (idx == 0)
                return 0;

            idx <<= 2;

            if (this.services == null || idx + 4 > this.services.Length)
                return 0xFFFFFFFF;

            return Util.Read32(this.services, (int)idx);
        }

    }
}
