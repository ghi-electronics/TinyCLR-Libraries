using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Usb;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    /// <summary>Allows a usb device to be used as a joystick.</summary>
	/// <remarks>By default, the reported range for position is between -512 and 512. There is an internal thread that processes the device events.</remarks>
	public class Joystick : BaseDevice {
        private uint currentButtonState;
        private Position currentCursor1Position;
        private Position currentCursor2Position;
        private HatSwitchDirection currentHatSwitchDirection;
        private DeviceCapabilities capabilities;

        private uint oldButtonState;
        private Position oldCursor1Position;
        private Position oldCursor2Position;
        private HatSwitchDirection oldHatSwitchDirection;

        private object syncRoot;

#pragma warning disable 0169
        private uint nativePointer;
#pragma warning restore 0169

        /// <summary>The delegate for when one of the joystick's button is pressed or released.</summary>
        /// <param name="sender">The joystick associated with this event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void ButtonChangedEventHandler(Joystick sender, ButtonChangedEventArgs e);

        /// <summary>The delegate for when one of the joystick's cursors moves.</summary>
        /// <param name="sender">The joystick associated with this event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void CursorMovedEventHandler(Joystick sender, CursorMovedEventArgs e);

        /// <summary>The delegate for when the joystick's hat switch is pressed.</summary>
        /// <param name="sender">The joystick associated with this event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void HatSwitchPressedEventHandler(Joystick sender, HatSwitchPressedEventArgs e);

        /// <summary>The event fired when one of the joystick's buttons is pressed or released.</summary>
        public event ButtonChangedEventHandler ButtonChanged;

        /// <summary>The event fired when the joystick's cursor has moved.</summary>
        public event CursorMovedEventHandler CursorMoved;

        /// <summary>The event fired when the joystick's hat switch has been pressed.</summary>
        public event HatSwitchPressedEventHandler HatSwitchPressed;

        /// <summary>The capabilities of the joystick.</summary>
        public DeviceCapabilities Capabilities => this.capabilities;

        /// <summary>The current position of the first cursor.</summary>
        public Position CursorPosition { get { this.RefreshData(); return this.currentCursor1Position; } }

        /// <summary>The current position of the second cursor.</summary>
        public Position Cursor2Position { get { this.RefreshData(); return this.currentCursor2Position; } }

        /// <summary>The current direction of the hat switch.</summary>
        public HatSwitchDirection CurrentHatSwitchDirection { get { this.RefreshData(); return this.currentHatSwitchDirection; } }

        /// <summary>Joystick capabilities.</summary>
        [Flags]
        public enum DeviceCapabilities : byte {

            /// <summary>Has X on cursor 1.</summary>
            CursorX = 0x01,

            /// <summary>Has Y on cursor 1.</summary>
            CursorY = 0x02,

            /// <summary>Has X on cursor 2.</summary>
            Cursor2X = 0x04,

            /// <summary>Has Y on cursor 2.</summary>
            Cursor2Y = 0x08,

            /// <summary>Has a hat switch.</summary>
            HatSwitch = 0x10,

            /// <summary>Has buttons.</summary>
            Buttons = 0x20,
        }

        /// <summary>Creates a new joystick.</summary>
        /// <param name="id">The device id.</param>
        /// <param name="interfaceIndex">The device interface index.</param>
        public Joystick(uint id, byte interfaceIndex)
            : base(id, interfaceIndex, DeviceType.Joystick) {
            this.capabilities = 0;

            this.NativeConstructor(this.Id, this.InterfaceIndex, out var pollingInterval);

            this.syncRoot = new object();

            this.currentButtonState = 0;
            this.currentCursor1Position = new Position();
            this.currentCursor2Position = new Position();
            this.currentHatSwitchDirection = HatSwitchDirection.None;

            this.oldButtonState = 0;
            this.oldCursor1Position = new Position();
            this.oldCursor2Position = new Position();
            this.oldHatSwitchDirection = HatSwitchDirection.None;

            this.WorkerInterval = pollingInterval;
        }

        /// <summary>The finalizer.</summary>
        ~Joystick() {
            this.Dispose(false);
        }

        /// <summary>Sets the bounds for given cursor.</summary>
        /// <param name="cursor">The cursor for which to set the bounds.</param>
        /// <param name="minX">The minimum X value.</param>
        /// <param name="maxX">The maximum X value.</param>
        /// <param name="minY">The minimum Y value.</param>
        /// <param name="maxY">The maximum Y value.</param>
        public void SetCursorBounds(Cursor cursor, int minX, int maxX, int minY, int maxY) {
            this.CheckObjectState();

            this.NativeSetCursorBounds((int)cursor, minX, maxX, minY, maxY);
        }

        /// <summary>Gets the current button state.</summary>
        /// <param name="buttonNumber">The button number to query.</param>
        /// <returns>The state of the button.</returns>
        public ButtonState GetButtonState(int buttonNumber) {
            this.RefreshData();

            return (ButtonState)(this.currentButtonState & (1 << buttonNumber));
        }

        /// <summary>Disposes the joystick.</summary>
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

            if (this.oldCursor1Position.X != this.currentCursor1Position.X || this.oldCursor1Position.Y != this.currentCursor1Position.Y)
                this.OnCursorMoved(new CursorMovedEventArgs() { NewPosition = this.currentCursor1Position, Delta = this.currentCursor1Position - this.oldCursor1Position, Which = Cursor.Cursor1 });

            if (this.oldCursor2Position.X != this.currentCursor2Position.X || this.oldCursor2Position.Y != this.currentCursor2Position.Y)
                this.OnCursorMoved(new CursorMovedEventArgs() { NewPosition = this.currentCursor2Position, Delta = this.currentCursor2Position - this.oldCursor2Position, Which = Cursor.Cursor2 });

            if (this.oldHatSwitchDirection != this.currentHatSwitchDirection)
                this.OnHatSwitchPressed(new HatSwitchPressedEventArgs() { Direction = this.currentHatSwitchDirection });

            for (var i = 0; i < 32; i++) {
                var oldState = this.oldButtonState & (1 << i);
                var currentState = this.currentButtonState & (1 << i);

                if (oldState != currentState)
                    this.OnButtonChanged(new ButtonChangedEventArgs() { State = currentState != 0 ? ButtonState.Pressed : ButtonState.Released, Which = i });
            }

            this.oldButtonState = this.currentButtonState;
            this.oldCursor1Position = this.currentCursor1Position;
            this.oldCursor2Position = this.currentCursor2Position;
            this.oldHatSwitchDirection = this.currentHatSwitchDirection;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeConstructor(uint id, byte interfaceIndex, out byte pollingInterval);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeFinalize();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private int NativeGetButtonState(int buttonNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void NativeSetCursorBounds(int cursor, int minX, int maxX, int minY, int maxY);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private bool NativeGetPosition(out int x, out int y, out int x2, out int y2, out byte hatswitch, out uint button);

        private void RefreshData() {
            this.CheckObjectState();

            lock (this.syncRoot) {

                if (!this.NativeGetPosition(out var newX, out var newY, out var newX2, out var newY2, out var newHatSwitchDirection, out var newButtonState))
                    return;

                this.currentCursor1Position.X = newX;
                this.currentCursor1Position.Y = newY;

                this.currentCursor2Position.X = newX;
                this.currentCursor2Position.Y = newY;

                this.currentHatSwitchDirection = (HatSwitchDirection)newHatSwitchDirection;

                this.currentButtonState = newButtonState;
            }
        }

        private void OnButtonChanged(ButtonChangedEventArgs e) => this.ButtonChanged?.Invoke(this, e);

        private void OnHatSwitchPressed(HatSwitchPressedEventArgs e) => this.HatSwitchPressed?.Invoke(this, e);

        private void OnCursorMoved(CursorMovedEventArgs e) => this.CursorMoved?.Invoke(this, e);
        /// <summary>The events args for the ButtonPressed and ButtonReleased events.</summary>
        public class ButtonChangedEventArgs : EventArgs {

            /// <summary>The index of the changed button.</summary>
            public int Which { get; internal set; }

            /// <summary>The new state of the button.</summary>
            public ButtonState State { get; internal set; }
        }

        /// <summary>The events args for the CursorMoved event.</summary>
        public class CursorMovedEventArgs : EventArgs {

            /// <summary>The index of the changed button.</summary>
            public Cursor Which { get; internal set; }

            /// <summary>The new state of the button.</summary>
            public Position NewPosition { get; internal set; }

            /// <summary>The change from the last position.</summary>
            public Position Delta { get; internal set; }
        }

        /// <summary>The events args for the HatSwitchPressed event.</summary>
        public class HatSwitchPressedEventArgs : EventArgs {

            /// <summary>The new direction of the hat switch.</summary>
            public HatSwitchDirection Direction { get; internal set; }
        }
    }
}
