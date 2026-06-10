////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Input {
    /// <summary>The delegate to use for handlers that receive GenericEventArgs.</summary>
    public delegate void GenericEventHandler(object sender, GenericEventArgs e);

    /// <summary>Base class for raw input events reported to the input system.</summary>
    public class BaseEvent {
        /// <summary>The identifier of the source that produced the event.</summary>
        public ushort Source;
        /// <summary>The message code describing the event.</summary>
        public byte EventMessage;
    }

    /// <summary>Represents a generic input event with category, data and position.</summary>
    public class GenericEvent : BaseEvent {
        /// <summary>The category of the event.</summary>
        public byte EventCategory;
        /// <summary>The data payload associated with the event.</summary>
        public uint EventData;
        /// <summary>The x coordinate associated with the event.</summary>
        public int X;
        /// <summary>The y coordinate associated with the event.</summary>
        public int Y;
        /// <summary>The time when the event occurred.</summary>
        public DateTime Time;
    }

    /// <summary>Contains information about a generic input event.</summary>
    public class GenericEventArgs : InputEventArgs {
        /// <summary>Constructs an instance of the GenericEventArgs class.</summary>
        public GenericEventArgs(InputDevice inputDevice, GenericEvent genericEvent)
            : base(inputDevice, genericEvent.Time) => this.InternalEvent = genericEvent;

        /// <summary>Read-only access to the underlying generic event.</summary>
        public readonly GenericEvent InternalEvent;
    }

    /// <summary>Defines the routed events raised by the generic input device.</summary>
    public sealed class GenericEvents {
        // Fields
        /// <summary>A routed event raised for a standard generic input event.</summary>
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

        /// <summary>Returns the element that input from this device is sent to.</summary>
        public override UIElement Target {
            get {
                VerifyAccess();

                return this._focus;
            }
        }

        /// <summary>Sets the element that input from this device is sent to.</summary>
        public void SetTarget(UIElement target) => this._focus = target;

        /// <summary>The input device type for this device.</summary>
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


