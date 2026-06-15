using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.UsbHost.Provider;
using GHIElectronics.TinyCLR.Native;
using static GHIElectronics.TinyCLR.Devices.UsbHost.BaseDevice;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    /// <summary>The connection status of a USB device.</summary>
    public enum DeviceConnectionStatus {
        /// <summary>The device has been disconnected.</summary>
        Disconnected = 0,
        /// <summary>The device has been connected.</summary>
        Connected = 1,
        /// <summary>The device was connected but is not functioning correctly.</summary>
        Bad = 2,
    };


    /// <summary>The delegate for when a device's connection status changes.</summary>
    public delegate void OnConnectionChanged(UsbHostController sender, DeviceConnectionEventArgs e);

    /// <summary>The event arguments for a device connection change.</summary>
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

        /// <summary>The device's vendor id.</summary>
        public ushort VendorId => this.vendorId;

        /// <summary>The device's product id.</summary>
        public ushort ProductId => this.productId;

        /// <summary>The device's USB port number.</summary>
        public byte PortNumber => this.portNumber;

        /// <summary>The device's connection status.</summary>
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

    /// <summary>Represents the USB host controller used to manage connected USB devices.</summary>
    public class UsbHostController : IDisposable {

        private static bool started;
        private static ArrayList devices;
        private static object listLock;


        private OnConnectionChanged onConnectionChangedCallbacks;

        /// <summary>The underlying provider that implements the host controller.</summary>
        public IUsbHostControllerProvider Provider { get; }

        private UsbHostController(IUsbHostControllerProvider provider) {
            this.Provider = provider;

            devices = new ArrayList();
            started = false;
            listLock = new object();
        }

        /// <summary>Gets the default USB host controller for the system.</summary>
        public static UsbHostController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.UsbHostController) is UsbHostController c ? c : UsbHostController.FromName(NativeApi.GetDefaultName(NativeApiType.UsbHostController));
        /// <summary>Gets the USB host controller with the given name.</summary>
        public static UsbHostController FromName(string name) => UsbHostController.FromProvider(new UsbHostControllerApiWrapper(NativeApi.Find(name, NativeApiType.UsbHostController)));
        /// <summary>Gets a USB host controller backed by the given provider.</summary>
        public static UsbHostController FromProvider(IUsbHostControllerProvider provider) => new UsbHostController(provider);

        /// <summary>Disposes the controller and its provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Enables the controller so devices can be detected.</summary>
        public void Enable() {
            this.Provider.Enable();
            started = true;
        }
        /// <summary>Disables the controller and stops detecting devices.</summary>
        public void Disable() {
            started = false;
            this.Provider.Disable();

        }

        /// <summary>Gets the currently connected devices, or null if the controller is not enabled.</summary>
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


        /// <summary>The event fired when a device's connection status changes.</summary>
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
        /// <summary>Provides the underlying implementation for a USB host controller.</summary>
        public interface IUsbHostControllerProvider : IDisposable {
            /// <summary>Enables the controller.</summary>
            void Enable();
            /// <summary>Disables the controller.</summary>
            void Disable();


            /// <summary>The event fired when a device's connection status changes.</summary>
            event OnConnectionChanged OnConnectionChangedEvent;
        }

        /// <summary>The native API wrapper that implements the USB host controller provider.</summary>
        public sealed class UsbHostControllerApiWrapper : IUsbHostControllerProvider {
            private readonly IntPtr impl;

            private readonly NativeEventDispatcher onConnectDispatcher;

            private OnConnectionChanged onConnectionChangedCallbacks;

            /// <summary>The native API backing this provider.</summary>
            public NativeApi Api { get; }

            /// <summary>Creates a new wrapper around the given native API.</summary>
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

            /// <summary>The event fired when a device's connection status changes.</summary>
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

            /// <summary>Releases the native API and disposes the provider.</summary>
            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            internal static extern void GetDeviceInformation(uint id, out ushort vendor, out ushort product, out byte port);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void OnConnectionChangedEventEnabled(bool enabled);

        }
    }
}
