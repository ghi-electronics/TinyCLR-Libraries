
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    /// <summary>Allows a usb device to be used a mouse.</summary>
    /// <remarks>
    /// By default, the reported range for position is between 0 and 512 and between -512 and 512 for delta values. There is an internal thread that
    /// processes the device events.
    /// </remarks>
    public class Mouse : BaseDevice {
        private ButtonState[] currentButtonState;
        private int currentWheelPosition;
        private Position currentCursorPosition;

        private ButtonState[] oldButtonState;
        private int oldWheelPosition;
        private Position oldCursorPosition;

        private object syncRoot;

#pragma warning disable 0169
        private uint nativePointer;
#pragma warning restore 0169

        /// <summary>The delegate for when one of the mouse's button is pressed or released.</summary>
        /// <param name="sender">The mouse associated with this event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ButtonChangedEventHandler(Mouse sender, ButtonChangedEventArgs e);

        /// <summary>The delegate for when the mouse's wheel moves.</summary>
        /// <param name="sender">The mouse associated with this event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void WheelMovedEventHandler(Mouse sender, WheelMovedEventArgs e);

        /// <summary>The delegate for when the mouse's cursor moves.</summary>
        /// <param name="sender">The mouse associated with this event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void CursorMovedEventHandler(Mouse sender, CursorMovedEventArgs e);

        /// <summary>The event fired when one of the mouse's buttons is pressed or released.</summary>
        public event ButtonChangedEventHandler ButtonChanged;

        /// <summary>The event fired when the mouse's wheel has moved.</summary>
        public event WheelMovedEventHandler WheelMoved;

        /// <summary>The event fired when the mouse's cursor has moved.</summary>
        public event CursorMovedEventHandler CursorMoved;

        /// <summary>The current state of the mouse's left button.</summary>
        public ButtonState LeftButtonState { get { this.RefreshData(); return this.currentButtonState[0]; } }

        /// <summary>The current state of the mouse's right button.</summary>
        public ButtonState RightButtonState { get { this.RefreshData(); return this.currentButtonState[1]; } }

        /// <summary>The current state of the mouse's middle button.</summary>
        public ButtonState MiddleButtonState { get { this.RefreshData(); return this.currentButtonState[2]; } }

        /// <summary>The current state of the mouse's first extended button.</summary>
        public ButtonState Extended1ButtonState { get { this.RefreshData(); return this.currentButtonState[3]; } }

        /// <summary>The current state of the mouse's second extended button.</summary>
        public ButtonState Extended2ButtonState { get { this.RefreshData(); return this.currentButtonState[4]; } }

        /// <summary>The current position of the cursor.</summary>
        public Position CursorPosition { get { this.RefreshData(); return this.currentCursorPosition; } }

        /// <summary>The current position of the mouse wheel.</summary>
        public int WheelPosition { get { this.RefreshData(); return this.currentWheelPosition; } }

        /// <summary>Creates a new mouse.</summary>
        /// <param name="id">The device id.</param>
        /// <param name="interfaceIndex">The device interface index.</param>
        /// <param name="vendorId">The device vendor id.</param>
        /// <param name="productId">The device product id.</param>
        /// <param name="portNumber">The device port number.</param>
        public Mouse(uint id, byte interfaceIndex)
            : base(id, interfaceIndex, DeviceType.Mouse) {
            this.NativeConstructor(this.Id, this.InterfaceIndex);

            this.syncRoot = new object();

            this.currentWheelPosition = 0;
            this.currentCursorPosition = new Position();
            this.currentButtonState = new ButtonState[] { ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released };

            this.oldWheelPosition = 0;
            this.oldCursorPosition = new Position();
            this.oldButtonState = new ButtonState[] { ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released };

            this.WorkerInterval = 50;
        }

        /// <summary>The finalizer.</summary>
        ~Mouse() {
            this.Dispose(false);
        }

        /// <summary>Scales the x, y, and wheel delta values.</summary>
        /// <param name="scale">The value by which to scale the data.</param>
        public void SetScale(float scale) {
            this.CheckObjectState();

            this.NativeSetScale(scale);
        }

        /// <summary>Sets the cursor bounds.</summary>
        /// <param name="minimum">The minimum bound.</param>
        /// <param name="maximum">The maximum bound.</param>
        public void SetCursorBounds(Position minimum, Position maximum) {
            this.CheckObjectState();

            this.NativeSetCursorBounds(minimum.X, maximum.X, minimum.Y, maximum.Y);
        }

        /// <summary>Sets the cursor position.</summary>
        /// <param name="newPosition">The new position of the cursor.</param>
        public void SetCursorPosition(Position newPosition) {
            this.CheckObjectState();

            this.NativeSetCursorPosition(newPosition.X, newPosition.Y);
        }

        /// <summary>Disposes the mouse.</summary>
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

            try {
                this.RefreshData();
            }
            catch {
            }

            if (this.oldWheelPosition != this.currentWheelPosition)
                this.OnWheelMoved(new WheelMovedEventArgs() { Delta = this.currentWheelPosition - this.oldWheelPosition, NewPosition = this.currentWheelPosition });

            if (this.oldCursorPosition.X != this.currentCursorPosition.X || this.oldCursorPosition.Y != this.currentCursorPosition.Y)
                this.OnCursorMoved(new CursorMovedEventArgs() { Delta = this.currentCursorPosition - this.oldCursorPosition, NewPosition = this.currentCursorPosition });

            for (var i = 0; i < 5; i++)
                if (this.oldButtonState[i] != this.currentButtonState[i])
                    this.OnButtonChanged(new ButtonChangedEventArgs() { Which = (Buttons)(1 << i), State = this.currentButtonState[i] });

            this.currentButtonState.CopyTo(this.oldButtonState, 0);

            this.oldCursorPosition = this.currentCursorPosition;
            this.oldWheelPosition = this.currentWheelPosition;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeConstructor(uint id, byte interfaceIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeFinalize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeSetScale(float value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeSetCursorBounds(int minimumX, int maximumX, int minimumY, int maximumY);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeSetCursorPosition(int x, int y);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private bool NativeGetPosition(out int deltaX, out int deltaY, out int deltaWheel, out int newX, out int newY, out uint button);

        private void RefreshData() {
            this.CheckObjectState();

            lock (this.syncRoot) {

                if (!this.NativeGetPosition(out var newDeltaX, out var newDeltaY, out var newDeltaWheel, out var newX, out var newY, out var newButtonState))
                    return;

                this.currentCursorPosition.X = newX;
                this.currentCursorPosition.Y = newY;

                this.currentWheelPosition += newDeltaWheel;

                for (var i = 0; i < 5; i++)
                    this.currentButtonState[i] = (newButtonState & (1 << i)) != 0 ? ButtonState.Pressed : ButtonState.Released;
            }
        }

        private void OnButtonChanged(ButtonChangedEventArgs e) => this.ButtonChanged?.Invoke(this, e);

        private void OnWheelMoved(WheelMovedEventArgs e) => this.WheelMoved?.Invoke(this, e);

        private void OnCursorMoved(CursorMovedEventArgs e) => this.CursorMoved?.Invoke(this, e);
        /// <summary>The events args for the ButtonPressed and ButtonReleased events.</summary>
        public class ButtonChangedEventArgs : EventArgs {
            private Buttons which;
            private ButtonState state;

            /// <summary>Which button changed its state.</summary>
            public Buttons Which { get => this.which; set => this.which = value; }

            /// <summary>The new state of the button.</summary>
            public ButtonState State { get => this.state; set => this.state = value; }
        }

        /// <summary>The events args for the WheelMoved event.</summary>
        public class WheelMovedEventArgs : EventArgs {
            private int newPosition;
            private int delta;

            /// <summary>The new position of the wheel.</summary>
            public int NewPosition { get => this.newPosition; set => this.newPosition = value; }

            /// <summary>The change from the last position.</summary>
            public int Delta { get => this.delta; set => this.delta = value; }
        }

        /// <summary>The events args for the CursorMoved event.</summary>
        public class CursorMovedEventArgs : EventArgs {
            private Position newPosition;
            private Position delta;

            /// <summary>The new state of the button.</summary>
            public Position NewPosition { get => this.newPosition; set => this.newPosition = value; }

            /// <summary>The change from the last position.</summary>
            public Position Delta { get => this.delta; set => this.delta = value; }
        }
    }
}
