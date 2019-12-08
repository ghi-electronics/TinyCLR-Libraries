using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Media {
    public sealed class Mpeg {
        const int BLOCK_SIZE = 4 * 1024;

        private FileStream streamToBuffer;
        private byte[] dataToBuffer;

        private bool isDecoding;

        private Graphics screen;

        public class Setting {
            public int BufferSize { get; set; } = 2 * 1024 * 1024;
            public int BufferCount { get; set; } = 3;
            public Graphics Screen { get; set; }
        }

        private class Fifo {
            internal int bufferSize;
            internal int bufferCount;

            internal int fifoIn;
            internal int fifoOut;
            internal int fifoCount;

            internal byte[][] buffer;
            internal IntPtr[] unmanagedPtr;
        }

        private Fifo fifo;

        public Mpeg(Setting setting) {
            this.fifo = new Fifo {
                bufferSize = setting.BufferSize,
                bufferCount = setting.BufferCount
            };

            this.fifo.buffer = new byte[this.fifo.bufferCount][];
            this.fifo.unmanagedPtr = new IntPtr[this.fifo.bufferCount];

            for (var c = 0; c < this.fifo.bufferCount; c++) {

                if (DeviceInformation.DeviceName == "Sc20260") {
                    var ptr = Memory.UnmanagedMemory.Allocate(this.fifo.bufferSize);
                    this.fifo.buffer[c] = Memory.UnmanagedMemory.ToBytes(ptr, this.fifo.bufferSize);

                    this.fifo.unmanagedPtr[c] = ptr;
                }
                else {
                    this.fifo.buffer[c] = new byte[this.fifo.bufferSize];
                }
            }

            this.screen = setting.Screen;

            if (this.screen == null)
                throw new NullReferenceException("");
        }

        ~Mpeg() {
            this.isDecoding = false;

            if (this.fifo.unmanagedPtr != null) {
                for (var c = 0; c < this.fifo.bufferCount; c++) {
                    Memory.UnmanagedMemory.Free(this.fifo.unmanagedPtr[c]);
                }
            }
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

        private void PushStreamToBuffer() {
            var streamLength = this.streamToBuffer.Length;
            var i = 0;

            while (i < streamLength) {

                Thread.Sleep(1);

                if (!this.isDecoding)
                    break;

                if (this.fifo.fifoCount == this.fifo.bufferCount) {
                    continue;
                }

                var lengthToBuffer = (int)((this.fifo.bufferSize < streamLength - i) ? this.fifo.bufferSize : (streamLength - i));

                var block = lengthToBuffer / BLOCK_SIZE;
                var remain = lengthToBuffer % BLOCK_SIZE;

                var index = 0;
                var id = this.fifo.fifoIn;

                while (block > 0) {
                    this.streamToBuffer.Read(this.fifo.buffer[id], index, BLOCK_SIZE);
                    index += BLOCK_SIZE;
                    block--;

                    i += BLOCK_SIZE;

                    Thread.Sleep(1);
                }

                if (remain > 0) {
                    this.streamToBuffer.Read(this.fifo.buffer[id], index, remain);

                    i += remain;
                }

                lock (this.fifo) {
                    this.fifo.fifoCount++;
                }

                this.fifo.fifoIn++;

                if (this.fifo.fifoIn == this.fifo.bufferCount) {
                    this.fifo.fifoIn = 0;
                }
            }
        }
        private void PushDataToBuffer() {
            var dataLength = this.dataToBuffer.Length;
            var i = 0;

            while (i < dataLength) {

                Thread.Sleep(1);

                if (!this.isDecoding)
                    break;

                if (this.fifo.fifoCount == this.fifo.bufferCount) {
                    continue;
                }

                var lengthToBuffer = (int)((this.fifo.bufferSize < dataLength - i) ? this.fifo.bufferSize : (dataLength - i));

                var block = lengthToBuffer / BLOCK_SIZE;
                var remain = lengthToBuffer % BLOCK_SIZE;

                var index = 0;
                var id = this.fifo.fifoIn;

                while (block > 0) {
                    Array.Copy(this.dataToBuffer, i, this.fifo.buffer[id], index, BLOCK_SIZE);

                    index += BLOCK_SIZE;
                    i += BLOCK_SIZE;

                    block--;
                }

                if (remain > 0) {
                    Array.Copy(this.dataToBuffer, i, this.fifo.buffer[id], index, remain);

                    i += remain;
                }

                lock (this.fifo) {
                    this.fifo.fifoCount++;
                }

                this.fifo.fifoIn++;

                if (this.fifo.fifoIn == this.fifo.bufferCount) {
                    this.fifo.fifoIn = 0;
                }
            }
        }

        private void Pop2Device() {

            while (this.isDecoding) {
                Thread.Sleep(1);

                if (this.fifo.fifoCount == 0) {
                    continue;
                }

                var id = this.fifo.fifoOut;

                var startFrame = 0;

                for (var i = 0; i < this.fifo.bufferSize - 4; i++) {

                    var jpegLength = 0;
                    var foundFrame = false;

                    if (this.fifo.buffer[id][i] == (byte)'0'
                    && this.fifo.buffer[id][i + 1] == (byte)'0'
                    && this.fifo.buffer[id][i + 2] == (byte)'d'
                    && this.fifo.buffer[id][i + 3] == (byte)'c') {
                        i += 4;

                        jpegLength = (this.fifo.buffer[id][i] | (this.fifo.buffer[id][i + 1] << 8) | (this.fifo.buffer[id][i + 2] << 16) | (this.fifo.buffer[id][i + 3] << 24));

                        i += 4;

                        startFrame = i;

                        if (i + jpegLength > this.fifo.buffer[id].Length)
                            break;

                        i += (jpegLength - 1);

                        foundFrame = jpegLength > 0;
                    }

                    if (foundFrame) {

                        var t1 = System.DateTime.Now.Ticks;

                        var frameData = new byte[jpegLength];

                        Array.Copy(this.fifo.buffer[id], startFrame, frameData, 0, frameData.Length);

                        using (var image = new Bitmap(frameData, BitmapImageType.Jpeg)) {

                            this.screen.DrawImage(image, 0, 0, image.Width, image.Height);

                            this.screen.Flush();
                        }

                        GC.WaitForPendingFinalizers();

                        var t2 = (int)(System.DateTime.Now.Ticks - t1) / 10000;

                        if (t2 < 50)
                            Thread.Sleep(50 - t2);

                    }
                }

                lock (this.fifo) {
                    this.fifo.fifoCount--;
                }

                this.fifo.fifoOut++;

                if (this.fifo.fifoOut == this.fifo.bufferCount)
                    this.fifo.fifoOut = 0;
            }
        }
    }
}
