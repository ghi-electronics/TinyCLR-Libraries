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

    internal class DefaultDisplayControllerProvider : IDisplayControllerProvider {
        public static DefaultDisplayControllerProvider Instance { get; } = new DefaultDisplayControllerProvider();

        private DefaultDisplayControllerProvider() { }

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
