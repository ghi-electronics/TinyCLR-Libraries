////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Input {
    public delegate void GenericEventHandler(object sender, GenericEventArgs e);

    public class BaseEvent {
        public ushort Source;
        public byte EventMessage;
    }

    public class GenericEvent : BaseEvent {
        public byte EventCategory;
        public uint EventData;
        public int X;
        public int Y;
        public DateTime Time;
    }

    public class GenericEventArgs : InputEventArgs {
        public GenericEventArgs(InputDevice inputDevice, GenericEvent genericEvent)
            : base(inputDevice, genericEvent.Time) => this.InternalEvent = genericEvent;

        public readonly GenericEvent InternalEvent;
    }

    public sealed class GenericEvents {
        // Fields
        public static readonly RoutedEvent GenericStandardEvent = new RoutedEvent("GenericStandardEvent", RoutingStrategy.Tunnel, typeof(GenericEventArgs));
    }

    /// <summary>
    ///     The GenericDevice class represents the Generic device to the
    ///     members of a context.
    /// </summary>
    public sealed class GenericDevice : InputDevice {
        internal GenericDevice(InputManager inputManager) {
            this._inputManager = inputManager;

            this._inputManager.InputDeviceEvents[(int)InputManager.InputDeviceType.Generic].PostProcessInput += new ProcessInputEventHandler(this.PostProcessInput);
        }

        private UIElement _focus = null;

        public override UIElement Target {
            get {
                VerifyAccess();

                return this._focus;
            }
        }

        public void SetTarget(UIElement target) => this._focus = target;

        public override InputManager.InputDeviceType DeviceType => InputManager.InputDeviceType.Generic;

        private void PostProcessInput(object sender, ProcessInputEventArgs e) {
            if (e.StagingItem.Input is InputReportEventArgs input && input.RoutedEvent == InputManager.InputReportEvent) {

                if (input.Report is RawGenericInputReport report) {
                    if (!e.StagingItem.Input.Handled) {
                        var ge = (GenericEvent)report.InternalEvent;
                        var args = new GenericEventArgs(
                            this,
                            report.InternalEvent) {
                            RoutedEvent = GenericEvents.GenericStandardEvent
                        };
                        if (report.Target != null) {
                            args.Source = report.Target;
                        }

                        e.PushInput(args, e.StagingItem);
                    }
                }
            }
        }

        private InputManager _inputManager;
    }
}


