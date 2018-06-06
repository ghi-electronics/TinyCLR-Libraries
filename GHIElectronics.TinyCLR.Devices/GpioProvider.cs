using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.Gpio.Provider {
    public delegate void GpioPinProviderValueChangedEventHandler(IGpioPinProvider sender, GpioPinProviderValueChangedEventArgs e);

    public enum ProviderGpioSharingMode {
        Exclusive = 0,
        SharedReadOnly,
    }

    public enum ProviderGpioPinDriveMode {
        Input = 0,
        Output,
        InputPullUp,
        InputPullDown,
        OutputOpenDrain,
        OutputOpenDrainPullUp,
        OutputOpenSource,
        OutputOpenSourcePullDown,
    }

    public enum ProviderGpioPinValue {
        Low = 0,
        High,
    }

    public enum ProviderGpioPinEdge {
        FallingEdge = 0,
        RisingEdge,
    }

    public sealed class GpioPinProviderValueChangedEventArgs {
        private ProviderGpioPinEdge m_edge;

        internal GpioPinProviderValueChangedEventArgs(ProviderGpioPinEdge edge) => this.m_edge = edge;

        public ProviderGpioPinEdge Edge => this.m_edge;
    }

    public interface IGpioPinProvider : IDisposable {
        TimeSpan DebounceTimeout { get; set; }
        int PinNumber { get; }
        ProviderGpioSharingMode SharingMode { get; }

        ProviderGpioPinDriveMode GetDriveMode();
        void SetDriveMode(ProviderGpioPinDriveMode value);
        bool IsDriveModeSupported(ProviderGpioPinDriveMode driveMode);
        ProviderGpioPinValue Read();
        void Write(ProviderGpioPinValue value);
        event GpioPinProviderValueChangedEventHandler ValueChanged;
    }

    public interface IGpioProvider {
        IGpioControllerProvider[] GetControllers();
    }

    public interface IGpioControllerProvider {
        int PinCount { get; }

        IGpioPinProvider OpenPinProvider(int pin, ProviderGpioSharingMode sharingMode);
    }

    public class GpioProvider : IGpioProvider {
        private IGpioControllerProvider[] controllers;
        private readonly static Hashtable providers = new Hashtable();

        public string Name { get; }

        public IGpioControllerProvider[] GetControllers() => this.controllers;

        private GpioProvider(string name) {
            var api = Api.Find(name, ApiType.GpioProvider);

            this.Name = name;

            this.controllers = new IGpioControllerProvider[DefaultGpioControllerProvider.GetControllerCount(api.Implementation)];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultGpioControllerProvider(name, i, api.Implementation);
        }

        public static IGpioProvider FromId(string id) {
            if (GpioProvider.providers.Contains(id))
                return (IGpioProvider)GpioProvider.providers[id];

            var res = new GpioProvider(id);

            GpioProvider.providers[id] = res;

            return res;
        }
    }

    internal class DefaultGpioControllerProvider : IGpioControllerProvider {
        private readonly IntPtr nativeProvider;
        private readonly ArrayList exclusivePins;
        private readonly Hashtable acquiredPins;
        public readonly string Name;
        public readonly uint Index;

        internal DefaultGpioControllerProvider(string name, uint index, IntPtr nativeProvider) {
            this.exclusivePins = new ArrayList();
            this.acquiredPins = new Hashtable();
            this.Name = name;
            this.Index = index;

            this.nativeProvider = nativeProvider;

            this.AcquireNative();
        }

        ~DefaultGpioControllerProvider() => this.ReleaseNative();

        public extern int PinCount {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public IGpioPinProvider OpenPinProvider(int pin, ProviderGpioSharingMode sharingMode) {
            if (sharingMode == ProviderGpioSharingMode.Exclusive) {
                if (this.exclusivePins.Contains(pin)) throw new InvalidOperationException("Sharing conflict.");

                this.exclusivePins.Add(pin);
            }

            if (this.acquiredPins.Contains(pin)) {
                this.acquiredPins[pin] = ((int)this.acquiredPins[pin]) + 1;
            }
            else {
                this.AcquireNative(pin);
                this.acquiredPins[pin] = 1;
            }

            return new DefaultGpioPinProvider(this, pin, this.nativeProvider, sharingMode);
        }

        public void Release(DefaultGpioPinProvider provider) {
            if (provider.SharingMode == ProviderGpioSharingMode.Exclusive)
                this.exclusivePins.Remove(provider.PinNumber);

            var cur = (int)this.acquiredPins[provider.PinNumber];

            if (--cur == 0) {
                this.ReleaseNative(provider.PinNumber);
                this.acquiredPins.Remove(provider.PinNumber);
            }
            else {
                this.acquiredPins[provider.PinNumber] = cur;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void AcquireNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void ReleaseNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void AcquireNative(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void ReleaseNative(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern static internal int GetControllerCount(IntPtr nativeProvider);
    }

    internal sealed class DefaultGpioPinProvider : IGpioPinProvider {
        private static GpioPinEventListener s_eventListener = new GpioPinEventListener();

        private object m_syncLock = new object();
        private bool m_disposed = false;
        private int m_pinNumber = -1;

        private ProviderGpioPinDriveMode m_driveMode = ProviderGpioPinDriveMode.Input;
        private ProviderGpioPinValue m_lastOutputValue = ProviderGpioPinValue.Low;
        private GpioPinProviderValueChangedEventHandler m_callbacks = null;
        private readonly IntPtr nativeProvider;
        private readonly DefaultGpioControllerProvider parent;

        internal DefaultGpioPinProvider(DefaultGpioControllerProvider parent, int pinNumber, IntPtr provider, ProviderGpioSharingMode sharingMode) {
            if (this.m_lastOutputValue == ProviderGpioPinValue.Low) { } // Silence an unused variable warning.

            this.parent = parent;
            this.nativeProvider = provider;
            this.m_pinNumber = pinNumber;

            this.SharingMode = sharingMode;

            s_eventListener.AddPin(this.parent.Name, this.parent.Index, this);
        }

        ~DefaultGpioPinProvider() {
            Dispose(false);
        }

        /// <summary>
        /// Occurs when the value of the general-purpose I/O (GPIO) pin changes, either because of an external stimulus
        /// when the pin is configured as an input, or when a value is written to the pin when the pin is configured as
        /// an output.
        /// </summary>
        public event GpioPinProviderValueChangedEventHandler ValueChanged {
            add {
                lock (this.m_syncLock) {
                    if (this.m_disposed) {
                        throw new ObjectDisposedException();
                    }

                    var callbacksOld = this.m_callbacks;
                    var callbacksNew = (GpioPinProviderValueChangedEventHandler)Delegate.Combine(callbacksOld, value);

                    try {
                        this.m_callbacks = callbacksNew;
                        SetDriveModeInternal(this.m_driveMode);
                    }
                    catch {
                        this.m_callbacks = callbacksOld;
                        throw;
                    }
                }
            }

            remove {
                lock (this.m_syncLock) {
                    if (this.m_disposed) {
                        throw new ObjectDisposedException();
                    }

                    var callbacksOld = this.m_callbacks;
                    var callbacksNew = (GpioPinProviderValueChangedEventHandler)Delegate.Remove(callbacksOld, value);

                    try {
                        this.m_callbacks = callbacksNew;
                        SetDriveModeInternal(this.m_driveMode);
                    }
                    catch {
                        this.m_callbacks = callbacksOld;
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the debounce timeout for the general-purpose I/O (GPIO) pin, which is an interval during which
        /// changes to the value of the pin are filtered out and do not generate <c>ValueChanged</c> events.
        /// </summary>
        /// <value> The debounce timeout for the GPIO pin, which is an interval during which changes to the value of the
        ///     pin are filtered out and do not generate <c>ValueChanged</c> events. If the length of this interval is
        ///     0, all changes to the value of the pin generate <c>ValueChanged</c> events.</value>
        extern public TimeSpan DebounceTimeout {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        /// <summary>
        /// Gets the pin number of the general-purpose I/O (GPIO) pin.
        /// </summary>
        /// <value>The pin number of the GPIO pin.</value>
        public int PinNumber {
            get {
                lock (this.m_syncLock) {
                    if (this.m_disposed) {
                        throw new ObjectDisposedException();
                    }

                    return this.m_pinNumber;
                }
            }
        }

        /// <summary>
        /// Gets the sharing mode in which the general-purpose I/O (GPIO) pin is open.
        /// </summary>
        /// <value>The sharing mode in which the GPIO pin is open.</value>
        public ProviderGpioSharingMode SharingMode { get; }

        /// <summary>
        /// Reads the current value of the general-purpose I/O (GPIO) pin.
        /// </summary>
        /// <returns>The current value of the GPIO pin. If the pin is configured as an output, this value is the last
        ///     value written to the pin.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public ProviderGpioPinValue Read();

        /// <summary>
        /// Drives the specified value onto the general purpose I/O (GPIO) pin according to the current drive mode for
        /// the pin if the pin is configured as an output, or updates the latched output value for the pin if the pin is
        /// configured as an input.
        /// </summary>
        /// <param name="value">The enumeration value to write to the GPIO pin.
        ///     <para>If the GPIO pin is configured as an output, the method drives the specified value onto the pin
        ///         according to the current drive mode for the pin.</para>
        ///     <para>If the GPIO pin is configured as an input, the method updates the latched output value for the pin.
        ///         The latched output value is driven onto the pin when the configuration for the pin changes to
        ///         output.</para></param>
        /// <remarks>If the pin drive mode is not currently set to output, this will latch <paramref name="value"/>
        ///     and drive the signal the when the mode is set.</remarks>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Write(ProviderGpioPinValue value);

        /// <summary>
        /// Gets whether the general-purpose I/O (GPIO) pin supports the specified drive mode.
        /// </summary>
        /// <param name="driveMode">The drive mode to check for support.</param>
        /// <returns>True if the GPIO pin supports the drive mode that driveMode specifies; otherwise false. If you
        ///     specify a drive mode for which this method returns false when you call SetDriveMode, SetDriveMode
        ///     generates an exception.</param>
        public bool IsDriveModeSupported(ProviderGpioPinDriveMode driveMode) {
            switch (driveMode) {
                case ProviderGpioPinDriveMode.Input:
                case ProviderGpioPinDriveMode.Output:
                case ProviderGpioPinDriveMode.InputPullUp:
                case ProviderGpioPinDriveMode.InputPullDown:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current drive mode for the general-purpose I/O (GPIO) pin. The drive mode specifies whether the pin
        /// is configured as an input or an output, and determines how values are driven onto the pin.
        /// </summary>
        /// <returns>An enumeration value that indicates the current drive mode for the GPIO pin. The drive mode
        ///     specifies whether the pin is configured as an input or an output, and determines how values are driven
        ///     onto the pin.</returns>
        public ProviderGpioPinDriveMode GetDriveMode() {
            lock (this.m_syncLock) {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                return this.m_driveMode;
            }
        }

        /// <summary>
        /// Sets the drive mode of the general-purpose I/O (GPIO) pin. The drive mode specifies whether the pin is
        /// configured as an input or an output, and determines how values are driven onto the pin.
        /// </summary>
        /// <param name="driveMode">An enumeration value that specifies drive mode to use for the GPIO pin. The drive
        ///     mode specifies whether the pin is configured as an input or an output, and determines how values are
        ///     driven onto the pin.</param>
        public void SetDriveMode(ProviderGpioPinDriveMode driveMode) {
            lock (this.m_syncLock) {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                if (driveMode != this.m_driveMode) {
                    SetDriveModeInternal(driveMode);
                    this.m_driveMode = driveMode;
                }
            }
        }

        /// <summary>
        /// Closes the general-purpose I/O (GPIO) pin and releases the resources associated with it.
        /// </summary>
        public void Dispose() {
            lock (this.m_syncLock) {
                if (!this.m_disposed) {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                    this.m_disposed = true;
                }
            }
        }

        /// <summary>
        /// Handles internal events and re-dispatches them to the publicly subsribed delegates.
        /// </summary>
        /// <param name="edge">The state transition for this event.</param>
        internal void OnPinChangedInternal(ProviderGpioPinEdge edge) {
            GpioPinProviderValueChangedEventHandler callbacks = null;

            lock (this.m_syncLock) {
                if (!this.m_disposed) {
                    callbacks = this.m_callbacks;
                }
            }

            callbacks?.Invoke(this, new GpioPinProviderValueChangedEventArgs(edge));
        }

        /// <summary>
        /// Interop method to set the pin drive mode in hardware.
        /// </summary>
        /// <param name="driveMode">Drive mode to set.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void SetDriveModeInternal(ProviderGpioPinDriveMode driveMode);

        /// <summary>
        /// Releases internal resources held by the GPIO pin.
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from the finalizer.</param>
        private void Dispose(bool disposing) {
            if (disposing) {
                s_eventListener.RemovePin(this.parent.Name, this.parent.Index, this);
                this.parent.Release(this);
            }
        }
    }
}
