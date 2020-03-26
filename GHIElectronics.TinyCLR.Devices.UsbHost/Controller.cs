

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.UsbHost;
/// <summary>
/// This class provides the ability to access the devices connected to the USB Host. You can connect multiple devices and the connect and
/// disconnect events are sent for each device with a unique Id. Some devices might have multiple functions represented as interfaces. These are
/// reported with the same Id but will differ in InterfaceIndex. Make sure to process the events from each device as fast as possible. These are
/// handled in a special thread that may suspend other threads. There is built in support for several USB devices: mass storage, mice, printers,
/// keyboards, joysticks, and serial converters. If a driver is not available, you can use RawDevice to access it but that requires knowledge of
/// USB specifications. See https://www.ghielectronics.com/docs/36/ for more information.
/// </summary>

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    public static class Controller {
        private static bool started;
        private static ArrayList devices;
        private static object listLock;

        /// <summary>The delegate used for the DeviceConnectFailed event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void DeviceConnectFailedEventHandler(object sender, EventArgs e);

        /// <summary>The delegate used for the JoystickConnected event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        //public delegate void JoystickConnectedEventHandler(object sender, Joystick e);

        /// <summary>The delegate used for the KeyboardConnected event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        //public delegate void KeyboardConnectedEventHandler(object sender, Keyboard e);

        /// <summary>The delegate used for the MassStorageConnected event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        //public delegate void MassStorageConnectedEventHandler(object sender, MassStorage e);

        /// <summary>The delegate used for the MouseConnected event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        //public delegate void MouseConnectedEventHandler(object sender, Mouse e);

        /// <summary>The delegate used for the RawDeviceConnected event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void UnknownDeviceConnectedEventHandler(object sender, UnknownDeviceConnectedEventArgs e);

        /// <summary>The delegate used for the UsbSerialConnected event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        //public delegate void UsbSerialConnectedEventHandler(object sender, UsbSerial e);

        /// <summary>The delegate used for the WebcamConnected event.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        //public delegate void WebcamConnectedEventHandler(object sender, Webcam e);

        /// <summary>Raised when a device fails to connect properly.</summary>
        public static event DeviceConnectFailedEventHandler DeviceConnectFailed;

        /// <summary>Raised when a joystick connects.</summary>
        //public static event JoystickConnectedEventHandler JoystickConnected;

        /// <summary>Raised when a keyboard connects.</summary>
        //public static event KeyboardConnectedEventHandler KeyboardConnected;

        /// <summary>Raised when a mass storage device connects.</summary>
        //public static event MassStorageConnectedEventHandler MassStorageConnected;

        /// <summary>Raised when a mouse connects.</summary>
        //public static event MouseConnectedEventHandler MouseConnected;

        /// <summary>Raised when a raw device connects.</summary>
        public static event UnknownDeviceConnectedEventHandler UnknownDeviceConnected;

        /// <summary>Raised when a usb serial converter connects.</summary>
        //public static event UsbSerialConnectedEventHandler UsbSerialConnected;

        /// <summary>Raised when a webcam connects.</summary>
        //public static event WebcamConnectedEventHandler WebcamConnected;

        /// <summary>USB host errors.</summary>
        public enum Error : uint {

            /// <summary>No error.</summary>
            NoError,

            /// <summary>Device is busy. Try communicating with the device at a later time.</summary>
            DeviceBusy,

            /// <summary>Transfer Error. Try Transferring again.</summary>
            TransferError,

            /// <summary>Maximum available handles reached.</summary>
            MaxDeviceUsage,

            /// <summary>Device is not connected.</summary>
            DeviceNotOnline,

            /// <summary>Out of memory.</summary>
            OutOfMemory,

            /// <summary>Maximum USB devices connected (127).</summary>
            MaxUsbDevicesReached,

            /// <summary>HID parse error.</summary>
            HIDParserError,

            /// <summary>HID item not found.</summary>
            HIDParserItemNotFound,

            /// <summary>Transfer completed successfully.</summary>
            CompletionCodeNoError = 0x10000000,

            /// <summary>Transfer error. Make sure you have enough power for the USB device and connections are stable.</summary>
            CompletionCodeCRC,

            /// <summary>Transfer error. Make sure you have enough power for the USB device and connections are stable.</summary>
            CompletionCodeBitStuffing,

            /// <summary>
            /// Transfer error. Make sure you have enough power for the USB device and connections are stable. This error means there might be some
            /// missing USB packets during communications. In many cases you can ignore this error if missing some packets is not significant. Several
            /// USB devices might drop some packets or incorrectly produce this error.
            /// </summary>
            CompletionCodeDataToggle,

            /// <summary>Transfer error. USB device refused the transfer. Check sent USB packet.</summary>
            CompletionCodeStall,

            /// <summary>Transfer error. Make sure you have enough power for the USB device and connections are stable.</summary>
            CompletionCodeNoResponse,

            /// <summary>Transfer error. Make sure you have enough power for the USB device and connections are stable.</summary>
            CompletionCodePIDCheck,

            /// <summary>Transfer error. Make sure you have enough power for the USB device and connections are stable.</summary>
            CompletionCodePIDUnExpected,

            /// <summary>Transfer error. Endpoint returned more data than expected.</summary>
            CompletionCodeDataOverRun,

            /// <summary>Transfer error. Endpoint returned less data than expected.</summary>
            CompletionCodeDataUnderRun,

            /// <summary>Transfer error. HC received data from endpoint faster than it could be written to system memory.</summary>
            CompletionCodeBufferOverRun,

            /// <summary>Transfer error. HC could not retrieve data from system memory fast enough to keep up with data USB data rate.</summary>
            CompletionCodeBufferUnderRun,

            /// <summary>Software use.</summary>
            CompletionCodeNotAccessed,

            /// <summary>Software use.</summary>
            CompletionCodeNotAccessedF,

            /// <summary>Mass Storage error.</summary>
            MSError = 0x20000000,

            /// <summary>Mass Storage error.</summary>
            MSCSWCommandFailed,

            /// <summary>Mass Storage error.</summary>
            MSCSWStatusPhaseError,

            /// <summary>Mass Storage error.</summary>
            MSCSW,

            /// <summary>Mass Storage error.</summary>
            MSWrongLunNumber,

            /// <summary>Mass Storage error.</summary>
            MSWrongSignature,

            /// <summary>Mass Storage error.</summary>
            MSTagMissmatched,

            /// <summary>Mass Storage error.</summary>
            MSNotReady,
        }

        static Controller() {
            Controller.devices = new ArrayList();
            Controller.started = false;
            Controller.listLock = new object();

            //InternalEvent.UsbDeviceConnected += Controller.OnConnect;
            //InternalEvent.UsbDeviceConnectionFailed += Controller.OnConnectFailed;
            //InternalEvent.UsbDeviceDisconnected += Controller.OnDisconnect;
        }

        /// <summary>Starts the USB Host controller.</summary>
        public static void Start() {
            if (Controller.started)
                return;

            Controller.NativeStart();

            Controller.started = true;
        }

        /// <summary>Gets a list of the currently connected devices.</summary>
        /// <returns>The currently connected devices.</returns>
        public static BaseDevice[] GetConnectedDevices() {
            Controller.Start();

            lock (Controller.listLock)
                return (BaseDevice[])Controller.devices.ToArray(typeof(BaseDevice));
        }

        /// <summary>Resets the USB host controller.</summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static void Reset();

        /// <summary>Gets the last USB error that occured.</summary>
        /// <returns>The error that occured.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static Error GetLastError();

        internal static void RegisterDevice(BaseDevice device) {
            lock (Controller.listLock)
                Controller.devices.Add(device);
        }

        private static void OnConnect(object sender, InternalEvent.InternalEventEventArgs e) {

            Controller.NativeGetDeviceParameters(e.EventId, out var vendorId, out var productId, out var portNumber);

            var type = (BaseDevice.DeviceType)(byte)(e.Data >> 8);

            switch (type) {
                case BaseDevice.DeviceType.Joystick:
                    //var joystick = new Joystick(e.EventId, (byte)e.Data, vendorId, productId, portNumber);

                    //Controller.OnJoystickConnected(joystick);

                    break;

                case BaseDevice.DeviceType.Keyboard:
                    //var keyboard = new Keyboard(e.EventId, (byte)e.Data, vendorId, productId, portNumber);

                    //Controller.OnKeyboardConnected(keyboard);

                    break;

                case BaseDevice.DeviceType.MassStorage:
                    //var massStorage = new MassStorage(e.EventId, (byte)e.Data, vendorId, productId, portNumber);

                    //Controller.OnMassStorageConnected(massStorage);

                    break;

                case BaseDevice.DeviceType.Mouse:
                    //var mouse = new Mouse(e.EventId, (byte)e.Data, vendorId, productId, portNumber);

                    //Controller.OnMouseConnected(mouse);

                    break;

                case BaseDevice.DeviceType.SerialFTDI:
#pragma warning disable 0618, 0612
                case BaseDevice.DeviceType.SerialCDC:
                case BaseDevice.DeviceType.SerialProlific:
                case BaseDevice.DeviceType.SerialProlific2:
                case BaseDevice.DeviceType.SerialSierraC885:
                case BaseDevice.DeviceType.SerialSiLabs:
#pragma warning restore 0618, 0612
                    //var usbSerial = new UsbSerial(e.EventId, (byte)e.Data, vendorId, productId, portNumber, type);

                    //Controller.OnUsbSerialConnected(usbSerial);

                    break;

                case BaseDevice.DeviceType.Webcam:
                    //var webcam = new Webcam(e.EventId, (byte)e.Data, vendorId, productId, portNumber);

                    //Controller.OnWebcamConnected(webcam);

                    break;

                default:
                    var baseDevice = new UnknownDeviceConnectedEventArgs(e.EventId, (byte)e.Data, type, vendorId, productId, portNumber);

                    //Controller.OnUnknownDeviceConnected(baseDevice);

                    break;
            }
        }

        private static void OnConnectFailed(object sender, InternalEvent.InternalEventEventArgs e) => Controller.DeviceConnectFailed?.Invoke(null, null);

        private static void OnDisconnect(object sender, InternalEvent.InternalEventEventArgs e) {
            lock (Controller.listLock) {
                var newList = new ArrayList();

                foreach (BaseDevice d in Controller.devices) {
                    if (d.Id == e.Data) {
                        d.OnDisconnected();
                        d.Dispose();
                    }
                    else {
                        newList.Add(d);
                    }
                }

                Controller.devices = newList;
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeGetDeviceParameters(uint id, out ushort vendorId, out ushort productId, out byte port);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeStart();

        //private static void OnJoystickConnected(Joystick device) {
        //	var e = Controller.JoystickConnected;
        //	if (e != null)
        //		e(null, device);
        //}

        //private static void OnKeyboardConnected(Keyboard device) {
        //	var e = Controller.KeyboardConnected;
        //	if (e != null)
        //		e(null, device);
        //}

        //private static void OnMassStorageConnected(MassStorage device) {
        //	var e = Controller.MassStorageConnected;
        //	if (e != null)
        //		e(null, device);
        //}

        //private static void OnMouseConnected(Mouse device) {
        //	var e = Controller.MouseConnected;
        //	if (e != null)
        //		e(null, device);
        //}

        //private static void OnUnknownDeviceConnected(UnknownDeviceConnectedEventArgs args) {
        //	var e = Controller.UnknownDeviceConnected;
        //	if (e != null)
        //		e(null, args);
        //}

        //private static void OnUsbSerialConnected(UsbSerial device) {
        //	var e = Controller.UsbSerialConnected;
        //	if (e != null)
        //		e(null, device);
        //}

        //private static void OnWebcamConnected(Webcam device) {
        //	var e = Controller.WebcamConnected;
        //	if (e != null)
        //		e(null, device);
        //}
        /// <summary>Events args for the UnknownDeviceConnected event.</summary>
        public class UnknownDeviceConnectedEventArgs : EventArgs {
            private uint id;
            private byte interfaceIndex;
            private BaseDevice.DeviceType type;
            private ushort vendorId;
            private ushort productId;
            private byte portNumber;

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

            internal UnknownDeviceConnectedEventArgs(uint id, byte interfaceIndex, BaseDevice.DeviceType type, ushort vendorId, ushort productId, byte portNumber) {
                this.id = id;
                this.interfaceIndex = interfaceIndex;
                this.type = type;
                this.vendorId = vendorId;
                this.productId = productId;
                this.portNumber = portNumber;
            }
        }
    }
}
