using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Graphics.Display {
    public sealed class DisplayConfiguration {
        private static DisplayConfiguration current;

        public IntPtr Hdc => new IntPtr(int.MaxValue);

        public uint Width { get; set; }
        public uint Height { get; set; }
        public bool OutputEnableIsFixed { get; set; }
        public bool OutputEnablePolarity { get; set; }
        public bool PixelPolarity { get; set; }
        public uint PixelClockRate { get; set; }
        public bool HorizontalSyncPolarity { get; set; }
        public uint HorizontalSyncPulseWidth { get; set; }
        public uint HorizontalFrontPorch { get; set; }
        public uint HorizontalBackPorch { get; set; }
        public bool VerticalSyncPolarity { get; set; }
        public uint VerticalSyncPulseWidth { get; set; }
        public uint VerticalFrontPorch { get; set; }
        public uint VerticalBackPorch { get; set; }

        public static DisplayConfiguration GetForCurrentDisplay() => DisplayConfiguration.current;

        public static bool SetForCurrentDisplay(DisplayConfiguration configuration) {
            var res = DisplayConfiguration.NativeSetLcdConfiguration(configuration.Width, configuration.Height, configuration.OutputEnableIsFixed, configuration.OutputEnablePolarity, configuration.PixelPolarity, configuration.PixelClockRate, configuration.HorizontalSyncPolarity, configuration.HorizontalSyncPulseWidth, configuration.HorizontalFrontPorch, configuration.HorizontalBackPorch, configuration.VerticalSyncPolarity, configuration.VerticalSyncPulseWidth, configuration.VerticalFrontPorch, configuration.VerticalBackPorch);

            if (res)
                DisplayConfiguration.current = configuration;

            return res;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool NativeSetLcdConfiguration(uint width, uint height, bool outputEnableIsFixed, bool outputEnablePolarity, bool pixelPolarity, uint pixelClockRate, bool horizontalSyncPolarity, uint horizontalSyncPulseWidth, uint horizontalFrontPorch, uint horizontalBackPorch, bool verticalSyncPolarity, uint verticalSyncPulseWidth, uint verticalFrontPorch, uint verticalBackPorch);
    }
}
