using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Display.Provider;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Display {
    public sealed class DisplayController : IDisposable {
        public IDisplayControllerProvider Provider { get; }

        private DisplayController(IDisplayControllerProvider provider) => this.Provider = provider;

        public static DisplayController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.DisplayController) is DisplayController c ? c : DisplayController.FromName(NativeApi.GetDefaultName(NativeApiType.DisplayController));
        public static DisplayController FromName(string name) => DisplayController.FromProvider(new DisplayControllerApiWrapper(NativeApi.Find(name, NativeApiType.DisplayController)));
        public static DisplayController FromProvider(IDisplayControllerProvider provider) => new DisplayController(provider);

        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : IntPtr.Zero;

        public DisplayControllerSettings ActiveConfiguration { get; private set; }

        public DisplayInterface Interface => this.Provider.Interface;
        public DisplayDataFormat[] SupportedDataFormats => this.Provider.SupportedDataFormats;

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public void DrawBuffer(int targetX, int targetY, int sourceX, int sourceY, int width, int height, int originalWidth, byte[] data, int offset) => this.Provider.DrawBuffer(targetX, targetY, sourceX, sourceY, width, height, originalWidth, data, offset);
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
        VerticalByteStrip1Bpp = 2,
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
        public string ApiName { get; set; }
        public SpiConnectionSettings Settings { get; set; }
    }

    public class I2cDisplayControllerSettings : DisplayControllerSettings {
        public string ApiName { get; set; }
        public I2cConnectionSettings Settings { get; set; }
    }

    namespace Provider {
        public interface IDisplayControllerProvider : IDisposable {
            DisplayInterface Interface { get; }
            DisplayDataFormat[] SupportedDataFormats { get; }

            void Enable();
            void Disable();
            void SetConfiguration(DisplayControllerSettings configuration);
            void DrawBuffer(int targetX, int targetY, int sourceX, int sourceY, int width, int height, int originalWidth, byte[] data, int offset);
            void DrawPixel(int x, int y, long color);
            void DrawString(string value);
        }

        public sealed class DisplayControllerApiWrapper : IDisplayControllerProvider, IApiImplementation {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            IntPtr IApiImplementation.Implementation => this.impl;

            public DisplayControllerApiWrapper(NativeApi api) {
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
            public extern void DrawBuffer(int targetX, int targetY, int sourceX, int sourceY, int width, int height, int originalWidth, byte[] data, int offset);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawPixel(int x, int y, long color);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawString(string value);

            public void SetConfiguration(DisplayControllerSettings configuration) {
                switch (this.Interface) {
                    case DisplayInterface.Parallel when configuration is ParallelDisplayControllerSettings pcfg:
                        this.SetConfiguration(pcfg);
                        break;

                    case DisplayInterface.Spi when configuration is SpiDisplayControllerSettings scfg:
                        this.SetConfiguration(scfg);
                        break;

                    case DisplayInterface.I2c when configuration is I2cDisplayControllerSettings icfg:
                        this.SetConfiguration(icfg);
                        break;

                    default:
                        throw new ArgumentException("Must pass an instance whose type matches the interface type.");
                }
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetConfiguration(ParallelDisplayControllerSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetConfiguration(SpiDisplayControllerSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetConfiguration(I2cDisplayControllerSettings settings);

            public extern DisplayInterface Interface { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern DisplayDataFormat[] SupportedDataFormats { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        }
    }
}
