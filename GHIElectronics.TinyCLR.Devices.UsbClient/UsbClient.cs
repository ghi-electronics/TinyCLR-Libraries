using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.UsbClient.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.UsbClient {
    public enum DeviceState {
        Detached = 0,
        Attached = 1,
        Powered = 2,
        Default = 3,
        Address = 4,
        Configured = 5,
        Suspended = 6,
    };

    public enum UsbClientMode {
        Cdc = 0,
        WinUsb = 1
    }

    public delegate void DataReceivedEventHandler(UsbClientController sender, uint count);
    public delegate void DeviceStateChangedEventHandler(UsbClientController sender, DeviceState state);

    public sealed class UsbClientController : IDisposable {
        private DataReceivedEventHandler dataReceivedCallbacks;
        private DeviceStateChangedEventHandler deviceStateChangedCallbacks;

        public IUsbClientControllerProvider Provider { get; }

        private UsbClientController(IUsbClientControllerProvider provider) => this.Provider = provider;

        public static UsbClientController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UsbClientController) is UsbClientController c ? c : UsbClientController.FromName(NativeApi.GetDefaultName(NativeApiType.UsbClientController));
        public static UsbClientController FromName(string name) => UsbClientController.FromProvider(new UsbClientControllerApiWrapper(NativeApi.Find(name, NativeApiType.UsbClientController)));
        public static UsbClientController FromProvider(IUsbClientControllerProvider provider) => new UsbClientController(provider);

        public void Dispose() => this.Provider.Dispose();

        public int ByteToRead => this.Provider.ByteToRead;
        public int ByteToWrite => this.Provider.ByteToWrite;
        public int WriteBufferSize { get => this.Provider.WriteBufferSize; set => this.Provider.WriteBufferSize = value; }
        public int ReadBufferSize { get => this.Provider.ReadBufferSize; set => this.Provider.ReadBufferSize = value; }

        public void SetActiveSetting(UsbClientMode mode, ushort productId, ushort vendorId) => this.SetActiveSetting(mode, null, null, null, productId, vendorId);
        public void SetActiveSetting(UsbClientMode mode, ushort productId, ushort vendorId, string guid) => this.SetActiveSetting(mode, null, null, null, productId, vendorId, guid);
        public void SetActiveSetting(UsbClientMode mode, string manufactureName, string productName, string serialNumber, ushort productId, ushort vendorId) => this.SetActiveSetting(mode, manufactureName, productName, serialNumber, productId, vendorId, null);

        public void SetActiveSetting(UsbClientMode mode, string manufactureName, string productName, string serialNumber, ushort productId, ushort vendorId, string guid) {
            if (mode == UsbClientMode.WinUsb && guid == null)
                throw new ArgumentNullException(nameof(guid));

            this.Provider.SetActiveSetting(mode, manufactureName, productName, serialNumber, productId, vendorId, guid);
        }

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();
        public int Read(byte[] data) => this.Read(data, 0, data.Length);

        public int Read(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data));

            return this.Provider.Read(data, offset, count);
        }

        public int Write(byte[] data) => this.Write(data, 0, data.Length);
        public int Write(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data));

            return this.Provider.Write(data, offset, count);
        }

        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();
        public void ClearWriteBuffer() => this.Provider.ClearWriteBuffer();
        public void Flush() => this.Provider.Flush();

        public DeviceState DeviceState => this.Provider.DeviceState;

        private void OnDataReceived(UsbClientController sender, uint count) => this.dataReceivedCallbacks?.Invoke(this, count);
        private void OnDeviceStateChanged(UsbClientController sender, DeviceState state) => this.deviceStateChangedCallbacks?.Invoke(this, state);

        public event DataReceivedEventHandler DataReceived {
            add {
                if (this.dataReceivedCallbacks == null)
                    this.Provider.DataReceived += this.OnDataReceived;

                this.dataReceivedCallbacks += value;
            }
            remove {
                this.dataReceivedCallbacks -= value;

                if (this.dataReceivedCallbacks == null)
                    this.Provider.DataReceived -= this.OnDataReceived;
            }
        }

        public event DeviceStateChangedEventHandler DeviceStateChanged {
            add {
                if (this.deviceStateChangedCallbacks == null)
                    this.Provider.DeviceStateChanged += this.OnDeviceStateChanged;

                this.deviceStateChangedCallbacks += value;
            }
            remove {
                this.deviceStateChangedCallbacks -= value;

                if (this.deviceStateChangedCallbacks == null)
                    this.Provider.DeviceStateChanged -= this.OnDeviceStateChanged;
            }
        }

    }

    namespace Provider {
        public interface IUsbClientControllerProvider : IDisposable {
            int ByteToRead { get; }
            int ByteToWrite { get; }

            int WriteBufferSize { get; set; }
            int ReadBufferSize { get; set; }

            DeviceState DeviceState { get; }

            void Enable();
            void Disable();

            void SetActiveSetting(UsbClientMode mode, string manufactureName, string productName, string serialNumber, ushort productId, ushort vendorId, string guid);

            int Read(byte[] data, int offset, int count);
            int Write(byte[] data, int offset, int count);

            void ClearReadBuffer();
            void ClearWriteBuffer();

            void Flush();


            event DataReceivedEventHandler DataReceived;
            event DeviceStateChangedEventHandler DeviceStateChanged;
        }

        public sealed class UsbClientControllerApiWrapper : IUsbClientControllerProvider {
            private readonly IntPtr impl;

            private readonly NativeEventDispatcher dataReceivedDispatcher;
            private readonly NativeEventDispatcher deviceStateChangedDispatcher;

            private DataReceivedEventHandler dataReceivedCallbacks;
            private DeviceStateChangedEventHandler deviceStateChangedCallbacks;

            public NativeApi Api { get; }

            public UsbClientControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.dataReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.UsbClient.DataReceived");
                this.dataReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.dataReceivedCallbacks?.Invoke(null, (uint)d0); };

                this.deviceStateChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.UsbClient.DeviceStateChanged");
                this.deviceStateChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.deviceStateChangedCallbacks?.Invoke(null, (DeviceState)d0); };

            }

            public event DataReceivedEventHandler DataReceived {
                add {
                    if (this.dataReceivedCallbacks == null)
                        this.SetDataReceivedEventEnabled(true);

                    this.dataReceivedCallbacks += value;
                }
                remove {
                    this.dataReceivedCallbacks -= value;

                    if (this.dataReceivedCallbacks == null)
                        this.SetDataReceivedEventEnabled(false);
                }
            }

            public event DeviceStateChangedEventHandler DeviceStateChanged {
                add {
                    if (this.deviceStateChangedCallbacks == null)
                        this.SetDataStateChangedEventEnabled(true);

                    this.deviceStateChangedCallbacks += value;
                }
                remove {
                    this.deviceStateChangedCallbacks -= value;

                    if (this.deviceStateChangedCallbacks == null)
                        this.SetDataStateChangedEventEnabled(false);
                }
            }

            public void Dispose() => this.Release();

            public int ByteToRead => this.GetByteToRead();
            public int ByteToWrite => this.GetByteToWrite();

            public DeviceState DeviceState => this.GetDeviceState();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSetting(UsbClientMode mode, string manufactureName, string productName, string serialNumber, ushort productId, ushort vendorId, string guid);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(byte[] data, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(byte[] data, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern DeviceState GetDeviceState();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int GetByteToRead();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern int GetByteToWrite();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetDataReceivedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetDataStateChangedEventEnabled(bool enabled);

            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }
            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

        }
    }
}
