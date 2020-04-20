using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.UsbHost.Provider;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    /// <summary>Represents a USB device connected.</summary>
    public abstract class BaseDevice : IDisposable {

        /// <summary>Whether or not the object has been disposed.</summary>
        protected bool disposed;

        private Timer worker;
        private int interval;
        private uint id;
        private byte interfaceIndex;
        private DeviceType type;
        private ushort vendorId;
        private ushort productId;
        private byte portNumber;
        private bool connected;

        /// <summary>The event handler type for when the device disconnects.</summary>
        public delegate void DisconnectedEventHandler(BaseDevice sender, EventArgs e);

        /// <summary>The event is fired when the device disconnects.</summary>
        public event DisconnectedEventHandler Disconnected;

        /// <summary>Whether or not the device is connected.</summary>
        public bool Connected => this.connected;

        /// <summary>The device id.</summary>
        /// <remarks>This is unique among the currently connected USB devices. It will be the same for a device with multiple interfaces.</remarks>
        public uint Id => this.id;

        /// <summary>The logical device interface index.</summary>
        /// <remarks>
        /// Some USB devices might have multiple functions represented as multiple interfaces. If a device functions as a whole, this will be equal to NO_INTERFACE_ASSOCIATED.
        /// </remarks>
        public byte InterfaceIndex => this.interfaceIndex;

        /// <summary>The device's type.</summary>
        public DeviceType Type => this.type;

        /// <summary>The devic's vendor id.</summary>
        /// <remarks>This is unique per company.</remarks>
        public ushort VendorId => this.vendorId;

        /// <summary>The device's product id.</summary>
        /// <remarks>This is unique per product for a certain company.</remarks>
        public ushort ProductId => this.productId;

        /// <summary>The device's USB port number.</summary>
        public byte PortNumber => this.portNumber;

        /// <summary>How often the internal worker callback is called.</summary>
        /// <remarks>Timeout.Infinite or 0 disable the internal worker.</remarks>
        public virtual int WorkerInterval {
            get => this.interval;

            set {
                if (this.disposed) throw new ObjectDisposedException();

                if (value <= 0)
                    value = Timeout.Infinite;

                this.interval = value;

                this.worker.Change(this.interval, this.interval);
            }
        }

        /// <summary>Possible device types.</summary>
        public enum DeviceType : byte {

            /// <summary>The device is not recognized.</summary>
            Unknown,

            /// <summary>USB Hub.</summary>
            Hub,

            /// <summary>Human Interface Device.</summary>
            HID,

            /// <summary>Mouse.</summary>
            Mouse,

            /// <summary>Keyboard.</summary>
            Keyboard,

            /// <summary>Joystick.</summary>
            Joystick,

            /// <summary>Mass Storage. This includes USB storage devices such as USB Thumbs drives and USB hard disks.</summary>
            MassStorage,

            /// <summary>Printer.</summary>
            [Obsolete()]
            Printer,

            /// <summary>USB to Serial device.</summary>
            SerialFTDI,

            /// <summary>USB to Serial device.</summary>
            [Obsolete()]
            SerialProlific,

            /// <summary>USB to Serial device.</summary>
            [Obsolete()]
            SerialProlific2,

            /// <summary>USB to Serial device.</summary>
            [Obsolete()]
            SerialSiLabs,

            /// <summary>USB to Serial device.</summary>
            SerialCDC,

            /// <summary>USB to Serial device.</summary>
            [Obsolete()]
            SerialSierraC885,

            /// <summary>Sierra Installer.</summary>
            [Obsolete()]
            SierraInstaller,

            /// <summary>Video device.</summary>
            //Video,

            /// <summary>Webcamera.</summary>
            //Webcam,
        }

        internal BaseDevice(uint id, byte interfaceIndex, DeviceType type) {
            UsbHostController.RegisterDevice(this);
            UsbHostControllerApiWrapper.GetDeviceInformation(id, out var vendor, out var product, out var port);

            this.id = id;
            this.interfaceIndex = interfaceIndex;
            this.type = type;
            this.vendorId = vendor;
            this.productId = product;
            this.portNumber = port;

            this.disposed = false;
            this.connected = true;

            this.worker = new Timer(this.CheckEvents, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>Disconnects and disposes the device.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void OnDisconnected() {
            this.Disconnected?.Invoke(this, null);

            this.WorkerInterval = Timeout.Infinite;

            this.connected = false;
        }

        /// <summary>Repeatedly called with a period defined by WorkerInterval. Used to poll the device for data and raise any desired events.</summary>
        /// <param name="sender">Always null.</param>
        protected virtual void CheckEvents(object sender) {
        }

        /// <summary>Disconnects and disposes the device.</summary>
        /// <param name="disposing">Whether or not this is called from Dispose.</param>
        protected virtual void Dispose(bool disposing) {
            if (this.disposed)
                return;

            if (disposing) {
                if (this.worker != null) {
                    this.worker.Dispose();
                    this.worker = null;
                }
            }

            this.disposed = true;
        }

        /// <summary>Verifies that the object is connected and not disposed.</summary>
        /// <param name="throwOnInvalid">If the object is connected or disposed, whether or not to throw an exception.</param>
        /// <returns>True if the object is connected and not disposed, false otherwise.</returns>
        protected bool CheckObjectState(bool throwOnInvalid = true) {
            if (throwOnInvalid) {
                if (!this.connected) throw new InvalidOperationException("The device has been disconnected.");
                if (this.disposed) throw new ObjectDisposedException();

                return true;
            }
            else {
                return this.connected && !this.disposed;
            }
        }
    }
}
