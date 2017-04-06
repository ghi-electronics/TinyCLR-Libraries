namespace GHIElectronics.TinyCLR.Devices.Display {
    public class DisplayControllerSettings {
        public uint Width { get; set; }
        public uint Height { get; set; }
    }

    public class LcdControllerSettings : DisplayControllerSettings {
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
}
