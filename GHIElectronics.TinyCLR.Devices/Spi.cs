using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;

namespace GHIElectronics.TinyCLR.Devices.Spi {
    public sealed class SpiController : IDisposable {
        private SpiDevice active;

        public ISpiControllerProvider Provider { get; }

        private SpiController(ISpiControllerProvider provider) => this.Provider = provider;

        public static SpiController GetDefault() => Api.GetDefaultFromCreator(ApiType.SpiController) is SpiController c ? c : SpiController.FromName(Api.GetDefaultName(ApiType.SpiController));
        public static SpiController FromName(string name) => SpiController.FromProvider(new SpiControllerApiWrapper(Api.Find(name, ApiType.SpiController)));
        public static SpiController FromProvider(ISpiControllerProvider provider) => new SpiController(provider);

        public void Dispose() => this.Provider.Dispose();

        public SpiDevice GetDevice(SpiConnectionSettings connectionSettings) => new SpiDevice(this, connectionSettings);

        public uint ChipSelectLineCount => this.Provider.ChipSelectLineCount;
        public uint MinClockFrequency => this.Provider.MinClockFrequency;
        public uint MaxClockFrequency => this.Provider.MaxClockFrequency;
        public uint[] SupportedDataBitLengths => this.Provider.SupportedDataBitLengths;

        internal void SetActive(SpiDevice device) {
            if (this.active != device) {
                this.active = device;

                this.Provider.SetActiveSettings(device.ConnectionSettings);
            }
        }
    }

    public sealed class SpiDevice : IDisposable {
        public SpiConnectionSettings ConnectionSettings { get; }
        public SpiController Controller { get; }

        internal SpiDevice(SpiController controller, SpiConnectionSettings connectionSettings) {
            this.ConnectionSettings = connectionSettings;
            this.Controller = controller;
        }

        public void Dispose() {

        }

        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer.Length);
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) => this.TransferFullDuplex(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) => this.TransferSequential(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        public void Read(byte[] buffer, int offset, int length) => this.WriteRead(null, 0, 0, buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.WriteRead(buffer, offset, length, null, 0, 0);
        public void TransferFullDuplex(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);

        public void TransferSequential(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            this.WriteRead(writeBuffer, writeOffset, writeLength, null, 0, 0, false);
            this.WriteRead(null, 0, 0, readBuffer, readOffset, readLength);
        }

        private void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter = true) {
            this.Controller.SetActive(this);

            this.Controller.Provider.WriteRead(writeBuffer, (uint)writeOffset, (uint)writeLength, readBuffer, (uint)readOffset, (uint)readLength, deselectAfter);
        }
    }

    public sealed class SpiConnectionSettings {
        public bool UseControllerChipSelect { get; set; } = false;
        public uint ChipSelectLine { get; set; }
        public uint ClockFrequency { get; set; } = 1_000_000;
        public uint DataBitLength { get; set; } = 8;
        public SpiMode Mode { get; set; } = SpiMode.Mode0;

        public SpiConnectionSettings(uint chipSelectLine) => this.ChipSelectLine = chipSelectLine;
    }

    public enum SpiMode {
        Mode0 = 0,
        Mode1 = 1,
        Mode2 = 2,
        Mode3 = 3,
    }

    namespace Provider {
        public interface ISpiControllerProvider : IDisposable {
            uint ChipSelectLineCount { get; }
            uint MinClockFrequency { get; }
            uint MaxClockFrequency { get; }
            uint[] SupportedDataBitLengths { get; }

            void SetActiveSettings(SpiConnectionSettings connectionSettings);
            void WriteRead(byte[] writeBuffer, uint writeOffset, uint writeLength, byte[] readBuffer, uint readOffset, uint readLength, bool deselectAfter);
        }

        public sealed class SpiControllerApiWrapper : ISpiControllerProvider {
            private readonly IntPtr impl;

            public Api Api { get; }

            public SpiControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            public void SetActiveSettings(SpiConnectionSettings connectionSettings) => this.SetActiveSettings(connectionSettings.ChipSelectLine, connectionSettings.UseControllerChipSelect, connectionSettings.ClockFrequency, connectionSettings.DataBitLength, connectionSettings.Mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern uint ChipSelectLineCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern uint MinClockFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern uint MaxClockFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern uint[] SupportedDataBitLengths { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetActiveSettings(uint chipSelectLine, bool useControllerChipSelect, uint clockFrequency, uint dataBitLength, SpiMode mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void WriteRead(byte[] writeBuffer, uint writeOffset, uint writeLength, byte[] readBuffer, uint readOffset, uint readLength, bool deselectAfter);
        }
    }
}
