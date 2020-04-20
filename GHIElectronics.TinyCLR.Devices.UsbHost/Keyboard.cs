using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    /// <summary>Allows a usb device to be used as a keyboard.</summary>
    /// <remarks>
    /// The CharUp and CharDown events are only fired when a given key press has a corresponding ASCII key code. There is an internal thread that
    /// processes the device events.
    /// </remarks>
    public class Keyboard : BaseDevice {
#pragma warning disable 0169
        private uint nativePointer;
#pragma warning restore 0169

        /// <summary>The handler for used for keyboard callbacks.</summary>
        /// <param name="sender">The object associated with this event.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void KeyboardEventHandler(Keyboard sender, KeyboardEventArgs args);

        /// <summary>Fired when a key is released.</summary>
        public event KeyboardEventHandler KeyUp;

        /// <summary>Fired when a key is pressed.</summary>
        public event KeyboardEventHandler KeyDown;

        /// <summary>Fired when a key is released and it can be converted to ASCII.</summary>
        public event KeyboardEventHandler CharUp;

        /// <summary>Fired when a key is pressed and it can be converted to ASCII.</summary>
        public event KeyboardEventHandler CharDown;

        /// <summary>Creates a new keyboard.</summary>
        /// <param name="id">The device id.</param>
        /// <param name="interfaceIndex">The device interface index.</param>
        /// <param name="vendorId">The device vendor id.</param>
        /// <param name="productId">The device product id.</param>
        /// <param name="portNumber">The device port number.</param>
        public Keyboard(uint id, byte interfaceIndex)
            : base(id, interfaceIndex, DeviceType.Keyboard) {
            this.NativeConstructor(this.Id, this.InterfaceIndex);

            this.WorkerInterval = 10;
        }

        /// <summary>The finalizer.</summary>
        ~Keyboard() {
            this.Dispose(false);
        }

        /// <summary>Gets the current state of a key.</summary>
        /// <param name="key">The key to query.</param>
        /// <returns>The state of the key.</returns>
        public KeyState IsKeyPressed(Key key) {
            this.CheckObjectState();

            return (KeyState)this.NativeGetKeyState((byte)key);
        }

        /// <summary>Disposes the keyboard.</summary>
        /// <param name="disposing">Whether or not this is called from Dispose.</param>
        protected override void Dispose(bool disposing) {
            if (this.disposed)
                return;

            this.NativeFinalize();

            base.Dispose(disposing);
        }

        /// <summary>Repeatedly called with a period defined by WorkerInterval. Used to poll the device for data and raise any desired events.</summary>
        /// <param name="sender">Always null.</param>
        protected override void CheckEvents(object sender) {
            if (!this.CheckObjectState(false)) return;

            byte key = 0, keyState = 0;
            var ascii = '\0';

            try {
                if (!this.NativeGetState(out key, out ascii, out keyState)) {
                    return;
                }
            }
            catch {
            }

            var args = new KeyboardEventArgs() { ASCII = ascii, Which = (Key)key };

            if ((KeyState)keyState == KeyState.Pressed) {
                this.OnKeyDown(args);

                if (args.ASCII != 0)
                    this.OnCharDown(args);
            }
            else if ((KeyState)keyState == KeyState.Released) {
                this.OnKeyUp(args);

                if (args.ASCII != 0)
                    this.OnCharUp(args);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private int NativeGetKeyState(byte key);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeConstructor(uint id, byte interfaceIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeFinalize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private bool NativeGetState(out byte key, out char ascii, out byte state);

        private void OnKeyUp(KeyboardEventArgs e) => this.KeyUp?.Invoke(this, e);

        private void OnKeyDown(KeyboardEventArgs e) => this.KeyDown?.Invoke(this, e);

        private void OnCharUp(KeyboardEventArgs e) => this.CharUp?.Invoke(this, e);

        private void OnCharDown(KeyboardEventArgs e) => this.CharDown?.Invoke(this, e);
        /// <summary>Event arguments for the keyboard events.</summary>
        public class KeyboardEventArgs : EventArgs {
            private Key which;
            private char ascii;

            /// <summary>The Key associated with the event.</summary>
            public Key Which { get => this.which; set => this.which = value; }

            /// <summary>The ASCII representation of the key, if available. Otherwise, this value is <b>0</b>.</summary>
            public char ASCII { get => this.ascii; set => this.ascii = value; }
        }
    }
}
