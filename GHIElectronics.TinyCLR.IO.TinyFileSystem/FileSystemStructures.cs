/*
 * TinyFileSystem driver for TinyCLR 2.0
 * 
 * Version 1.0
 *  - Initial revision, based on Chris Taylor (Taylorza) work
 *  - adaptations to conform to MikroBus.Net drivers design
 *  
 *  
 * Copyright 2020 MikroBus.Net
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language governing permissions and limitations under the License.
 */

using System;
using System.Text;

namespace GHIElectronics.TinyCLR.IO.TinyFileSystem {
  // The following summarizes the physical layout of the file system structures on the device.
  //
  // File Entry Cluster 
  // -------+-------+-----------------
  // offset | bytes | Description
  // -------+-------+-----------------  
  // 0      | 1     | Cluster Marker 
  //        |       |   1st Cluster of Sector : 0xff - unformatted/invalid sector, 0x7f - formatted sector/free cluster
  //        |       |   nth Cluster of Sector : 0xff - free cluster, 0x3f - allocated cluster
  //        |       |   0x1f - Orphaned page
  // 1      | 2     | Object ID > 0
  // 3      | 2     | Block Id  
  // 5      | 2     | Data Length
  // 7      | 1     | FileName Length
  // 8      | 16    | FileName
  // 24     | 8     | CreationTime
  // 32     | n     | The first n bytes of data contained in the file. The max value of n is dependent on the cluster size.

  // File Data Cluster
  // -------+-------+-----------------
  // offset | bytes | Description
  // -------+-------+-----------------  
  // 0      | 1     | Cluster Marker 
  //        |       |   1st Cluster of Sector : 0xff - unformatted/invalid sector, 0x7f - formatted sector/free cluster
  //        |       |   nth Cluster of Sector : 0xff - free cluster, 0x3f - allocated cluster
  //        |       |   0x1f - Orphaned page
  // 1      | 2     | Object ID = 0
  // 3      | 2     | Block Id  
  // 5      | 2     | Data Length
  // 7      | n     | n bytes of data contained in the file at Block Id. The max value of n is dependent on the cluster size.

    /// <summary>
    /// Markers used to indicate the state of the a cluster or sector on the disk
    /// In the case of a sector, this is just the first cluster on the disk which
    /// has the additional possible state of having the FormattedSector marker.
    /// </summary>
    public partial class TinyFileSystem
    {
        internal static class BlockMarkers
        {
            public const byte ErasedSector = 0xff;
            public const byte FormattedSector = 0x7f;
            public const byte PendingCluster = 0x3f;
            public const byte AllocatedCluster = 0x1f;
            public const byte OrphanedCluster = 0x0f;

            public static byte[] FormattedSectorBytes = {FormattedSector};
            public static byte[] PendingClusterBytes = {PendingCluster};
            public static byte[] AllocatedClusterBytes = {AllocatedCluster};
            public static byte[] OrphanedClusterBytes = {OrphanedCluster};
        }

        /// <summary>
        /// In memory representation of a file on "disk".
        /// This structure tracks the files total size, the clusters 
        /// that make up the content of the file and the number of currently open
        /// Streams on the file.
        /// </summary>
        public class FileRef
        {
            /// <summary>
            /// Unique object id of the file.
            /// </summary>
            public ushort objId;

            /// <summary>
            /// Size of the file in bytes.
            /// </summary>
            public int fileSize;

            /// <summary>
            /// Number of open streams on the file.
            /// </summary>
            public byte openCount;

            /// <summary>
            /// The list of clusters that make up the file content.
            ///
            /// Each block is sequenced in the order that the data occurs in the file.
            /// For example, index 0's value is the  cluster id of the disk location that 
            /// storing the data for the first block of file data, index 1 points to the 
            /// cluster id of the second block of data etc.
            /// <remarks>
            /// Block 0 also contains the files meta data in the form of a FileClusterBlock,
            /// subsequent block will be in the form of DataClusterBlocks. As a space optimization
            /// the FileClusterBlock also contain the initial data in the file.
            /// </remarks>
            /// </summary>
            public UInt16Array blocks = new UInt16Array();
        }

        /// <summary>
        /// Statistics for the device
        /// </summary>
        public struct DeviceStats
        {
            /// <summary>
            /// Free memory available for use.
            /// </summary>
            /// <remarks>
            /// The free memory is reported in multiples of free cluster sizes. The unused space
            /// on currently allocated clusters is not reported. This also excludes any potential 
            /// free space that is currently occupied by orphaned clusters.
            /// </remarks>
            public readonly int BytesFree;

            /// <summary>
            /// Memory occupied by orphaned clusters.
            /// </summary>
            /// <remarks>
            /// This counter will report the amount of space currently allocated to orphaned clusters.
            /// Compacting the file system will return this memory to the free pool.
            /// </remarks>
            public readonly int BytesOrphaned;

            /// <summary>
            /// Creates an instance of the DeviceStats structure.
            /// </summary>
            /// <param name="bytesFree">Bytes free in the file system.</param>
            /// <param name="bytesOrphaned">Bytes orphaned in the file system.</param>
            public DeviceStats(int bytesFree, int bytesOrphaned)
            {
                this.BytesFree = bytesFree;
                this.BytesOrphaned = bytesOrphaned;
            }

            /// <summary>
            /// Returns a <see cref="string" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="string" /> that represents this instance.
            /// </returns>
            public override string ToString() => "Bytes Free: " + this.BytesFree + "\r\n" +
                       "Bytes Orphaned: " + this.BytesOrphaned;
        }

        /// <summary>
        /// Utility class used to serialize/deserialize the data structures of the file system
        /// between the in memory representation and the on device representation.
        /// </summary>
        /// <remarks>
        /// If these structures are changed you need to be very careful about correctly updating
        /// the constants found in this structure.
        /// </remarks>
        private struct ClusterBuffer
        {
            public const int MaxFileNameLength = 16;
            public const int CommonHeaderSize = 1 + 2 + 2 + 2;
            public const int FileClusterHeaderSize = CommonHeaderSize + 1 + MaxFileNameLength + 8;
            public const int DataClusterHeaderSize = CommonHeaderSize;

            private const int MarkerOffset = 0;
            private const int ObjIdOffset = 1;
            private const int BlockIdOffset = 3;
            private const int DataLengthOffset = 5;
            public const int FileNameLengthOffset = 7;
            private const int FileNameOffset = 8;
            private const int CreationTimeOffset = 24;

            private readonly byte[] buffer;
            private readonly int clusterSize;
            private int minWrite;

            public int MaxWrite { get; set; }

            public int FileClusterMaxDataLength { get; }

            public int DataClusterMaxDataLength { get; }

            public ClusterBuffer(int clusterSize) :
                this()
            {
                this.clusterSize = clusterSize;
                this.buffer = new byte[clusterSize];
                this.FileClusterMaxDataLength = clusterSize - FileClusterHeaderSize;
                this.DataClusterMaxDataLength = clusterSize - DataClusterHeaderSize;
            }

            public void Clear()
            {
                Array.Clear(this.buffer, 0, this.clusterSize);
                this.minWrite = 0;
                this.MaxWrite = 0;
            }


            #region Get methods

            public byte GetMarker() => Blitter.GetByte(this.buffer, MarkerOffset);

            public ushort GetObjId() => Blitter.GetUInt16(this.buffer, ObjIdOffset);

            public ushort GetBlockId() => Blitter.GetUInt16(this.buffer, BlockIdOffset);

            public ushort GetDataLength() => Blitter.GetUInt16(this.buffer, DataLengthOffset);

            private byte GetFileNameLength() => Blitter.GetByte(this.buffer, FileNameLengthOffset);

            public string GetFileName() => Blitter.GetString(this.buffer, this.GetFileNameLength(), FileNameOffset);

            public DateTime GetCreationTime() => Blitter.GetDateTime(this.buffer, CreationTimeOffset);

            public ushort GetDataStartOffset() => this.GetDataOffset(this.GetBlockId() == 0);

            private ushort GetDataOffset(bool isFileEntry) => (ushort)(isFileEntry ? FileClusterHeaderSize : DataClusterHeaderSize);

            #endregion

            #region Set methods

            public void SetMarker(byte value) => this.UpdateWriteRange(MarkerOffset, Blitter.ToBytes(this.buffer, value, MarkerOffset));

            public void SetObjId(ushort value) => this.UpdateWriteRange(ObjIdOffset, Blitter.ToBytes(this.buffer, value, ObjIdOffset));

            public void SetBlockId(ushort value) => this.UpdateWriteRange(BlockIdOffset, Blitter.ToBytes(this.buffer, value, BlockIdOffset));

            public void SetDataLength(ushort value) => this.UpdateWriteRange(DataLengthOffset, Blitter.ToBytes(this.buffer, value, DataLengthOffset));

            public void SetFileName(string value)
            {
                var byteLen = (byte) Blitter.ToBytes(this.buffer, value.ToUpper(), MaxFileNameLength, FileNameOffset);
                Blitter.ToBytes(this.buffer, byteLen, FileNameLengthOffset);
                this.UpdateWriteRange(FileNameOffset, byteLen);
            }

            public void SetCreationTime(DateTime value) => this.UpdateWriteRange(CreationTimeOffset, Blitter.ToBytes(this.buffer, value, CreationTimeOffset));

            public void SetData(byte[] data, int offset, int destinationOffset, int length)
            {
                var firstByteOffset = this.GetDataStartOffset() + destinationOffset;
                Array.Copy(data, offset, this.buffer, firstByteOffset, length);
                this.UpdateWriteRange(firstByteOffset, length);
            }

            #endregion

            private void UpdateWriteRange(int offset, int length)
            {
                if (this.minWrite > offset) this.minWrite = offset;
                if (this.MaxWrite < offset + length) this.MaxWrite = offset + length;
            }

            public static implicit operator byte[](ClusterBuffer o) => o.buffer;
        }

        /// <summary>
        /// A utility class to facilitate the serialization and deserialization
        /// of the data types used in the FileS System structures to and from memory.
        /// </summary>
        /// <remarks>
        /// This utility class is limited to the types required by the file system.
        /// GetXXX  - functions extract the data type XXX from the supplied buffer starting at the specified index
        /// ToBytes - function push the byte representation of the data type into the supplied byte buffer starting at the specified index.
        /// </remarks>
        private static class Blitter
        {
            #region GetXXX

            public static byte GetByte(byte[] buffer, int index) => buffer[index];

            public static ushort GetUInt16(byte[] buffer, int index)
            {
                var b1 = buffer[index++];
                var b2 = buffer[index];
                return (ushort) (b1 | (b2 << 8));
            }

/*
            public static int GetInt32(byte[] buffer, int index)
            {
                byte b1 = buffer[index++];
                byte b2 = buffer[index++];
                byte b3 = buffer[index++];
                byte b4 = buffer[index];
                return (ushort) (b1 | (b2 << 8) | (b3 << 16) | (b4 | 24));
            }
*/

            private static long GetInt64(byte[] buffer, int index)
            {
                long b1 = buffer[index++];
                long b2 = buffer[index++];
                long b3 = buffer[index++];
                long b4 = buffer[index++];
                long b5 = buffer[index++];
                long b6 = buffer[index++];
                long b7 = buffer[index++];
                long b8 = buffer[index];

                return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24) | (b5 << 32) | (b6 << 40) | (b7 << 48) | (b8 << 56);
            }

            public static DateTime GetDateTime(byte[] buffer, int index)
            {
                var ticks = GetInt64(buffer, index);
                return new DateTime(ticks);
            }

/*
            public static string GetString(byte[] buffer, int index)
            {
                ushort length = GetUInt16(buffer, index);
                return GetString(buffer, length, index);
            }
*/

            public static string GetString(byte[] buffer, int length, int index)
            {
                var bytes = new byte[length];
                Array.Copy(buffer, index, bytes, 0, length);
/*
                index += length;
*/
                return new string(Encoding.UTF8.GetChars(bytes));
            }

            #endregion

            #region ToBytes

            public static int ToBytes(byte[] buffer, byte value, int index)
            {
                buffer[index] = value;
                return 1;
            }

            public static int ToBytes(byte[] buffer, ushort value, int index)
            {
                buffer[index++] = (byte) (value & 0xff);
                buffer[index] = (byte) ((value >> 8) & 0xff);
                return 2;
            }

/*
            public static int ToBytes(byte[] buffer, int value, int index)
            {
                buffer[index++] = (byte) (value & 0xff);
                buffer[index++] = (byte) ((value >> 8) & 0xff);
                buffer[index++] = (byte) ((value >> 16) & 0xff);
                buffer[index] = (byte) ((value >> 24) & 0xff);
                return 4;
            }
*/

            private static int ToBytes(byte[] buffer, long value, int index)
            {
                buffer[index++] = (byte) (value & 0xff);
                buffer[index++] = (byte) ((value >> 8) & 0xff);
                buffer[index++] = (byte) ((value >> 16) & 0xff);
                buffer[index++] = (byte) ((value >> 24) & 0xff);
                buffer[index++] = (byte) ((value >> 32) & 0xff);
                buffer[index++] = (byte) ((value >> 40) & 0xff);
                buffer[index++] = (byte) ((value >> 48) & 0xff);
                buffer[index] = (byte) ((value >> 56) & 0xff);
                return 8;
            }

            public static int ToBytes(byte[] buffer, DateTime value, int index) => ToBytes(buffer, value.Ticks, index);

            /*
                        public static int ToBytes(byte[] buffer, string value, int index)
                        {
                            return ToBytes(buffer, value, ushort.MaxValue, index);
                        }
            */

            public static int ToBytes(byte[] buffer, string value, ushort maxLength, int index)
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var byteCount = bytes.Length;
                Array.Copy(bytes, 0, buffer, index, Math.Min(maxLength, byteCount));
/*
                index += byteCount;
*/
                return byteCount;
            }

            #endregion
        }
    }
}
