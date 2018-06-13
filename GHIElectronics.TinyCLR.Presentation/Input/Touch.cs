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
}


