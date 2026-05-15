using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Display.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Display {
    /// <summary>
    /// Represents the framebuffer / panel controller. After supplying timing via
    /// <see cref="SetConfiguration(DisplayControllerSettings)"/> and calling
    /// <see cref="Enable"/>, push pixels with <see cref="DrawBuffer"/> /
    /// <see cref="DrawPixel"/> — or mount the controller into the higher-level
    /// drawing/UI stack via <see cref="Hdc"/>.
    /// </summary>
    public class DisplayController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IDisplayControllerProvider Provider { get; }

        private DisplayController(IDisplayControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default display controller for this device.</summary>
        public static DisplayController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.DisplayController) is DisplayController c ? c : DisplayController.FromName(NativeApi.GetDefaultName(NativeApiType.DisplayController));
        /// <summary>Returns a display controller identified by its native API name.</summary>
        public static DisplayController FromName(string name) => DisplayController.FromProvider(new DisplayControllerApiWrapper(NativeApi.Find(name, NativeApiType.DisplayController)));
        /// <summary>Creates a controller from a custom <see cref="IDisplayControllerProvider"/>.</summary>
        public static DisplayController FromProvider(IDisplayControllerProvider provider) => new DisplayController(provider);

        /// <summary>Native handle (HDC) for use with the drawing/UI stack.</summary>
        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : IntPtr.Zero;

        /// <summary>The settings most recently applied via <see cref="SetConfiguration(DisplayControllerSettings)"/>.</summary>
        public DisplayControllerSettings ActiveConfiguration { get; private set; }

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Powers on the panel and the timing engine.</summary>
        public void Enable() => this.Provider.Enable();
        /// <summary>Powers off the panel.</summary>
        public void Disable() => this.Provider.Disable();

        /// <summary>Blits a rectangular region of an off-screen buffer to the panel.</summary>
        /// <param name="targetX">Destination left edge.</param>
        /// <param name="targetY">Destination top edge.</param>
        /// <param name="sourceX">Source-buffer left edge.</param>
        /// <param name="sourceY">Source-buffer top edge.</param>
        /// <param name="width">Width of the region in pixels.</param>
        /// <param name="height">Height of the region in pixels.</param>
        /// <param name="originalWidth">Width of the full source buffer in pixels.</param>
        /// <param name="data">Pixel data (RGB565 byte pairs).</param>
        /// <param name="offset">Starting offset within <paramref name="data"/>.</param>
        public void DrawBuffer(int targetX, int targetY, int sourceX, int sourceY, int width, int height, int originalWidth, byte[] data, int offset) => this.Provider.DrawBuffer(targetX, targetY, sourceX, sourceY, width, height, originalWidth, data, offset);
        /// <summary>Sets a single pixel.</summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="color">Pixel color in the active <see cref="DisplayDataFormat"/>.</param>
        public void DrawPixel(int x, int y, long color) => this.Provider.DrawPixel(x, y, color);
        /// <summary>Renders a string via the controller's built-in text mode (where supported).</summary>
        public void DrawString(string value) => this.Provider.DrawString(value);

        /// <summary>Applies a display configuration (timing, size, orientation, color format).</summary>
        /// <param name="configuration">A concrete settings instance — pass a <see cref="ParallelDisplayControllerSettings"/> for parallel RGB panels.</param>
        public void SetConfiguration(DisplayControllerSettings configuration) {
            if (this.Provider != null)
                this.Provider.SetConfiguration(configuration);

            this.ActiveConfiguration = configuration;
        }
    }

    /// <summary>Physical bus connecting the SoC to the panel.</summary>
    public enum DisplayInterface {
        /// <summary>Parallel RGB / RGBxxx interface.</summary>
        Parallel = 0,
    }

    /// <summary>Pixel encoding written to the framebuffer.</summary>
    public enum DisplayDataFormat {
        /// <summary>16-bit RGB (5 red, 6 green, 5 blue).</summary>
        Rgb565 = 0,
    }

    /// <summary>Logical screen rotation applied to drawing operations.</summary>
    public enum DisplayOrientation {
        /// <summary>No rotation.</summary>
        Degrees0 = 0,
        /// <summary>Rotated 90° clockwise.</summary>
        Degrees90 = 1,
        /// <summary>Rotated 180°.</summary>
        Degrees180 = 2,
        /// <summary>Rotated 270° clockwise.</summary>
        Degrees270 = 3
    }

    /// <summary>Common settings for any display controller (size, color format, orientation).</summary>
    public class DisplayControllerSettings {
        /// <summary>Panel width in pixels (before rotation).</summary>
        public int Width { get; set; }
        /// <summary>Panel height in pixels (before rotation).</summary>
        public int Height { get; set; }
        /// <summary>Pixel encoding.</summary>
        public DisplayDataFormat DataFormat { get; set; }
        /// <summary>Logical screen rotation.</summary>
        public DisplayOrientation Orientation { get; set; }
    }

    /// <summary>Timing settings for a parallel RGB panel. Values come from the panel's datasheet.</summary>
    public class ParallelDisplayControllerSettings : DisplayControllerSettings {
        /// <summary>True when the data-enable signal is generated by the panel rather than the controller.</summary>
        public bool DataEnableIsFixed { get; set; }
        /// <summary>Polarity of the data-enable signal.</summary>
        public bool DataEnablePolarity { get; set; }
        /// <summary>Polarity of the pixel-clock signal.</summary>
        public bool PixelPolarity { get; set; }
        /// <summary>Pixel-clock rate in Hz.</summary>
        public int PixelClockRate { get; set; }
        /// <summary>Polarity of the horizontal-sync signal.</summary>
        public bool HorizontalSyncPolarity { get; set; }
        /// <summary>Horizontal-sync pulse width in pixel clocks.</summary>
        public int HorizontalSyncPulseWidth { get; set; }
        /// <summary>Pixel clocks between the last active pixel of a line and HSYNC assertion.</summary>
        public int HorizontalFrontPorch { get; set; }
        /// <summary>Pixel clocks between HSYNC deassertion and the first active pixel of the next line.</summary>
        public int HorizontalBackPorch { get; set; }
        /// <summary>Polarity of the vertical-sync signal.</summary>
        public bool VerticalSyncPolarity { get; set; }
        /// <summary>Vertical-sync pulse width in line periods.</summary>
        public int VerticalSyncPulseWidth { get; set; }
        /// <summary>Lines between the last active row of a frame and VSYNC assertion.</summary>
        public int VerticalFrontPorch { get; set; }
        /// <summary>Lines between VSYNC deassertion and the first active row of the next frame.</summary>
        public int VerticalBackPorch { get; set; }
    }

    namespace Provider {
        /// <summary>Provider contract for a display controller.</summary>
        public interface IDisplayControllerProvider : IDisposable {
            /// <summary>Powers on the panel.</summary>
            void Enable();
            /// <summary>Powers off the panel.</summary>
            void Disable();
            /// <summary>Applies a configuration.</summary>
            void SetConfiguration(DisplayControllerSettings configuration);
            /// <summary>Blits a rectangle of pixel data to the panel.</summary>
            void DrawBuffer(int targetX, int targetY, int sourceX, int sourceY, int width, int height, int originalWidth, byte[] data, int offset);
            /// <summary>Sets a single pixel.</summary>
            void DrawPixel(int x, int y, long color);
            /// <summary>Renders text via the controller's built-in text mode.</summary>
            void DrawString(string value);
        }

        /// <summary>Concrete <see cref="IDisplayControllerProvider"/> backed by the native TinyCLR display HAL.</summary>
        public sealed class DisplayControllerApiWrapper : IDisplayControllerProvider, IApiImplementation {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            IntPtr IApiImplementation.Implementation => this.impl;

            /// <summary>Wraps the given native API as a provider.</summary>
            public DisplayControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawBuffer(int targetX, int targetY, int sourceX, int sourceY, int width, int height, int originalWidth, byte[] data, int offset);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawPixel(int x, int y, long color);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawString(string value);

            /// <inheritdoc/>
            public void SetConfiguration(DisplayControllerSettings configuration) {
                if (configuration is ParallelDisplayControllerSettings pcfg) {
                    this.SetConfiguration(pcfg);
                }
                else {
                    throw new ArgumentException("Must pass an instance whose type matches the interface type.");
                }
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetConfiguration(ParallelDisplayControllerSettings settings);
        }
    }
}
