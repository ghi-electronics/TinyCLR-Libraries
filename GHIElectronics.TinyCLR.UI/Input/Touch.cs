////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Input {
    public delegate void TouchEventHandler(object sender, TouchEventArgs e);

    public enum CaptureMode {
        None,
        Element,
        SubTree,
    }

    public static class TouchCapture {
        public static bool Capture(UIElement element) => Capture(element, CaptureMode.Element);

        public static bool Capture(UIElement element, CaptureMode mode) {
            if (mode != CaptureMode.None) {
                if (element == null) {
                    throw new ArgumentException();
                }

                /// Make sure the element is attached
                /// to the MainWindow subtree.
                if (!IsMainWindowChild(element)) {
                    throw new ArgumentException();
                }

                if (mode == CaptureMode.SubTree) {
                    throw new NotImplementedException();
                }

                if (mode == CaptureMode.Element) {
                    _captureElement = element;
                }
            }
            else {
                _captureElement = null;
            }

            return true;
        }

        public static UIElement Captured => _captureElement;

        private static bool IsMainWindowChild(UIElement element) {
            UIElement mainWindow = Application.Current.MainWindow;
            while (element != null) {
                if (element == mainWindow)
                    return true;

                element = element.Parent;
            }

            return false;
        }

        private static UIElement _captureElement = null;
    }

    public sealed class TouchEvents {
        // Fields
        public static readonly RoutedEvent TouchDownEvent = new RoutedEvent("TouchDownEvent", RoutingStrategy.Tunnel, typeof(TouchEventArgs));
        public static readonly RoutedEvent TouchMoveEvent = new RoutedEvent("TouchMoveEvent", RoutingStrategy.Tunnel, typeof(TouchEventArgs));
        public static readonly RoutedEvent TouchUpEvent = new RoutedEvent("TouchUpEvent", RoutingStrategy.Tunnel, typeof(TouchEventArgs));
    }

    public class TouchEventArgs : InputEventArgs {
        // Fields
        public TouchInput[] Touches;

        // Methods
        public TouchEventArgs(InputDevice inputDevice, DateTime timestamp, TouchInput[] touches)
            : base(inputDevice, timestamp) => this.Touches = touches;

        public void GetPosition(UIElement relativeTo, int touchIndex, out int x, out int y) {
            x = this.Touches[touchIndex].X;
            y = this.Touches[touchIndex].Y;

            relativeTo.PointToClient(ref x, ref y);
        }
    }

    public enum TouchMessages : byte {
        Down = 1,
        Up = 2,
        Move = 3,
    }

    public class TouchInput {
        public int X;
        public int Y;
    }

    public class TouchEvent : BaseEvent {
        public DateTime Time;
        public TouchInput[] Touches;
    }

    public enum TouchGesture : uint {
        NoGesture = 0,          //Can be used to represent an error gesture or unknown gesture

        //Standard Win7 Gestures
        Begin = 1,       //Used to identify the beginning of a Gesture Sequence; App can use this to highlight UIElement or some other sort of notification.
        End = 2,       //Used to identify the end of a gesture sequence; Fired when last finger involved in a gesture is removed.

        // Standard stylus (single touch) gestues
        Right = 3,
        UpRight = 4,
        Up = 5,
        UpLeft = 6,
        Left = 7,
        DownLeft = 8,
        Down = 9,
        DownRight = 10,
        Tap = 11,
        DoubleTap = 12,

        // Multi-touch gestures
        Zoom = 114,      //Equivalent to your "Pinch" gesture
        Pan = 115,      //Equivalent to your "Scroll" gesture
        Rotate = 116,
        TwoFingerTap = 117,
        Rollover = 118,      // Press and tap

        //Additional NetMF gestures
        UserDefined = 200,
    }

    public class TouchGestureEventArgs : EventArgs {
        public readonly DateTime Timestamp;

        public TouchGesture Gesture;

        ///<note> X and Y form the center location of the gesture for multi-touch or the starting location for single touch </note>
        public int X;
        public int Y;

        /// <note>2 bytes for gesture-specific arguments.
        /// TouchGesture.Zoom: Arguments = distance between fingers
        /// TouchGesture.Rotate: Arguments = angle in degrees (0-360)
        /// </note>
        public ushort Arguments;

        public double Angle => (double)(this.Arguments);
    }

    public delegate void TouchGestureEventHandler(object sender, TouchGestureEventArgs e);
}


