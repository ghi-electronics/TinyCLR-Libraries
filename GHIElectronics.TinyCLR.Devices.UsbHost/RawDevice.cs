using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    /// <summary>
    /// Provides low level access functionalities for the connected USB device. This is useful if there is not already a predefined driver for that device.
    /// </summary>
    public class RawDevice : BaseDevice {
        private byte configIndex;

#pragma warning disable 0169
        private uint nativePointer;
#pragma warning restore 0169

        /// <summary>Creates a new raw device.</summary>
        /// <param name="id">The device id.</param>
        /// <param name="interfaceIndex">The device interface index.</param>
        /// <param name="vendorId">The device vendor id.</param>
        /// <param name="productId">The device product id.</param>
        /// <param name="portNumber">The device port number.</param>
        /// <param name="deviceType">The device type.</param>
        public RawDevice(uint id, byte interfaceIndex, DeviceType type)
            : base(id, interfaceIndex, type) {
            this.NativeConstructor(this.Id);

            this.configIndex = 0;
        }

        /// <summary>The finalizer.</summary>
        ~RawDevice() {
            this.Dispose(false);
        }

        /// <summary>Opens a new communication pipe.</summary>
        /// <param name="endpoint">The descriptor for the communication endpoint.</param>
        /// <returns>The new pipe.</returns>
        public RawDevice.Pipe OpenPipe(Descriptors.Endpoint endpoint) {
            this.CheckObjectState();

            return new Pipe(this.configIndex, this, endpoint);
        }

        /// <summary>Sends a USB setup packet.</summary>
        /// <param name="requestType">The request type, receipient, and direction.</param>
        /// <param name="request">The request to make.</param>
        /// <param name="value">The value of the request.</param>
        /// <param name="index">The index of the request.</param>
        public void SendSetupPacket(byte requestType, byte request, ushort value, ushort index) {
            this.CheckObjectState();

            if (requestType == 0x00 && request == 0x09)
                this.configIndex = (byte)(value - 1);

            this.NativeSendSetupPacket(requestType, request, value, index);
        }

        /// <summary>Sends a USB setup packet.</summary>
        /// <param name="requestType">The request type, receipient, and direction.</param>
        /// <param name="request">The request to make.</param>
        /// <param name="value">The value of the request.</param>
        /// <param name="index">The index of the request.</param>
        /// <param name="data">The data to receive.</param>
        /// <param name="dataOffset">The offset into data at which to receive the data.</param>
        /// <param name="dataCount">The number of bytes to receive into data starting at offset.</param>
        public void SendSetupPacket(byte requestType, byte request, ushort value, ushort index, byte[] data, int dataOffset, int dataCount) {
            this.CheckObjectState();

            this.NativeSendSetupPacket(requestType, request, value, index, data, dataOffset, dataCount);
        }

        /// <summary>Gets the device descriptor.</summary>
        /// <returns>The device descriptor.</returns>
        public Descriptors.Device GetDeviceDescriptor() {
            this.CheckObjectState();

            var bytes = new byte[Descriptors.Device.LENGTH];

            this.SendSetupPacket(0x80, 0x06, 0x0100, 0, bytes, 0, bytes.Length);

            return new Descriptors.Device(bytes);
        }

        /// <summary>Gets the configuration descriptor.</summary>
        /// <param name="configurationIndex">The configuration index.</param>
        /// <returns>The configuration descriptor.</returns>
        public Descriptors.Configuration GetConfigurationDescriptor(byte configurationIndex) {
            this.CheckObjectState();

            var bytes = new byte[Descriptors.Configuration.LENGTH];
            this.SendSetupPacket(0x80, 0x06, (ushort)(0x0200 | configurationIndex), 0, bytes, 0, bytes.Length);

            var descriptor = new Descriptors.Configuration(bytes);

            bytes = new byte[descriptor.TotalLength];
            this.SendSetupPacket(0x80, 0x06, (ushort)(0x0200 | configurationIndex), 0, bytes, 0, bytes.Length);

            descriptor.FillChildren(bytes, Descriptors.Configuration.LENGTH);

            return descriptor;
        }

        /// <summary>Disconnects and disposes the device.</summary>
        /// <param name="disposing">Whether or not this is called from Dispose.</param>
        protected override void Dispose(bool disposing) {
            if (this.disposed)
                return;

            this.NativeDispose();

            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeConstructor(uint id);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeDispose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeSendSetupPacket(byte requestType, byte request, ushort value, ushort index);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeSendSetupPacket(byte requestType, byte request, ushort value, ushort index, byte[] data, int dataOffset, int dataCount);
        /// <summary>A USB communication pipe.</summary>
        public class Pipe : IDisposable {
            private bool disposed;
            private RawDevice device;
            private Descriptors.Endpoint endpoint;
            private int transferTimeout = 500;
#pragma warning disable 0169
            private uint pipeId;
#pragma warning restore 0169

            /// <summary>The timeout for transfer operations. Defaults to 500ms.</summary>
            public int TransferTimeout {
                get => this.transferTimeout;

                set {
                    if (value < 0) throw new ArgumentOutOfRangeException("value", "value must be non-negative.");

                    this.transferTimeout = value;
                }
            }

            /// <summary>Endpoint associated with this pipe.</summary>
            public Descriptors.Endpoint Endpoint { get => this.endpoint; private set => this.endpoint = value; }

            internal Pipe(byte configIndex, RawDevice device, Descriptors.Endpoint endpoint) {
                this.disposed = false;
                this.device = device;
                this.Endpoint = endpoint;

                this.NativeConstructor(configIndex, device.Id, endpoint.Address);

                this.device.Disconnected += (a, b) => this.Dispose();
            }

            /// <summary>The finalizer.</summary>
            ~Pipe() {
                this.Dispose(false);
            }

            /// <summary>Transfers data to/from the endpoint.</summary>
            /// <param name="buffer">The transfer buffer.</param>
            /// <returns>The number of bytes successfully transferred.</returns>
            public int Transfer(byte[] buffer) {
                if (buffer == null) throw new ArgumentNullException("buffer");

                return this.Transfer(buffer, 0, buffer.Length);
            }

            /// <summary>Transfers data to/from the endpoint.</summary>
            /// <param name="buffer">The transfer buffer.</param>
            /// <param name="offset">The offset into the buffer.</param>
            /// <param name="count">The amount to transfer starting at offset from the buffer.</param>
            /// <returns>The number of bytes successfully transferred.</returns>
            public int Transfer(byte[] buffer, int offset, int count) {
                if (buffer == null) throw new ArgumentNullException("buffer");
                if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must be non-negative.");
                if (count < 0) throw new ArgumentOutOfRangeException("count", "count must be non-negative.");
                if (buffer.Length < offset + count) throw new ArgumentOutOfRangeException("buffer", "buffer.Length must be at least offset + count.");

                this.device.CheckObjectState();

                return this.NativeTransfer(buffer, offset, count, this.TransferTimeout);

            }

            /// <summary>Disconnects and disposes the device.</summary>
            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>Disconnects and disposes the device.</summary>
            /// <param name="disposing">Whether or not this is called from Dispose.</param>
            protected virtual void Dispose(bool disposing) {
                if (this.disposed)
                    return;

                this.NativeFinalize();

                this.disposed = true;
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            extern private int NativeTransfer(byte[] buffer, int offset, int count, int transferTimeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            extern private void NativeConstructor(byte configIndex, uint deviceId, byte ep);

            [MethodImpl(MethodImplOptions.InternalCall)]
            extern private void NativeFinalize();
        }
    }
}
