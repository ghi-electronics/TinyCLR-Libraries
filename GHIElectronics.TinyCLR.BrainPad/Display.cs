/*

The display code is part GHI Electronics Part from
https://github.com/adafruit/Adafruit_SSD1306/blob/master/Adafruit_SSD1306.cpp
with thie following license...

Software License Agreement (BSD License)
Copyright (c) 2012, Adafruit Industries
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.
3. Neither the name of the copyright holders nor the
names of its contributors may be used to endorse or promote products
derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using GHIElectronics.TinyCLR.Devices.I2c;

namespace GHIElectronics.TinyCLR.BrainPad.Internal {
    public class Display {
        internal I2cDevice i2cDevice = I2cDevice.FromId(I2cDevice.GetDeviceSelector("I2C1"), new I2cConnectionSettings(0x3C) { BusSpeed = I2cBusSpeed.FastMode });
        internal ImageBuffer imageBuffer;

        private void Ssd1306_command(int cmd) {

            var buff = new byte[2];
            buff[1] = (byte)cmd;
            this.i2cDevice.Write(buff);


        }

        public Display() {
            this.imageBuffer = new ImageBuffer(this);


            // Init sequence
            Ssd1306_command(0xae);// SSD1306_DISPLAYOFF);                    // 0xAE
            Ssd1306_command(0xd5);// SSD1306_SETDISPLAYCLOCKDIV);            // 0xD5
            Ssd1306_command(0x80);                                  // the suggested ratio 0x80

            Ssd1306_command(0xa8);// SSD1306_SETMULTIPLEX);                  // 0xA8
            Ssd1306_command(64 - 1);

            Ssd1306_command(0xd3);// SSD1306_SETDISPLAYOFFSET);              // 0xD3
            Ssd1306_command(0x0);                                   // no offset
            Ssd1306_command(0x40);// SSD1306_SETSTARTLINE | 0x0);            // line #0
            Ssd1306_command(0x8d);// SSD1306_CHARGEPUMP);                    // 0x8D

            //if (false)//vccstate == SSD1306_EXTERNALVCC)
            //
            //{ Ssd1306_command(0x10); }
            //else {
            Ssd1306_command(0x14);
            //}

            Ssd1306_command(0x20);// SSD1306_MEMORYMODE);                    // 0x20
            Ssd1306_command(0x00);                                  // 0x0 act like ks0108
            Ssd1306_command(0xa1);// SSD1306_SEGREMAP | 0x1);
            Ssd1306_command(0xc8);// SSD1306_COMSCANDEC);


            Ssd1306_command(0xda);// SSD1306_SETCOMPINS);                    // 0xDA
            Ssd1306_command(0x12);
            Ssd1306_command(0x81);// SSD1306_SETCONTRAST);                   // 0x81

            //if (false)//vccstate == SSD1306_EXTERNALVCC)
            //{ Ssd1306_command(0x9F); }
            //else {
            Ssd1306_command(0xCF);
            //}

            Ssd1306_command(0xd9);// SSD1306_SETPRECHARGE);                  // 0xd9

            //if (false)//vccstate == SSD1306_EXTERNALVCC)
            //{ Ssd1306_command(0x22); }
            //else {
            Ssd1306_command(0xF1);
            //}

            Ssd1306_command(0xd8);// SSD1306_SETVCOMDETECT);                 // 0xDB
            Ssd1306_command(0x40);
            Ssd1306_command(0xa4);//SSD1306_DISPLAYALLON_RESUME);           // 0xA4
            Ssd1306_command(0xa6);// SSD1306_NORMALDISPLAY);                 // 0xA6

            Ssd1306_command(0x2e);// SSD1306_DEACTIVATE_SCROLL);

            Ssd1306_command(0xaf);// SSD1306_DISPLAYON);//--turn on oled panel


            Ssd1306_command(0x21);// SSD1306_COLUMNADDR);
            Ssd1306_command(0);   // Column start address (0 = reset)
            Ssd1306_command(128 - 1); // Column end address (127 = reset)
            Ssd1306_command(0x22);// SSD1306_PAGEADDR);
            Ssd1306_command(0); // Page start address (0 = reset)
            Ssd1306_command(7); // Page end address


            this.imageBuffer.Clear();
            this.imageBuffer.ShowOnScreen();
        }


        /// <summary>
        /// Draws text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="number">The number to draw.</param>
        public void DrawText(int x, int y, string text) {
            this.imageBuffer.Clear();
            this.imageBuffer.DrawText(x, y, text, 2, 2);
            this.imageBuffer.ShowOnScreen();
        }
        public void DrawSmallText(int x, int y, string text) {
            this.imageBuffer.Clear();
            this.imageBuffer.DrawText(x, y, text, 1, 1);
            this.imageBuffer.ShowOnScreen();
        }
        public void DrawText(int x, int y, double number) => DrawText(x, y, number.ToString("N2"));
        public void DrawSmallText(int x, int y, double number) => DrawSmallText(x, y, number.ToString("N2"));
        public void InvertColors(bool invert) {
            if (invert)
                Ssd1306_command(0xa7);
            else
                Ssd1306_command(0xa6);
        }
    }
}
