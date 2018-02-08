using System;
using System.Collections;
using System.Text;
using GHIElectronics.TinyCLR.Networking.SPWF04Sx.Helpers;

namespace GHIElectronics.TinyCLR.Networking.SPWF04Sx {
    public sealed class SPWF04SxCommand {
        private readonly string[] parameters = new string[16];
        private readonly Queue pendingReads = new Queue();
        private readonly Semaphore pendingReadsSemaphore = new Semaphore();
        private readonly GrowableBuffer writeHeader = new GrowableBuffer(4, 512);
        private ReadWriteBuffer readPayload;
        private int parameterCount;
        private int partialRead;
        private int writeHeaderLength;
        private byte[] writePayload;
        private int writePayloadOffset;
        private int writePayloadLength;

        internal delegate void DataReaderWriter(byte[] buffer, int offset, int count);

        internal SPWF04SxCommand() { }

        internal bool Sent { get; set; }
        internal bool HasWritePayload => this.writePayloadLength > 0;

        public SPWF04SxCommand AddParameter(string parameter) {
            this.parameters[this.parameterCount++] = parameter;

            return this;
        }

        public SPWF04SxCommand Finalize(SPWF04SxCommandIds cmdId) => this.Finalize(cmdId, null, 0, 0);

        public SPWF04SxCommand Finalize(SPWF04SxCommandIds cmdId, byte[] rawData, int rawDataOffset, int rawDataCount) {
            if (rawData == null && rawDataCount != 0) throw new ArgumentException();
            if (rawDataOffset < 0) throw new ArgumentOutOfRangeException();
            if (rawDataCount < 0) throw new ArgumentOutOfRangeException();
            if (rawData != null && rawDataOffset + rawDataCount > rawData.Length) throw new ArgumentOutOfRangeException();

            var required = 4 + this.parameterCount;

            for (var i = 0; i < this.parameterCount; i++) {
                var p = this.parameters[i];

                required += p != null ? p.Length : 0;
            }

            this.writeHeader.EnsureSize(required, false);

            var idx = 0;
            var buf = this.writeHeader.Data;

            buf[idx++] = 0x00;
            buf[idx++] = 0x00;

            buf[idx++] = (byte)cmdId;
            buf[idx++] = (byte)this.parameterCount;

            for (var i = 0; i < this.parameterCount; i++) {
                var p = this.parameters[i];
                var pLen = p != null ? p.Length : 0;

                buf[idx++] = (byte)pLen;

                if (!string.IsNullOrEmpty(p))
                    Encoding.UTF8.GetBytes(p, 0, pLen, buf, idx);

                idx += pLen;
            }

            var len = idx + rawDataCount - 2;
            buf[0] = (byte)((len >> 8) & 0xFF);
            buf[1] = (byte)((len >> 0) & 0xFF);

            this.writePayload = rawData;
            this.writePayloadOffset = rawDataOffset;
            this.writePayloadLength = rawDataCount;
            this.writeHeaderLength = idx;

            return this;
        }

        public string ReadString() {
            this.pendingReadsSemaphore.WaitOne();

            lock (this.pendingReads) {
                var start = this.readPayload.ReadOffset;
                var len = (int)this.pendingReads.Dequeue();
                var res = Encoding.UTF8.GetString(this.readPayload.Data, start, len);

                this.readPayload.Read(len);

                return res;
            }
        }

        public int ReadBuffer() => this.ReadBuffer(null, 0, 0);

        public int ReadBuffer(byte[] buffer, int offset, int count) {
            this.pendingReadsSemaphore.WaitOne();

            lock (this.pendingReads) {
                var len = 0;

                if (buffer != null) {
                    len = (int)this.pendingReads.Peek() - this.partialRead;

                    if (len <= count) {
                        Array.Copy(this.readPayload.Data, this.readPayload.ReadOffset, buffer, offset, len);

                        this.partialRead = 0;

                        this.pendingReads.Dequeue();
                    }
                    else {
                        len = count;

                        Array.Copy(this.readPayload.Data, this.readPayload.ReadOffset, buffer, offset, count);

                        this.partialRead += count;
                    }
                }
                else {
                    len = (int)this.pendingReads.Dequeue();
                }

                this.readPayload.Read(len);

                return len;
            }
        }

        internal void ReadPayload(DataReaderWriter reader, int count) {
            var remaining = count;

            while (remaining > 0) {
                this.readPayload.WaitForWriteSpace(remaining);

                var actual = Math.Min(remaining, this.readPayload.AvailableWrite);

                reader(this.readPayload.Data, this.readPayload.WriteOffset, actual);

                this.readPayload.Write(actual);

                remaining -= actual;
            }

            lock (this.pendingReads) {
                this.pendingReads.Enqueue(count);
                this.pendingReadsSemaphore.Release();
            }
        }

        internal void WriteHeader(DataReaderWriter reader) => reader(this.writeHeader.Data, 0, this.writeHeaderLength);
        internal void WritePayload(DataReaderWriter reader) => reader(this.writePayload, this.writePayloadOffset, this.writePayloadLength);

        internal void Reset() {
            lock (this.pendingReads)
                if (!this.Sent || this.pendingReads.Count != 0)
                    throw new Exception("Not complete");

            this.Sent = false;
            this.parameterCount = 0;
            this.writeHeaderLength = 0;
            this.writePayloadOffset = 0;
            this.writePayloadLength = 0;

            this.readPayload.Reset();
            this.readPayload = null;
        }

        internal void SetPayloadBuffer(ReadWriteBuffer buffer) => this.readPayload = buffer;
    }
}
