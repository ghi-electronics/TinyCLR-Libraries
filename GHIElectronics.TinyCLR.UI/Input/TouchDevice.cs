////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>
    ///     The TouchDevice class represents the stylus/touch device to the
    ///     members of a context.
    /// </summary>
    public sealed class TouchDevice : InputDevice {
        internal TouchDevice(InputManager inputManager) {
            this._inputManager = inputManager;

            this._inputManager.InputDeviceEvents[(int)InputManager.InputDeviceType.Touch].PostProcessInput += new ProcessInputEventHandler(this.PostProcessInput);
        }

        public override UIElement Target => this._focus;

        public override InputManager.InputDeviceType DeviceType => InputManager.InputDeviceType.Touch;

        public void SetTarget(UIElement target) => this._focus = target;

        private void PostProcessInput(object sender, ProcessInputEventArgs e) {
            if (!e.StagingItem.Input.Handled) {
                var routedEvent = e.StagingItem.Input.RoutedEvent;
                if (routedEvent == InputManager.InputReportEvent) {
                    if (e.StagingItem.Input is InputReportEventArgs input) {
                        if (input.Report is RawTouchInputReport report) {
                            var args = new TouchEventArgs(
                            this,
                            report.Timestamp,
                            report.Touches);

                            var target = report.Target;
                            if (report.EventMessage == (byte)TouchMessages.Down) {
                                args.RoutedEvent = TouchEvents.TouchDownEvent;
                            }
                            else if (report.EventMessage == (byte)TouchMessages.Up) {
                                args.RoutedEvent = TouchEvents.TouchUpEvent;
                            }
                            else if (report.EventMessage == (byte)TouchMessages.Move) {
                                args.RoutedEvent = TouchEvents.TouchMoveEvent;
                            }
                            else
                                throw new Exception("Unknown touch event.");

                            args.Source = (target ?? this._focus);
                            e.PushInput(args, e.StagingItem);
                        }
                    }
                }
            }
        }

        private InputManager _inputManager;
        private UIElement _focus;
    }
}


