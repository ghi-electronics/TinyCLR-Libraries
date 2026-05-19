using System.Drawing;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI {
    // Hand-authored partial extending the auto-generated Resources.Designer.cs.
    //
    // Ownership policy for built-in bitmap resources
    // ----------------------------------------------
    //
    // 1. `Resources.GetBitmap(id)` returns a FRESH `System.Drawing.Bitmap`
    //    instance on every call. The ResourceManager constructs a new managed
    //    wrapper around the native resource data each time. Controls do NOT
    //    share their wrapper objects — two callers asking for `Button_Up`
    //    receive two independent Bitmaps backed by the same native pixel data.
    //
    // 2. `Graphics.FromImage(bmp)` returns `bmp.data` — the bitmap's *internal*
    //    Graphics. It does not allocate a new Graphics. Disposing the returned
    //    Graphics tears down the bitmap's draw surface and effectively destroys
    //    the bitmap for further use.
    //
    // 3. `BitmapImage.FromGraphics(g)` wraps `g` as an `ImageSource` for use by
    //    DrawingContext. The wrapper does not take a second reference — when
    //    `g` is disposed, the BitmapImage is dead.
    //
    // The standard control disposal pattern is therefore:
    //
    //   protected virtual void Dispose(bool disposing) {
    //       if (disposed) return;
    //       if (disposing) {
    //           this._bitmapImage?.graphics?.Dispose();   // releases everything
    //       }
    //       disposed = true;
    //   }
    //
    // The `disposing` flag is honored so the finalizer path doesn't reach
    // managed objects that may have already been finalized.
    //
    // Centralized loader
    // ------------------
    //
    // `LoadBitmapImage(id)` collapses the standard
    //   `BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(id)))`
    // chain into one call. It was duplicated 13 times across Button, CheckBox,
    // RadioButton, ProgressBar, Dropdown, and Slider. Use this helper instead.

    internal partial class Resources {
        /// <summary>
        /// Loads a built-in bitmap resource and wraps it as a ready-to-draw
        /// <see cref="BitmapImage"/>. Each call returns an independent owning
        /// instance — the caller is responsible for disposing the inner
        /// Graphics in its own <c>Dispose(true)</c> path.
        /// </summary>
        internal static BitmapImage LoadBitmapImage(BitmapResources id) =>
            BitmapImage.FromGraphics(Graphics.FromImage(GetBitmap(id)));
    }
}
