using System;
using System.Collections;
using System.Threading;

namespace GHIElectronics.TinyCLR.Networking.SPWF04Sx.Helpers {
    internal delegate object PoolObjectCreator();

    internal class ObjectPool {
        private readonly ArrayList all = new ArrayList();
        private readonly Stack available = new Stack();
        private readonly PoolObjectCreator creator;

        public ObjectPool(PoolObjectCreator creator) => this.creator = creator;

        public object Acquire() {
            lock (this.available) {
                if (this.available.Count == 0) {
                    var obj = this.creator();

                    this.all.Add(obj);

                    return obj;
                }

                return this.available.Pop();
            }
        }

        public void Release(object obj) {
            lock (this.available) {
                if (!this.all.Contains(obj)) throw new ArgumentException();

                this.available.Push(obj);
            }
        }

        public void ResetAll() {
            lock (this.available) {
                this.available.Clear();

                foreach (var obj in this.all)
                    this.available.Push(obj);
            }
        }
    }

    internal class GrowableBuffer {
        private byte[] buffer;

        public byte[] Data => this.buffer;
        public int CurrentSize => this.buffer.Length;
        public int RemainingSize => this.MaxSize - this.CurrentSize;
        public int MaxSize { get; }

        public GrowableBuffer(int startSize) : this(startSize, int.MaxValue) { }

        public GrowableBuffer(int startSize, int maxSize) {
            this.MaxSize = maxSize;

            if (!this.TryGrow(false, startSize))
                throw new ArgumentException();
        }

        public void EnsureSize(int size, bool copy) {
            if (size > this.CurrentSize && !this.TryGrow(copy, size + Math.Min(this.RemainingSize, 100)))
                throw new Exception("Buffer size exceeded max.");
        }

        public bool TryGrow(bool copy) => this.RemainingSize > 0 && this.TryGrow(copy, this.CurrentSize + Math.Min(this.RemainingSize, 100));

        public bool TryGrow(bool copy, int size) {
            if (size >= this.MaxSize)
                return false;

            var newBuffer = new byte[size];

            if (copy)
                Array.Copy(this.buffer, newBuffer, this.CurrentSize);

            this.buffer = newBuffer;

            return true;
        }
    }

    internal class ReadWriteBuffer {
        private readonly object lck = new object();
        private readonly ManualResetEvent writeWaiter = new ManualResetEvent(false);
        private readonly GrowableBuffer buffer;
        private int nextRead;
        private int nextWrite;

        public byte[] Data => this.buffer.Data;

        public int AvailableWrite { get { lock (this.lck) return this.buffer.CurrentSize - this.nextWrite; } }
        public int AvailableRead { get { lock (this.lck) return this.nextWrite - this.nextRead; } }

        public int WriteOffset => this.nextWrite;
        public int ReadOffset => this.nextRead;

        public ReadWriteBuffer(int size, int maxSize) => this.buffer = new GrowableBuffer(size, maxSize);

        public void WaitForWriteSpace(int desired) {
            while (true) {
                lock (this.lck) {
                    if (this.AvailableWrite < desired)
                        this.buffer.TryGrow(this.nextRead != 0 || this.nextWrite != 0, desired + this.nextWrite);

                    if (this.AvailableWrite != 0) {
                        this.writeWaiter.Reset();

                        return;
                    }

                    if (this.nextRead != 0) {
                        Array.Copy(this.buffer.Data, this.nextRead, this.buffer.Data, 0, this.nextWrite - this.nextRead);

                        this.nextWrite -= this.nextRead;
                        this.nextRead = 0;

                        continue;
                    }
                }

                this.writeWaiter.WaitOne();
            }
        }

        public void Write(int count) {
            lock (this.lck)
                this.nextWrite += count;
        }

        public void Read(int count) {
            lock (this.lck) {
                this.nextRead += count;
                this.writeWaiter.Set();
            }
        }

        public void Reset() {
            lock (this.lck) {
                this.writeWaiter.Reset();
                this.nextWrite = 0;
                this.nextRead = 0;
            }
        }
    }

    internal class Semaphore : WaitHandle {
        private readonly object lck = new object();
        private readonly ManualResetEvent evt = new ManualResetEvent(false);
        private int count;

        public override bool WaitOne() {
            while (true) {
                this.evt.WaitOne();

                lock (this.lck) {
                    if (this.count > 0) {
                        if (--this.count == 0)
                            this.evt.Reset();

                        return true;
                    }
                }
            }
        }

        public int Release() {
            lock (this.lck) {
                var cnt = this.count;

                this.count++;

                this.evt.Set();

                return cnt;
            }
        }
    }
}
