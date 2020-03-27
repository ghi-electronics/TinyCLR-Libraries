using System;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {

	/// <summary>Lists available mouse buttons.</summary>
	[Flags]
	public enum Buttons : byte {

		/// <summary>No button.</summary>
		None = 0x00,

		/// <summary>The left button.</summary>
		Left = 0x01,

		/// <summary>The right button.</summary>
		Right = 0x02,

		/// <summary>The middle button.</summary>
		Middle = 0x04,

		/// <summary>The first extended button.</summary>
		Extended1 = 0x08,

		/// <summary>The second extended button.</summary>
		Extended2 = 0x10,
	}

	/// <summary>The possible button states.</summary>
	public enum ButtonState {

		/// <summary>The button is released.</summary>
		Released = 0,

		/// <summary>The button is pressed.</summary>
		Pressed = 1,
	}

	/// <summary>The possible cursors.</summary>
	public enum Cursor {

		/// <summary>Cursor 1.</summary>
		Cursor1 = 0,

		/// <summary>Cursor 2.</summary>
		Cursor2 = 1,
	}

	/// <summary>The keys on the keyboard.</summary>
	public enum Key : byte {

		/// <summary>No key.</summary>
		None = 0,

		/// <summary>A.</summary>
		A = 4,

		/// <summary>B.</summary>
		B,

		/// <summary>C.</summary>
		C,

		/// <summary>D.</summary>
		D,

		/// <summary>E.</summary>
		E,

		/// <summary>F.</summary>
		F,

		/// <summary>G.</summary>
		G,

		/// <summary>H.</summary>
		H,

		/// <summary>I.</summary>
		I,

		/// <summary>J.</summary>
		J,

		/// <summary>K.</summary>
		K,

		/// <summary>L.</summary>
		L,

		/// <summary>M.</summary>
		M,

		/// <summary>N.</summary>
		N,

		/// <summary>O.</summary>
		O,

		/// <summary>P.</summary>
		P,

		/// <summary>Q.</summary>
		Q,

		/// <summary>R.</summary>
		R,

		/// <summary>S.</summary>
		S,

		/// <summary>T.</summary>
		T,

		/// <summary>U.</summary>
		U,

		/// <summary>V.</summary>
		V,

		/// <summary>W.</summary>
		W,

		/// <summary>X.</summary>
		X,

		/// <summary>Y.</summary>
		Y,

		/// <summary>Z.</summary>
		Z,

		/// <summary>1 or !.</summary>
		D1,

		/// <summary>2 or @.</summary>
		D2,

		/// <summary>3 or #.</summary>
		D3,

		/// <summary>4 or $.</summary>
		D4,

		/// <summary>5 or %.</summary>
		D5,

		/// <summary>6 or ^.</summary>
		D6,

		/// <summary>7 or &amp;.</summary>
		D7,

		/// <summary>8 or *.</summary>
		D8,

		/// <summary>9 or (.</summary>
		D9,

		/// <summary>0 or ).</summary>
		D0,

		/// <summary>Enter.</summary>
		Enter,

		/// <summary>Esc.</summary>
		Escape,

		/// <summary>Backspace.</summary>
		BackSpace,

		/// <summary>Tab.</summary>
		Tab,

		/// <summary>Space.</summary>
		Space,

		/// <summary>- or _.</summary>
		Substract,

		/// <summary>= or +.</summary>
		Equal,

		/// <summary>[ or {.</summary>
		OpenBrackets,

		/// <summary>] or }.</summary>
		CloseBrackets,

		/// <summary>\ or |.</summary>
		Backslash,

		/// <summary>Non-US keyboard character.</summary>
		NonUS,

		/// <summary>; or :.</summary>
		Semicolon,

		/// <summary>' or ".</summary>
		Quotes,

		/// <summary>` or ~.</summary>
		GraveAccent,

		/// <summary>, or &lt;.</summary>
		Comma,

		/// <summary>. or &gt;.</summary>
		Period,

		/// <summary>/ or ?.</summary>
		Divide,

		/// <summary>Caps Lock.</summary>
		CapsLock,

		/// <summary>F1.</summary>
		F1,

		/// <summary>F2.</summary>
		F2,

		/// <summary>F3.</summary>
		F3,

		/// <summary>F4.</summary>
		F4,

		/// <summary>F5.</summary>
		F5,

		/// <summary>F6.</summary>
		F6,

		/// <summary>F7.</summary>
		F7,

		/// <summary>F8.</summary>
		F8,

		/// <summary>F9.</summary>
		F9,

		/// <summary>F10.</summary>
		F10,

		/// <summary>F11.</summary>
		F11,

		/// <summary>F12.</summary>
		F12,

		/// <summary>Print Screen.</summary>
		PrintScreen,

		/// <summary>Scroll Lock.</summary>
		ScrollLock,

		/// <summary>Pause.</summary>
		Pause,

		/// <summary>Insert.</summary>
		Insert,

		/// <summary>Home.</summary>
		Home,

		/// <summary>Page Up.</summary>
		PageUp,

		/// <summary>Delete.</summary>
		Delete,

		/// <summary>End.</summary>
		End,

		/// <summary>Page Down.</summary>
		PageDown,

		/// <summary>Right Arrow.</summary>
		RightArrow,

		/// <summary>Left Arrow.</summary>
		LeftArrow,

		/// <summary>Down Arrow.</summary>
		DownArrow,

		/// <summary>Up Arrow.</summary>
		UpArrow,

		/// <summary>Num Lock.</summary>
		NumLock,

		/// <summary>Keypad /.</summary>
		KeypadDivide,

		/// <summary>Keypad *.</summary>
		KeypadMultiply,

		/// <summary>Keypad -.</summary>
		KeypadSubstract,

		/// <summary>Keypad +.</summary>
		KeypadAdd,

		/// <summary>Keypad Enter.</summary>
		KeypadEnter,

		/// <summary>Keypad 1 or End.</summary>
		Keypad1,

		/// <summary>Keypad 2 or Down Arrow.</summary>
		Keypad2,

		/// <summary>Keypad 3 or Page Down.</summary>
		Keypad3,

		/// <summary>Keypad 4 or Left Arrow.</summary>
		Keypad4,

		/// <summary>Keypad 5.</summary>
		Keypad5,

		/// <summary>Keypad 6 or Right Arrow.</summary>
		Keypad6,

		/// <summary>Keypad 7 or Home.</summary>
		Keypad7,

		/// <summary>Keypad 8 or Up Arrow.</summary>
		Keypad8,

		/// <summary>Keypad 9 or Page Up.</summary>
		Keypad9,

		/// <summary>Keypad 0 or Insert.</summary>
		Keypad0,

		/// <summary>Keypad . or Delete.</summary>
		KeypadDelete,

		/// <summary>Non-US keyboard character.</summary>
		NonUS2,

		/// <summary>Application.</summary>
		Application,

		/// <summary>Left Ctrl.</summary>
		LeftCtrl = 0xE0,

		/// <summary>Left Shift.</summary>
		LeftShift,

		/// <summary>Left Alt.</summary>
		LeftAlt,

		/// <summary>Left GUI.</summary>
		LeftGUI,

		/// <summary>Right Ctrl.</summary>
		RightCtrl,

		/// <summary>Right Shift.</summary>
		RightShift,

		/// <summary>Right Alt.</summary>
		RightAlt,

		/// <summary>Right GUI.</summary>
		RightGUI
	};

	/// <summary>The possible key states.</summary>
	public enum KeyState {

		/// <summary>The key is released.</summary>
		Released = 2,

		/// <summary>The key is released.</summary>
		Pressed = 1
	};

	/// <summary>Joystick hat switch directions.</summary>
	public enum HatSwitchDirection {

		/// <summary>Up.</summary>
		Up,

		/// <summary>Up right.</summary>
		UpRight,

		/// <summary>Right.</summary>
		Right,

		/// <summary>Down right.</summary>
		DownRight,

		/// <summary>Down.</summary>
		Down,

		/// <summary>Down left.</summary>
		DownLeft,

		/// <summary>Left.</summary>
		Left,

		/// <summary>Up left.</summary>
		UpLeft,

		/// <summary>Default position.</summary>
		None
	}

	/// <summary>Represents the a generic position.</summary>
	public struct Position {
		private int x;
		private int y;
		private int z;

		/// <summary>The X coordinate.</summary>
		public int X { get => this.x; set => this.x = value; }

        /// <summary>The Y coordinate.</summary>
        public int Y { get => this.y; set => this.y = value; }

        /// <summary>The Z coordinate.</summary>
        public int Z { get => this.z; set => this.z = value; }

        /// <summary>Subtracts one position from another.</summary>
        /// <param name="lhs">The position from which to subtract.</param>
        /// <param name="rhs">The position to subtract.</param>
        /// <returns>The new positon.</returns>
        public static Position operator -(Position lhs, Position rhs) => new Position() { X = lhs.X - rhs.X, Y = lhs.Y - rhs.Y, Z = lhs.Z - rhs.Z };

        /// <summary>Returns a string representation of the position.</summary>
        /// <returns>The position.</returns>
        public override string ToString() => "(" + this.X.ToString() + ", " + this.Y.ToString() + ", " + this.Z.ToString() + ")";
    }
	/// <summary>The exception thrown when an operation times out.</summary>
	[Serializable]
	public class OperationTimedOutException : System.Exception {

		internal OperationTimedOutException()
			: base() {
		}

		internal OperationTimedOutException(string message)
			: base(message) {
		}

		internal OperationTimedOutException(string message, Exception innerException)
			: base(message, innerException) {
		}
	}
}
