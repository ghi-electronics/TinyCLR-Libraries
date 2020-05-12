using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.UsbHost.Provider;
using GHIElectronics.TinyCLR.Native;
using static GHIElectronics.TinyCLR.Devices.UsbHost.BaseDevice;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    public enum DeviceConnectionStatus {
        Disconnected = 0,
        Connected = 1,
        Bad = 2,
    };


    public delegate void OnConnectionChanged(UsbHostController sender, DeviceConnectionEventArgs e);

    public class DeviceConnectionEventArgs : EventArgs {
        private readonly uint id;
        private readonly byte interfaceIndex;
        private readonly BaseDevice.DeviceType type;
        private readonly ushort vendorId;
        private readonly ushort productId;
        private readonly byte portNumber;
        private readonly DeviceConnectionStatus deviceStatus;

        /// <summary>The device id.</summary>
        public uint Id => this.id;

        /// <summary>The logical device interface index.</summary>
        public byte InterfaceIndex => this.interfaceIndex;

        /// <summary>The device's type.</summary>
        public BaseDevice.DeviceType Type => this.type;

        /// <summary>The devic's vendor id.</summary>
        public ushort VendorId => this.vendorId;

        /// <summary>The device's product id.</summary>
        public ushort ProductId => this.productId;

        /// <summary>The device's USB port number.</summary>
        public byte PortNumber => this.portNumber;

        public DeviceConnectionStatus DeviceStatus => this.deviceStatus;


        internal DeviceConnectionEventArgs(uint id, byte interfaceIndex, BaseDevice.DeviceType type, ushort vendorId, ushort productId, byte portNumber, DeviceConnectionStatus deviceStatus) {
            this.id = id;
            this.interfaceIndex = interfaceIndex;
            this.type = type;
            this.vendorId = vendorId;
            this.productId = productId;
            this.portNumber = portNumber;
            this.deviceStatus = deviceStatus;
        }
    }

    public sealed class UsbHostController : IDisposable {

        private static bool started;
        private static ArrayList devices;
        private static object listLock;


        private OnConnectionChanged onConnectionChangedCallbacks;

        public IUsbHostControllerProvider Provider { get; }

        private UsbHostController(IUsbHostControllerProvider provider) {
            this.Provider = provider;

            devices = new ArrayList();
            started = false;
            listLock = new object();
        }

        public static UsbHostController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UsbHostController) is UsbHostController c ? c : UsbHostController.FromName(NativeApi.GetDefaultName(NativeApiType.UsbHostController));
        public static UsbHostController FromName(string name) => UsbHostController.FromProvider(new UsbHostControllerApiWrapper(NativeApi.Find(name, NativeApiType.UsbHostController)));
        public static UsbHostController FromProvider(IUsbHostControllerProvider provider) => new UsbHostController(provider);

        public void Dispose() => this.Provider.Dispose();

        public void Enable() {
            this.Provider.Enable();
            started = true;
        }
        public void Disable() {
            started = false;
            this.Provider.Disable();

        }

        public static BaseDevice[] GetConnectedDevices() {
            if (started == false)
                return null;

            lock (listLock)
                return (BaseDevice[])devices.ToArray(typeof(BaseDevice));
        }

        internal static void RegisterDevice(BaseDevice device) {
            lock (listLock)
                devices.Add(device);
        }

        private static void OnDisconnect(object sender, DeviceConnectionEventArgs e) {
            lock (listLock) {
                var newList = new ArrayList();

                foreach (BaseDevice d in devices) {
                    if (d.Id == e.Id) {
                        d.OnDisconnected();
                        d.Dispose();
                    }
                    else {
                        newList.Add(d);
                    }
                }

                devices = newList;
            }
        }


        private void OnConnectionChangedCallBack(UsbHostController sender, DeviceConnectionEventArgs e) {
            if (e.DeviceStatus == DeviceConnectionStatus.Disconnected) {
                OnDisconnect(sender, e);
            }

            this.onConnectionChangedCallbacks?.Invoke(this, e);
        }


        public event OnConnectionChanged OnConnectionChangedEvent {
            add {
                if (this.onConnectionChangedCallbacks == null)
                    this.Provider.OnConnectionChangedEvent += this.OnConnectionChangedCallBack;

                this.onConnectionChangedCallbacks += value;
            }
            remove {
                this.onConnectionChangedCallbacks -= value;

                if (this.onConnectionChangedCallbacks == null)
                    this.Provider.OnConnectionChangedEvent -= this.OnConnectionChangedCallBack;
            }
        }

    }

    namespace Provider {
        public interface IUsbHostControllerProvider : IDisposable {
            void Enable();
            void Disable();


            event OnConnectionChanged OnConnectionChangedEvent;
        }

        public sealed class UsbHostControllerApiWrapper : IUsbHostControllerProvider {
            private readonly IntPtr impl;

            private readonly NativeEventDispatcher onConnectDispatcher;

            private OnConnectionChanged onConnectionChangedCallbacks;

            public NativeApi Api { get; }

            public UsbHostControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.onConnectDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.UsbHost.OnConnectionChanged");
                this.onConnectDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => {
                    if (this.Api.Name == apiName) {

                        var id = (uint)d0;

                        var connection = (DeviceConnectionStatus)d3;
                        var interfaceIndex = (byte)d1;
                        var deviceType = (DeviceType)d2;

                        GetDeviceInformation(id, out var vendor, out var product, out var port);

                        var deviceConnectedEventArgs = new DeviceConnectionEventArgs(id, interfaceIndex, deviceType, vendor, product, port, connection);


                        this.onConnectionChangedCallbacks?.Invoke(null, deviceConnectedEventArgs);
                    }
                };


            }

            public event OnConnectionChanged OnConnectionChangedEvent {
                add {
                    if (this.onConnectionChangedCallbacks == null)
                        this.OnConnectionChangedEventEnabled(true);

                    this.onConnectionChangedCallbacks += value;
                }
                remove {
                    this.onConnectionChangedCallbacks -= value;

                    if (this.onConnectionChangedCallbacks == null)
                        this.OnConnectionChangedEventEnabled(false);
                }
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            internal static extern void GetDeviceInformation(uint id, out ushort vendor, out ushort product, out byte port);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void OnConnectionChangedEventEnabled(bool enabled);

        }
    }
}
