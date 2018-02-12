namespace GHIElectronics.TinyCLR.Devices.Display {
    public enum DisplayInterface {
        Parallel = 0,
        Spi = 1,
    }

    public enum DisplayDataFormat {
        Rgb565 = 0,
    }

    public class DisplayControllerSettings {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public DisplayDataFormat DataFormat { get; set; }
    }

    public class ParallelDisplayControllerSettings : DisplayControllerSettings {
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
    }

    public class SpiDisplayControllerSettings : DisplayControllerSettings {
        public string SpiSelector { get; set; }
    }
}
