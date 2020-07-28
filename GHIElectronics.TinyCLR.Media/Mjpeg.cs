using System;
using System.Collections;
using System.IO;
using System.Threading;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Media {
    public sealed class Mjpeg : IDisposable {
        const int BLOCK_SIZE = 4 * 1024;

        public delegate void DataDecodedEventHandler(byte[] data);
        public event DataDecodedEventHandler FrameReceived;

        //public event DataDecodedEventHandler Mp3DataDecodedEvent;

        private Stream stream;
        public bool IsDecoding { get; internal set; }
        private int delayBetweenFrames;
        private Queue queue;
        private Setting setting;

        public class Setting {
            public int BufferSize { get; set; } = 2 * 1024 * 1024;
            public int BufferCount { get; set; } = 3;
            public TimeSpan DelayBetweenFramesMilliseconds { get; set; } = TimeSpan.FromMilliseconds(1000 / 16);
        }

        private byte[][] buffer;
        private UnmanagedBuffer[] unmanagedBuffer;
        private uint currentBufferIdx;

        private class HeaderInfo {
            public int TimeBetweenFrames { get; internal set; }
            public int Width { get; internal set; }
            public int Height { get; internal set; }
            public int SuggestedBufferSize { get; internal set; }
            public int TotalFrames { get; internal set; }

        }

        private HeaderInfo headerInfo;

        public Mjpeg(Setting setting) {
            this.currentBufferIdx = 0;
            this.queue = new Queue();
            this.setting = setting;
            this.headerInfo = new HeaderInfo();
            this.delayBetweenFrames = (int)setting.DelayBetweenFramesMilliseconds.TotalMilliseconds;

            this.unmanagedBuffer = new UnmanagedBuffer[this.setting.BufferCount];
            this.buffer = new byte[this.setting.BufferCount][];

            for (var c = 0; c < this.setting.BufferCount; c++) {

                if (Memory.UnmanagedMemory.FreeBytes > this.setting.BufferSize) {
                    this.unmanagedBuffer[c] = new UnmanagedBuffer(this.setting.BufferSize);

                    this.buffer[c] = this.unmanagedBuffer[c].Bytes;

                }
                else {
                    this.buffer[c] = new byte[this.setting.BufferSize];
                }
            }
        }

        public void StartDecode(Stream stream) {
            this.stream = stream ?? throw new ArgumentNullException();

            this.IsDecoding = true;

            var pushThread = new Thread(this.Buffering);

            pushThread.Start();

            var pop2Device = new Thread(this.Decoding);

            pop2Device.Start();
        }

        public void StopDecode() => this.IsDecoding = false;

        private void Buffering() {
            var streamLength = this.stream.Length;
            var i = 0;

            while (i < streamLength) {
                Thread.Sleep(1);

                if (!this.IsDecoding)
                    break;

                lock (this.queue) {
                    if (this.queue.Count == this.setting.BufferCount)
                        continue;
                }

                var lengthToBuffer = (int)((this.setting.BufferSize < streamLength - i) ? this.setting.BufferSize : (streamLength - i));

                var block = lengthToBuffer / BLOCK_SIZE;
                var remain = lengthToBuffer % BLOCK_SIZE;

                var index = 0;

                while (block > 0) {
                    this.stream.Read(this.buffer[this.currentBufferIdx], index, BLOCK_SIZE);
                    index += BLOCK_SIZE;
                    block--;

                    i += BLOCK_SIZE;

                    Thread.Sleep(1);
                }

                if (remain > 0) {
                    this.stream.Read(this.buffer[this.currentBufferIdx], index, remain);

                    i += remain;
                }

                lock (this.queue) {
                    this.queue.Enqueue(this.buffer[this.currentBufferIdx]);
                }

                this.currentBufferIdx++;

                if (this.currentBufferIdx == this.setting.BufferCount) {
                    this.currentBufferIdx = 0;
                }
            }
        }

        private void Decoding() {
            var decodeHeader = true;

            while (this.IsDecoding) {
                Thread.Sleep(1);

                lock (this.queue) {
                    if (this.queue.Count == 0)
                        continue;
                }

                byte[] data;

                lock (this.queue) {
                    data = (byte[])this.queue.Dequeue();
                }

                for (var i = 0; i < this.setting.BufferSize - 4; i++) {
                    // Decode header
                    var t1 = System.DateTime.Now;
                    var foundData = false;

                    if (decodeHeader) {
                        var riff = System.Text.Encoding.UTF8.GetString(data, 0, 4);
                        var type = System.Text.Encoding.UTF8.GetString(data, 8, 4);

                        if (riff.CompareTo("RIFF") == 0 || type.CompareTo("AVI ") == 0) {

                            i = 0x20; // fps

                            this.headerInfo.TimeBetweenFrames = (data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24)) / 1000;

                            i += 4 * 4;

                            this.headerInfo.TotalFrames = data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24);

                            i += 3 * 4;

                            this.headerInfo.SuggestedBufferSize = data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24);

                            i += 4;

                            this.headerInfo.Width = data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24);

                            i += 4;

                            this.headerInfo.Height = data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24);

                            i += 4;
                        }

                        decodeHeader = false;
                    }

                    // Decode image
                    {
                        var jpegLength = 0;

                        if (data[i] == (byte)'0'
                        && data[i + 1] == (byte)'0'
                        && data[i + 2] == (byte)'d'
                        && data[i + 3] == (byte)'c') {
                            i += 4;

                            jpegLength = (data[i] | (data[i + 1] << 8) | (data[i + 2] << 16) | (data[i + 3] << 24));

                            i += 4;

                            if (i + jpegLength > data.Length)
                                break;

                            if (jpegLength > 0) {

                                var dataJpeg = new byte[jpegLength];

                                Array.Copy(data, i, dataJpeg, 0, jpegLength);

                                FrameReceived?.Invoke(dataJpeg);

                                i += (jpegLength - 1);

                                foundData = true;

                            }
                        }
                    }


                    if (foundData) {
                        var now = (System.DateTime.Now - t1);

                        if ((int)now.TotalMilliseconds < this.delayBetweenFrames)
                            Thread.Sleep(this.delayBetweenFrames - (int)now.TotalMilliseconds);

                    }
                }
            }
        }

        public void Dispose() {
            this.IsDecoding = false;
            this.queue.Clear();

            if (this.unmanagedBuffer != null) {
                for (var c = 0; c < this.setting.BufferCount; c++) {
                    this.unmanagedBuffer[c].Dispose();
                }
            }
        }
    }
}
