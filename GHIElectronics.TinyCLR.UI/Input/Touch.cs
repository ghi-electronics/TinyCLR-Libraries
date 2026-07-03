////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>The delegate to use for handlers that receive TouchEventArgs.</summary>
    public delegate void TouchEventHandler(object sender, TouchEventArgs e);

    /// <summary>Specifies how touch input is captured to an element.</summary>
    public enum CaptureMode {
        /// <summary>No capture.</summary>
        None,
        /// <summary>Capture touch input to a single element.</summary>
        Element,
        /// <summary>Capture touch input to an element and its subtree.</summary>
        SubTree,
    }

    /// <summary>Provides methods for capturing touch input to an element.</summary>
    public static class TouchCapture {
        /// <summary>Captures touch input to the specified element.</summary>
        public static bool Capture(UIElement element) => Capture(element, CaptureMode.Element);

        /// <summary>Captures touch input to the specified element using the given capture mode.</summary>
        public static bool Capture(UIElement element, CaptureMode mode) {
            if (mode != CaptureMode.None) {
                if (element == null) {
                    throw new ArgumentException();
                }

                // Make sure the element is attached
                // to the MainWindow subtree.
                if (!IsMainWindowChild(element)) {
                    throw new ArgumentException();
                }

                // TinyCLR's touch dispatcher already routes by the captured
                // element's rendered bounds (see Application.cs ProcessInput),
                // so Element and SubTree modes produce the same observable
                // behavior here - descendants within the captured element's
                // bounds already receive events through the captured root.
                if (mode == CaptureMode.SubTree || mode == CaptureMode.Element) {
                    _captureElement = element;
                }
            }
            else {
                _captureElement = null;
            }

            return true;
        }

        /// <summary>Returns the element that currently has touch capture, or null.</summary>
        public static UIElement Captured => _captureElement;

        private static bool IsMainWindowChild(UIElement element) {
            // Touch may be captured by any element in the active window tree. All top-level Windows are children of
            // the WindowManager (not necessarily of Application.MainWindow), so validating only against MainWindow
            // broke multi-window apps: navigating by bringing a non-main Window to the top made WindowManager call
            // Capture on it, which threw here and wedged the UI dispatcher. Accept the WindowManager root too.
            UIElement mainWindow = Application.Current?.MainWindow;
            UIElement root = WindowManager.Instance;
            while (element != null) {
                if (element == mainWindow || element == root)
                    return true;

                element = element.Parent;
            }

            return false;
        }

        private static UIElement _captureElement = null;
    }

    /// <summary>Defines the routed events raised for touch input.</summary>
    public sealed class TouchEvents {
        /// <summary>A routed event raised when a touch press occurs.</summary>
        // Bubble routing: the deepest hit-tested element receives the event
        // first, and ancestors see it afterwards. This matches WPF's regular
        // Touch* events (its Preview* variants are the Tunnel ones, which we
        // don't currently expose). Controls that override OnTouchDown/Up to
        // implement their own behaviour rely on running before their parents
        // — Tunnel routing here would let an ancestor handler clobber state
        // (e.g. clearing a press flag) before the originating control's own
        // OnTouchUp can act on it. See Button.OnParentTouchUp for why.
        public static readonly RoutedEvent TouchDownEvent = new RoutedEvent("TouchDownEvent", RoutingStrategy.Bubble, typeof(TouchEventArgs));
        /// <summary>A routed event raised when a touch point moves.</summary>
        public static readonly RoutedEvent TouchMoveEvent = new RoutedEvent("TouchMoveEvent", RoutingStrategy.Bubble, typeof(TouchEventArgs));
        /// <summary>A routed event raised when a touch release occurs.</summary>
        public static readonly RoutedEvent TouchUpEvent = new RoutedEvent("TouchUpEvent", RoutingStrategy.Bubble, typeof(TouchEventArgs));
    }

    /// <summary>Contains information about a touch input event.</summary>
    public class TouchEventArgs : InputEventArgs {
        // Fields
        /// <summary>The touch points associated with this event.</summary>
        public TouchInput[] Touches;

        // Methods
        /// <summary>Constructs an instance of the TouchEventArgs class.</summary>
        public TouchEventArgs(InputDevice inputDevice, DateTime timestamp, TouchInput[] touches)
            : base(inputDevice, timestamp) => this.Touches = touches;

        /// <summary>Gets the position of a touch point relative to the specified element.</summary>
        public void GetPosition(UIElement relativeTo, int touchIndex, out int x, out int y) {
            x = this.Touches[touchIndex].X;
            y = this.Touches[touchIndex].Y;

            relativeTo.PointToClient(ref x, ref y);
        }
    }

    /// <summary>Identifies the kind of touch message.</summary>
    public enum TouchMessages : byte {
        /// <summary>A touch press.</summary>
        Down = 1,
        /// <summary>A touch release.</summary>
        Up = 2,
        /// <summary>A touch move.</summary>
        Move = 3,
    }

    /// <summary>Represents a single touch point.</summary>
    public class TouchInput {
        /// <summary>The x coordinate of the touch point.</summary>
        public int X;
        /// <summary>The y coordinate of the touch point.</summary>
        public int Y;
    }

    /// <summary>Represents a raw touch event with its touch points.</summary>
    public class TouchEvent : BaseEvent {
        /// <summary>The time when the event occurred.</summary>
        public DateTime Time;
        /// <summary>The touch points associated with the event.</summary>
        public TouchInput[] Touches;
    }

    /// <summary>Identifies a touch gesture.</summary>
    public enum TouchGesture : uint {
        /// <summary>No gesture, or an unknown gesture.</summary>
        NoGesture = 0,          //Can be used to represent an error gesture or unknown gesture

        /// <summary>The beginning of a gesture sequence.</summary>
        //Standard Win7 Gestures
        Begin = 1,       //Used to identify the beginning of a Gesture Sequence; App can use this to highlight UIElement or some other sort of notification.
        /// <summary>The end of a gesture sequence.</summary>
        End = 2,       //Used to identify the end of a gesture sequence; Fired when last finger involved in a gesture is removed.

        /// <summary>A swipe to the right.</summary>
        // Standard stylus (single touch) gestues
        Right = 3,
        /// <summary>A swipe up and to the right.</summary>
        UpRight = 4,
        /// <summary>A swipe upward.</summary>
        Up = 5,
        /// <summary>A swipe up and to the left.</summary>
        UpLeft = 6,
        /// <summary>A swipe to the left.</summary>
        Left = 7,
        /// <summary>A swipe down and to the left.</summary>
        DownLeft = 8,
        /// <summary>A swipe downward.</summary>
        Down = 9,
        /// <summary>A swipe down and to the right.</summary>
        DownRight = 10,
        /// <summary>A tap.</summary>
        Tap = 11,
        /// <summary>A double tap.</summary>
        DoubleTap = 12,

        /// <summary>A pinch (zoom) gesture.</summary>
        // Multi-touch gestures
        Zoom = 114,      //Equivalent to your "Pinch" gesture
        /// <summary>A pan (scroll) gesture.</summary>
        Pan = 115,      //Equivalent to your "Scroll" gesture
        /// <summary>A rotate gesture.</summary>
        Rotate = 116,
        /// <summary>A two-finger tap.</summary>
        TwoFingerTap = 117,
        /// <summary>A press-and-tap (rollover) gesture.</summary>
        Rollover = 118,      // Press and tap

        /// <summary>A user-defined gesture.</summary>
        //Additional NetMF gestures
        UserDefined = 200,
    }

    /// <summary>Contains information about a touch gesture event.</summary>
    public class TouchGestureEventArgs : EventArgs {
        /// <summary>The time when the gesture occurred.</summary>
        public readonly DateTime Timestamp;

        /// <summary>The gesture that was recognized.</summary>
        public TouchGesture Gesture;

        ///<note> X and Y form the center location of the gesture for multi-touch or the starting location for single touch </note>
        /// <summary>The x center of the gesture (or start point for single touch).</summary>
        public int X;
        /// <summary>The y coordinate of the gesture location.</summary>
        public int Y;

        /// <note>2 bytes for gesture-specific arguments.
        /// TouchGesture.Zoom: Arguments = distance between fingers
        /// TouchGesture.Rotate: Arguments = angle in degrees (0-360)
        /// </note>
        /// <summary>Gesture-specific value (e.g. zoom distance or rotation angle).</summary>
        public ushort Arguments;

        /// <summary>The gesture angle in degrees, derived from the arguments.</summary>
        public double Angle => (double)(this.Arguments);
    }

    /// <summary>The delegate to use for handlers that receive TouchGestureEventArgs.</summary>
    public delegate void TouchGestureEventHandler(object sender, TouchGestureEventArgs e);
}


