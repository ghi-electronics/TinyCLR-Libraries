using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.Display.Provider {
    public interface IDisplayProvider {
        IDisplayControllerProvider[] GetControllers();
    }

    public interface IDisplayControllerProvider {
        IntPtr Hdc { get; }
        DisplayInterface Interface { get; }
        DisplayDataFormat[] SupportedDataFormats { get; }

        void ApplySettings(DisplayControllerSettings settings);
        void WriteString(string str);
    }

    public class DisplayProvider : IDisplayProvider {
        private IDisplayControllerProvider[] controllers;
        private static Hashtable providers = new Hashtable();

        public string Name { get; }

        public IDisplayControllerProvider[] GetControllers() => this.controllers;

        private DisplayProvider(string name) {
            var api = Api.Find(name, ApiType.DisplayProvider);

            this.Name = name;
            this.controllers = new IDisplayControllerProvider[api.Count];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultDisplayControllerProvider(api.Implementation[i]);
        }

        public static IDisplayProvider FromId(string id) {
            if (DisplayProvider.providers.Contains(id))
                return (IDisplayProvider)DisplayProvider.providers[id];

            var res = new DisplayProvider(id);

            DisplayProvider.providers[id] = res;

            return res;
        }
    }

    internal class DefaultDisplayControllerProvider : IDisplayControllerProvider {
#pragma warning disable CS0169
        private readonly IntPtr nativeProvider;
#pragma warning restore CS0169

        internal DefaultDisplayControllerProvider(IntPtr nativeProvider) => this.nativeProvider = nativeProvider;

        public IntPtr Hdc => this.nativeProvider;

        public extern DisplayInterface Interface {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern DisplayDataFormat[] SupportedDataFormats {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public void ApplySettings(DisplayControllerSettings settings) {
            if (this.Interface == DisplayInterface.Parallel && settings is ParallelDisplayControllerSettings pcfg) {
                if (!this.SetParallelConfiguration(pcfg.Width, pcfg.Height, pcfg.DataFormat, pcfg.OutputEnableIsFixed, pcfg.OutputEnablePolarity, pcfg.PixelPolarity, pcfg.PixelClockRate, pcfg.HorizontalSyncPolarity, pcfg.HorizontalSyncPulseWidth, pcfg.HorizontalFrontPorch, pcfg.HorizontalBackPorch, pcfg.VerticalSyncPolarity, pcfg.VerticalSyncPulseWidth, pcfg.VerticalFrontPorch, pcfg.VerticalBackPorch))
                    throw new InvalidOperationException("Invalid settings passed.");
            }
            else if (this.Interface == DisplayInterface.Spi && settings is SpiDisplayControllerSettings scfg) {
                if (!this.SetSpiConfiguration(scfg.Width, scfg.Height, scfg.DataFormat, scfg.SpiSelector))
                    throw new InvalidOperationException("Invalid settings passed.");
            }
            else {
                throw new ArgumentException($"Must pass an instance whose type matches the interface type.");
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void WriteString(string str);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool SetParallelConfiguration(uint width, uint height, DisplayDataFormat dataFormat, bool outputEnableIsFixed, bool outputEnablePolarity, bool pixelPolarity, uint pixelClockRate, bool horizontalSyncPolarity, uint horizontalSyncPulseWidth, uint horizontalFrontPorch, uint horizontalBackPorch, bool verticalSyncPolarity, uint verticalSyncPulseWidth, uint verticalFrontPorch, uint verticalBackPorch);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool SetSpiConfiguration(uint width, uint height, DisplayDataFormat dataFormat, string spiSelector);
    }
}
