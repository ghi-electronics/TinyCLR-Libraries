using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Media.Mpeg {
    public sealed class Player {

        private byte[][] buffer;

        private FileStream streamToBuffer;
        private byte[] dataToBuffer;

        private bool isDecoding;

        private int fifoIn;
        private int fifoOut;
        private int fifoCount;

        private Graphics screen;

        private int width;
        private int height;
        private int bufferSize;
        private int bufferCount;

        public class Setting {
            public int Width { get; set; }
            public int Height { get; set; }
            public int BufferSize { get; set; } = 100 * 1024;
            public int BufferCount { get; set; } = 3;
        }

        public Player(Setting setting) {
            this.width = setting.Width;
            this.height = setting.Height;
            this.bufferSize = setting.BufferSize;
            this.bufferCount = setting.BufferCount;

            this.buffer = new byte[this.bufferCount][];

            for (var c = 0; c < this.bufferCount; c++) {
                this.buffer[c] = new byte[this.bufferSize];
            }

            this.screen = Graphics.FromImage(new Bitmap(this.width, this.height));
        }

        private void PushStreamToBuffer() {
            var streamLength = this.streamToBuffer.Length;
            var i = 0;

            while (i < streamLength) {

                Thread.Sleep(1);

                if (!this.isDecoding)
                    break;

                if (this.fifoCount == this.bufferCount) {
                    continue;
                }

                var lengthToBuffer = (int)((this.bufferSize < streamLength - i) ? this.bufferSize : (streamLength - i));

                const int BLOCK_SIZE = 10 * 1024;

                var block = lengthToBuffer / BLOCK_SIZE;
                var remain = lengthToBuffer % BLOCK_SIZE;

                var index = 0;
                var id = this.fifoIn;

                while (block > 0) {
                    this.streamToBuffer.Read(this.buffer[id], index, BLOCK_SIZE);
                    index += BLOCK_SIZE;
                    block--;

                    i += BLOCK_SIZE;
                }

                if (remain > 0) {
                    this.streamToBuffer.Read(this.buffer[id], index, remain);

                    i += remain;
                }

                this.fifoCount++;
                this.fifoIn++;

                if (this.fifoIn == this.bufferCount) {
                    this.fifoIn = 0;
                }
            }
        }
        private void PushDataToBuffer() {
            //TODO
        }

        public void StartDecode(FileStream stream) {
            this.streamToBuffer = stream;

            this.isDecoding = true;

            var pushThread = new Thread(this.PushStreamToBuffer);

            pushThread.Start();

            var pop2Device = new Thread(this.Pop2Device);

            pop2Device.Start();
        }

        public void StartDecode(byte[] data) {
            this.dataToBuffer = data;

            this.isDecoding = true;

            var pushThread = new Thread(this.PushDataToBuffer);

            pushThread.Start();

            var pop2Device = new Thread(this.Pop2Device);

            pop2Device.Start();
        }

        public void StopDecode() => this.isDecoding = false;

        private void Pop2Device() {

            while (this.isDecoding) {
                Thread.Sleep(1);

                if (this.fifoCount == 0) {
                    continue;
                }

                var id = this.fifoOut;

                var startFrame = 0;

                for (var i = 0; i < this.bufferSize - 4; i++) {

                    var lentmp = 0;
                    var foundFrame = false;

                    if (this.buffer[id][i] == (byte)'0'
                    && this.buffer[id][i + 1] == (byte)'0'
                    && this.buffer[id][i + 2] == (byte)'d'
                    && this.buffer[id][i + 3] == (byte)'c') {
                        i += 4;

                        lentmp = (this.buffer[id][i] | (this.buffer[id][i + 1] << 8) | (this.buffer[id][i + 2] << 16) | (this.buffer[id][i + 3] << 24));

                        i += 4;

                        startFrame = i;

                        if (i + lentmp > this.buffer[id].Length)
                            break;

                        i += (lentmp - 1);

                        foundFrame = lentmp > 0;
                    }

                    if (foundFrame) {
                        var frameData = new byte[lentmp];

                        Array.Copy(this.buffer[id], startFrame, frameData, 0, frameData.Length);

                        var image = new Bitmap(frameData, BitmapImageType.Jpeg);

                        this.screen.DrawImage(image, 0, 0, image.Width, image.Height);

                        OnFrameReceived?.Invoke(this.screen.GetBitmap());

                        image.Dispose();
                    }
                }

                this.fifoCount--;
                this.fifoOut++;

                if (this.fifoOut == this.bufferCount)
                    this.fifoOut = 0;
            }
        }

        public delegate void FrameReceivedHandler(byte[] data);
        public event FrameReceivedHandler OnFrameReceived;

    }
}
