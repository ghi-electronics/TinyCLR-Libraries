using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Display.Provider;

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

        public void DrawBuffer(uint x, uint y, uint width, uint height, byte[] data, int offset) => this.Provider.DrawBuffer(x, y, width, height, data, offset);
        public void DrawString(string value) => this.Provider.DrawString(value);

        public void SetConfiguration(DisplayControllerSettings configuration) {
            this.Provider.SetConfiguration(configuration);

            this.ActiveConfiguration = configuration;
        }
    }

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
        public bool DataEnableIsFixed { get; set; }
        public bool DataEnablePolarity { get; set; }
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
        public string SpiApiName { get; set; }
    }

    namespace Provider {
        public interface IDisplayControllerProvider : IDisposable {
            DisplayInterface Interface { get; }
            DisplayDataFormat[] SupportedDataFormats { get; }

            void Enable();
            void Disable();
            void SetConfiguration(DisplayControllerSettings configuration);
            void DrawBuffer(uint x, uint y, uint width, uint height, byte[] data, int offset);
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
            public extern void DrawBuffer(uint x, uint y, uint width, uint height, byte[] data, int offset);

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
            private extern void SetParallelConfiguration(uint width, uint height, DisplayDataFormat dataFormat, bool dataEnableIsFixed, bool dataEnablePolarity, bool pixelPolarity, uint pixelClockRate, bool horizontalSyncPolarity, uint horizontalSyncPulseWidth, uint horizontalFrontPorch, uint horizontalBackPorch, bool verticalSyncPolarity, uint verticalSyncPulseWidth, uint verticalFrontPorch, uint verticalBackPorch);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetSpiConfiguration(uint width, uint height, DisplayDataFormat dataFormat, string spiApiName);

            public extern DisplayInterface Interface { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern DisplayDataFormat[] SupportedDataFormats { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        }
    }
}
