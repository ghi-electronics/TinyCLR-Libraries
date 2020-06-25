////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Xml
{
  internal class BufferBuilder
  {
    private const int BufferSize = 16384;
    private const int InitialBufferArrayLength = 4;
    private const int MaxStringBuilderLength = 16384;
    private const int DefaultSBCapacity = 16;
    private BufferBuilder.Buffer[] buffers;
    private int buffersCount;
    private char[] lastBuffer;
    private int lastBufferIndex;
    private int length;

    public int Length
    {
      get
      {
        return this.length;
      }
      set
      {
        if (value < 0 || value > this.length)
          throw new ArgumentOutOfRangeException(nameof (value));
        if (value == 0)
          this.Clear();
        else
          this.SetLength(value);
      }
    }

    public void Append(char value)
    {
      if (this.lastBuffer == null)
        this.CreateBuffers();
      if (this.lastBufferIndex == this.lastBuffer.Length)
        this.AddBuffer();
      this.lastBuffer[this.lastBufferIndex++] = value;
      ++this.length;
    }

    public void Append(char[] value)
    {
      this.Append(value, 0, value.Length);
    }

    public void Append(char[] value, int start, int count)
    {
      if (value == null)
      {
        if (start != 0 || count != 0)
          throw new ArgumentNullException(nameof (value));
      }
      else
      {
        if (count == 0)
          return;
        if (start < 0)
          throw new ArgumentOutOfRangeException(nameof (start));
        if (count < 0 || start + count > value.Length)
          throw new ArgumentOutOfRangeException(nameof (count));
        if (this.lastBuffer == null)
          this.CreateBuffers();
        int sourceIndex = start;
        while (count > 0)
        {
          if (this.lastBufferIndex >= this.lastBuffer.Length)
            this.AddBuffer();
          int length = count;
          int num = this.lastBuffer.Length - this.lastBufferIndex;
          if (num < length)
            length = num;
          Array.Copy((Array) value, sourceIndex, (Array) this.lastBuffer, this.lastBufferIndex, length);
          sourceIndex += length;
          this.lastBufferIndex += length;
          this.length += length;
          count -= length;
        }
      }
    }

    public void Append(string value)
    {
      this.Append(value, 0, value.Length);
    }

    public void Append(string value, int start, int count)
    {
      if (value == null)
      {
        if (start != 0 || count != 0)
          throw new ArgumentNullException(nameof (value));
      }
      else
      {
        if (count == 0)
          return;
        if (start < 0)
          throw new ArgumentOutOfRangeException(nameof (start));
        if (count < 0 || start + count > value.Length)
          throw new ArgumentOutOfRangeException(nameof (count));
        if (this.lastBuffer == null)
          this.CreateBuffers();
        int num1 = start;
        while (count > 0)
        {
          if (this.lastBufferIndex >= this.lastBuffer.Length)
            this.AddBuffer();
          int num2 = count;
          int num3 = this.lastBuffer.Length - this.lastBufferIndex;
          if (num3 < num2)
            num2 = num3;
          for (int index = 0; index < num2; ++index)
            this.lastBuffer[this.lastBufferIndex++] = value[num1++];
          this.length += num2;
          count -= num2;
        }
      }
    }

    public void Clear()
    {
      if (this.lastBuffer != null)
        this.ClearBuffers();
      this.length = 0;
    }

    internal void ClearBuffers()
    {
      if (this.buffers != null)
      {
        for (int index = 0; index < this.buffersCount; ++index)
          this.Recycle(this.buffers[index]);
        this.lastBuffer = (char[]) null;
      }
      this.lastBufferIndex = 0;
      this.buffersCount = 0;
    }

    public override string ToString()
    {
      string str;
      if (this.buffersCount == 0 || this.buffersCount == 1 && this.lastBufferIndex == 0)
        str = "";
      else if (this.buffersCount == 1)
      {
        str = new string(this.buffers[0].buffer, 0, this.length);
      }
      else
      {
        str = "";
        for (int index = 0; index < this.buffersCount - 1; ++index)
        {
          char[] buffer = this.buffers[index].buffer;
          int length = index == this.buffersCount - 1 ? this.lastBufferIndex + 1 : buffer.Length;
          str += new string(buffer, 0, length);
        }
      }
      return str;
    }

    public string ToString(int startIndex, int len)
    {
      if (startIndex < 0 || startIndex >= this.length)
        throw new ArgumentOutOfRangeException(nameof (startIndex));
      if (len < 0 || startIndex + len > this.length)
        throw new ArgumentOutOfRangeException(nameof (len));
      if (this.buffersCount == 0 || this.buffersCount == 1 && this.lastBufferIndex == 0)
        return "";
      if (this.buffersCount == 1)
        return new string(this.buffers[0].buffer, startIndex, len);
      string str = "";
      int index1;
      for (index1 = 0; index1 < this.buffersCount && startIndex >= this.buffers[index1].buffer.Length; ++index1)
        startIndex -= this.buffers[index1].buffer.Length;
      if (index1 < this.buffersCount)
      {
        for (int index2 = len; index1 < this.buffersCount && index2 > 0; ++index1)
        {
          char[] buffer = this.buffers[index1].buffer;
          int length = buffer.Length < index2 ? buffer.Length : index2;
          str += new string(buffer, startIndex, length);
          startIndex = 0;
          index2 -= length;
        }
      }
      return str;
    }

    private void CreateBuffers()
    {
      if (this.buffers == null)
      {
        this.lastBuffer = new char[16384];
        this.buffers = new BufferBuilder.Buffer[4];
        this.buffers[0].buffer = this.lastBuffer;
        this.buffersCount = 1;
      }
      else
        this.AddBuffer();
    }

    private void AddBuffer()
    {
      if (this.buffersCount + 1 == this.buffers.Length)
      {
        BufferBuilder.Buffer[] bufferArray = new BufferBuilder.Buffer[this.buffers.Length * 2];
        Array.Copy((Array) this.buffers, 0, (Array) bufferArray, 0, this.buffers.Length);
        this.buffers = bufferArray;
      }
      char[] chArray;
      if (this.buffers[this.buffersCount].recycledBuffer != null)
      {
        chArray = (char[]) this.buffers[this.buffersCount].recycledBuffer.Target;
        if (chArray != null)
        {
          this.buffers[this.buffersCount].recycledBuffer.Target = (object) null;
          goto label_6;
        }
      }
      chArray = new char[16384];
label_6:
      this.lastBuffer = chArray;
      this.buffers[this.buffersCount++].buffer = chArray;
      this.lastBufferIndex = 0;
    }

    private void Recycle(BufferBuilder.Buffer buf)
    {
      if (buf.recycledBuffer == null)
        buf.recycledBuffer = new WeakReference((object) buf.buffer);
      else
        buf.recycledBuffer.Target = (object) buf.buffer;
      buf.buffer = (char[]) null;
    }

    private void SetLength(int newLength)
    {
      if (newLength == this.length)
        return;
      int num1 = newLength;
      int index1;
      for (index1 = 0; index1 < this.buffersCount && num1 >= this.buffers[index1].buffer.Length; ++index1)
        num1 -= this.buffers[index1].buffer.Length;
      if (index1 < this.buffersCount)
      {
        this.lastBuffer = this.buffers[index1].buffer;
        this.lastBufferIndex = num1;
        int index2 = index1 + 1;
        int num2 = index2;
        for (; index2 < this.buffersCount; ++index2)
          this.Recycle(this.buffers[index2]);
        this.buffersCount = num2;
      }
      this.length = newLength;
    }

    private struct Buffer
    {
      internal char[] buffer;
      internal WeakReference recycledBuffer;
    }
  }
}
