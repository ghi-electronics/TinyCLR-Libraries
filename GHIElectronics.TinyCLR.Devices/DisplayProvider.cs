using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.Display.Provider {
    public interface IDisplayProvider {
        IDisplayControllerProvider[] GetControllers();
    }

    public interface IDisplayControllerProvider {
        IntPtr Hdc { get; }

        void ApplySettings(DisplayControllerSettings settings);
    }

    public class DisplayProvider : IDisplayProvider {
        private IDisplayControllerProvider[] controllers;

        public string Name { get; }

        public IDisplayControllerProvider[] GetControllers() => this.controllers;

        private DisplayProvider(string name) {
            this.Name = name;
            this.controllers = new IDisplayControllerProvider[DefaultDisplayControllerProvider.GetControllerCount(name)];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultDisplayControllerProvider(name, i);
        }

        public static IDisplayProvider FromId(string id) => new DisplayProvider(id);
    }

    internal class DefaultDisplayControllerProvider : IDisplayControllerProvider {
#pragma warning disable CS0169
        private IntPtr nativeProvider;
#pragma warning restore CS0169

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern uint GetControllerCount(string providerName);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern DefaultDisplayControllerProvider(string name, uint index);

        public IntPtr Hdc { get; private set; }

        public void ApplySettings(DisplayControllerSettings settings) {
            if (settings is LcdControllerSettings config) {
                if (DefaultDisplayControllerProvider.NativeSetLcdConfiguration(config.Width, config.Height, config.OutputEnableIsFixed, config.OutputEnablePolarity, config.PixelPolarity, config.PixelClockRate, config.HorizontalSyncPolarity, config.HorizontalSyncPulseWidth, config.HorizontalFrontPorch, config.HorizontalBackPorch, config.VerticalSyncPolarity, config.VerticalSyncPulseWidth, config.VerticalFrontPorch, config.VerticalBackPorch, out var hdc)) {
                    this.Hdc = hdc;
                }
                else {
                    throw new InvalidOperationException("Invalid settings passed.");
                }
            }
            else {
                throw new ArgumentException($"Must pass an instance of {nameof(LcdControllerSettings)}.");
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool NativeSetLcdConfiguration(uint width, uint height, bool outputEnableIsFixed, bool outputEnablePolarity, bool pixelPolarity, uint pixelClockRate, bool horizontalSyncPolarity, uint horizontalSyncPulseWidth, uint horizontalFrontPorch, uint horizontalBackPorch, bool verticalSyncPolarity, uint verticalSyncPulseWidth, uint verticalFrontPorch, uint verticalBackPorch, out IntPtr hdc);
    }
}
