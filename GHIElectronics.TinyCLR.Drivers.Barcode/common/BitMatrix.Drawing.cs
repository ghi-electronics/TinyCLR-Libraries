/*
* Copyright 2007 ZXing authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Drawing;

//using Microsoft.SPOT;
//using Microsoft.SPOT.Presentation.Media;

namespace GHIElectronics.TinyCLR.Drivers.Barcode.Common
{
    public sealed partial class BitMatrix
    {
        public Bitmap ToBitmap()
        {
            return ToBitmap(BarcodeFormat.EAN_8, null);
        }

        /// <summary>
        /// Converts this ByteMatrix to a black and white bitmap.
        /// </summary>
        /// <returns>A black and white bitmap converted from this ByteMatrix.</returns>
        public Bitmap ToBitmap(BarcodeFormat format, String content)
        {
            int width = Width;
            int height = Height;
            bool outputContent = !(content == null || content.Length == 0) && (format == BarcodeFormat.CODE_39 ||
                                                                    format == BarcodeFormat.CODE_128 ||
                                                                    format == BarcodeFormat.EAN_13 ||
                                                                    format == BarcodeFormat.EAN_8 ||
                                                                    format == BarcodeFormat.CODABAR ||
                                                                    format == BarcodeFormat.ITF ||
                                                                    format == BarcodeFormat.UPC_A);
            int emptyArea = outputContent ? 16 : 0;

            // create the bitmap and lock the bits because we need the stride
            // which is the width of the image and possible padding bytes
            var bmp = new Bitmap(width, height);
            for (int y = 0; y < height - emptyArea; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var color = this[x, y] ? Color.Black : Color.White;
                    bmp.SetPixel(x, y, color);
                }
            }

            if (outputContent)
            {
                //switch (format)
                //{
                //   case BarcodeFormat.EAN_8:
                //      if (content.Length < 8)
                //         content = OneDimensionalCodeWriter.CalculateChecksumDigitModulo10(content);
                //      content = content.Insert(4, "   ");
                //      break;
                //   case BarcodeFormat.EAN_13:
                //      if (content.Length < 13)
                //         content = OneDimensionalCodeWriter.CalculateChecksumDigitModulo10(content);
                //      content = content.Insert(7, "   ");
                //      content = content.Insert(1, "   ");
                //      break;
                //}
                //bmp.DrawText(content, null, Color.Black, width / 2, height - 14);
                throw new NotImplementedException();
            }

            return bmp;
        }
    }
}