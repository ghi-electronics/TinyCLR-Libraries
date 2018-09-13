using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Display.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Display {
    public sealed class DisplayController : IDisposable {
        public IDisplayControllerProvider Provider { get; }

        private DisplayController(IDisplayControllerProvider provider) => this.Provider = provider;

        public static DisplayController GetDefault() => Api.GetDefaultFromCreator(ApiType.DisplayController) is DisplayController c ? c : DisplayController.FromName(Api.GetDefaultName(ApiType.DisplayController));
        public static DisplayController FromName(string name) => DisplayController.FromProvider(new DisplayControllerApiWrapper(Api.Find(name, ApiType.DisplayController)));
        public static DisplayController FromProvider(IDisplayControllerProvider provider) => new DisplayController(provider);

        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : throw new NotSupportedException();

        public DisplayControllerSettings ActiveConfiguration { get; private set; }

        public DisplayInterface Interface => this.Provider.Interface;
        public DisplayDataFormat[] SupportedDataFormats => this.Provider.SupportedDataFormats;

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public void DrawBuffer(int x, int y, int width, int height, byte[] data, int offset) => this.Provider.DrawBuffer(x, y, width, height, data, offset);
        public void DrawPixel(int x, int y, long color) => this.Provider.DrawPixel(x, y, color);
        public void DrawString(string value) => this.Provider.DrawString(value);

        public void SetConfiguration(DisplayControllerSettings configuration) {
            this.Provider.SetConfiguration(configuration);

            this.ActiveConfiguration = configuration;
        }
    }

    public enum DisplayInterface {
        Parallel = 0,
        Spi = 1,
        I2c = 2,
    }

    public enum DisplayDataFormat {
        Rgb565 = 0,
        Rgb444 = 1,
        VerticalStrip1Bpp = 2,
    }

    public class DisplayControllerSettings {
        public int Width { get; set; }
        public int Height { get; set; }
        public DisplayDataFormat DataFormat { get; set; }
    }

    public class ParallelDisplayControllerSettings : DisplayControllerSettings {
        public bool DataEnableIsFixed { get; set; }
        public bool DataEnablePolarity { get; set; }
        public bool PixelPolarity { get; set; }
        public int PixelClockRate { get; set; }
        public bool HorizontalSyncPolarity { get; set; }
        public int HorizontalSyncPulseWidth { get; set; }
        public int HorizontalFrontPorch { get; set; }
        public int HorizontalBackPorch { get; set; }
        public bool VerticalSyncPolarity { get; set; }
        public int VerticalSyncPulseWidth { get; set; }
        public int VerticalFrontPorch { get; set; }
        public int VerticalBackPorch { get; set; }
    }

    public class SpiDisplayControllerSettings : DisplayControllerSettings {
        public string SpiApiName { get; set; }
    }

    namespace Provider {
        public interface IDisplayControllerProvider : IDisposable {
            DisplayInterface Interface { get; }
            DisplayDataFormat[] SupportedDataFormats { get; }

            void Enable();
            void Disable();
            void SetConfiguration(DisplayControllerSettings configuration);
            void DrawBuffer(int x, int y, int width, int height, byte[] data, int offset);
            void DrawPixel(int x, int y, long color);
            void DrawString(string value);
        }

        public sealed class DisplayControllerApiWrapper : IDisplayControllerProvider, IApiImplementation {
            private readonly IntPtr impl;

            public Api Api { get; }

            IntPtr IApiImplementation.Implementation => this.impl;

            public DisplayControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawBuffer(int x, int y, int width, int height, byte[] data, int offset);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawPixel(int x, int y, long color);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawString(string value);

            public void SetConfiguration(DisplayControllerSettings configuration) {
                switch (this.Interface) {
                    case DisplayInterface.Parallel when configuration is ParallelDisplayControllerSettings pcfg:
                        this.SetParallelConfiguration(pcfg.Width, pcfg.Height, pcfg.DataFormat, pcfg.DataEnableIsFixed, pcfg.DataEnablePolarity, pcfg.PixelPolarity, pcfg.PixelClockRate, pcfg.HorizontalSyncPolarity, pcfg.HorizontalSyncPulseWidth, pcfg.HorizontalFrontPorch, pcfg.HorizontalBackPorch, pcfg.VerticalSyncPolarity, pcfg.VerticalSyncPulseWidth, pcfg.VerticalFrontPorch, pcfg.VerticalBackPorch);
                        break;

                    case DisplayInterface.Spi when configuration is SpiDisplayControllerSettings scfg:
                        this.SetSpiConfiguration(scfg.Width, scfg.Height, scfg.DataFormat, scfg.SpiApiName);
                        break;

                    default:
                        throw new ArgumentException("Must pass an instance whose type matches the interface type.");
                }
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetParallelConfiguration(int width, int height, DisplayDataFormat dataFormat, bool dataEnableIsFixed, bool dataEnablePolarity, bool pixelPolarity, int pixelClockRate, bool horizontalSyncPolarity, int horizontalSyncPulseWidth, int horizontalFrontPorch, int horizontalBackPorch, bool verticalSyncPolarity, int verticalSyncPulseWidth, int verticalFrontPorch, int verticalBackPorch);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetSpiConfiguration(int width, int height, DisplayDataFormat dataFormat, string spiApiName);

            public extern DisplayInterface Interface { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern DisplayDataFormat[] SupportedDataFormats { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        }
    }
}
