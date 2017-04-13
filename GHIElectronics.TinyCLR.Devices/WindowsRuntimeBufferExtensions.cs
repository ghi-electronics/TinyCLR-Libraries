using GHIElectronics.TinyCLR.Storage.Streams;
using System.IO;

namespace System.Runtime.InteropServices.WindowsRuntime {
    public static class WindowsRuntimeBufferExtensions {
        public static IBuffer AsBuffer(this byte[] source) {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.AsBuffer(0, source.Length, source.Length);
        }

        public static IBuffer AsBuffer(this byte[] source, int offset, int length) {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.AsBuffer(offset, length, source.Length);
        }

        public static IBuffer AsBuffer(this byte[] source, int offset, int length, int capacity) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (length > capacity || source.Length - offset < length || source.Length - offset < capacity) throw new ArgumentException();

            return new Buffer(source, offset, length, capacity);
        }

        public static Stream AsStream(this IBuffer source) {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var casted = (Buffer)source;

            return new MemoryStream(casted.data, casted.offset, (int)casted.Length);
        }

        public static void CopyTo(this byte[] source, int sourceIndex, IBuffer destination, uint destinationIndex, int count) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var src = source;
            var dst = (Buffer)destination;

            if (sourceIndex >= source.Length || destinationIndex + dst.offset >= destination.Capacity || source.Length - sourceIndex < count || destination.Capacity - destinationIndex < count) throw new ArgumentException();

            Array.Copy(src, sourceIndex, dst.data, (int)(dst.offset + destinationIndex), (int)count);
        }

        public static void CopyTo(this byte[] source, IBuffer destination) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            source.CopyTo(0, destination, 0, source.Length);
        }

        public static void CopyTo(this IBuffer source, byte[] destination) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            source.CopyTo(0, destination, 0, (int)source.Length);
        }

        public static void CopyTo(this IBuffer source, uint sourceIndex, byte[] destination, int destinationIndex, int count) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var src = (Buffer)source;
            var dst = destination;

            if (sourceIndex + src.offset >= source.Capacity || destinationIndex >= destination.Length || source.Capacity - sourceIndex < count || destination.Length - destinationIndex < count) throw new ArgumentException();

            Array.Copy(src.data, (int)(src.offset + sourceIndex), dst, destinationIndex, count);
        }

        public static void CopyTo(this IBuffer source, uint sourceIndex, IBuffer destination, uint destinationIndex, uint count) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var src = (Buffer)source;
            var dst = (Buffer)destination;

            if (sourceIndex + src.offset >= source.Capacity || destinationIndex + dst.offset >= destination.Capacity || source.Capacity - sourceIndex < count || destination.Capacity - destinationIndex < count) throw new ArgumentException();

            Array.Copy(src.data, (int)(src.offset + sourceIndex), dst.data, (int)(dst.offset + destinationIndex), (int)count);
        }

        public static void CopyTo(this IBuffer source, IBuffer destination) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            source.CopyTo(0, destination, 0, source.Length);
        }

        public static byte GetByte(this IBuffer source, uint byteOffset) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (byteOffset < 0) throw new ArgumentOutOfRangeException(nameof(byteOffset));
            if (byteOffset >= source.Capacity) throw new ArgumentException();

            return ((Buffer)source).data[byteOffset];
        }

        public static IBuffer GetWindowsRuntimeBuffer(this MemoryStream underlyingStream) {
            if (underlyingStream == null) throw new ArgumentNullException(nameof(underlyingStream));

            return new Buffer(underlyingStream.GetBuffer(), 0, (int)underlyingStream.Length, (int)underlyingStream.Length);
        }

        public static IBuffer GetWindowsRuntimeBuffer(this MemoryStream underlyingStream, int positionInStream, int length) {
            if (underlyingStream == null) throw new ArgumentNullException(nameof(underlyingStream));
            if (positionInStream < 0) throw new ArgumentOutOfRangeException(nameof(positionInStream));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (positionInStream >= underlyingStream.Length) throw new ArgumentException();

            return new Buffer(underlyingStream.GetBuffer(), positionInStream, length, length);
        }

        public static bool IsSameData(this IBuffer buffer, IBuffer otherBuffer) => ((Buffer)buffer).data == ((Buffer)otherBuffer).data && ((Buffer)buffer).offset == ((Buffer)otherBuffer).offset;

        public static byte[] ToArray(this IBuffer source) => source.ToArray(0, (int)source.Length);

        public static byte[] ToArray(this IBuffer source, uint sourceIndex, int count) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (sourceIndex < 0) throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (sourceIndex >= source.Capacity || count > source.Capacity - sourceIndex) throw new ArgumentException();

            var newArr = new byte[count];

            Array.Copy(((Buffer)source).data, (int)sourceIndex, newArr, 0, count);

            return newArr;
        }
    }
}