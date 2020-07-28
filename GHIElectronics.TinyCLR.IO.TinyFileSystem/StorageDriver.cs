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

namespace GHIElectronics.TinyCLR.IO.TinyFileSystem {
    public abstract class StorageDriver {
        /// <summary>
        /// Gets the memory capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity, in bytes.
        /// </value>
        public abstract int Capacity { get; }

        /// <summary>
        /// Gets the size of a page in memory.
        /// </summary>
        /// <value>
        /// The size of a page in bytes
        /// </value>
        public abstract int PageSize { get; }

        /// <summary>
        /// Gets the size of a sector.
        /// </summary>
        /// <value>
        /// The size of a sector in bytes.
        /// </value>
        public abstract int SectorSize { get; }

        /// <summary>
        /// Gets the size of a block.
        /// </summary>
        /// <value>
        /// The size of a block in bytes.
        /// </value>
        public abstract int BlockSize { get; }

        /// <summary>
        /// Gets the number of pages per cluster.
        /// </summary>
        /// <value>
        /// The number of pages per cluster.
        /// </value>
        public virtual int PagesPerCluster { get; set; } = 4;

        /// <summary>
        /// Completely erases the chip.
        /// </summary>
        /// <remarks>This method is mainly used by Flash memory chips, because of their internal behaviour. It can be safely ignored with other memory types.</remarks>       
        public abstract void EraseChip();

        /// <summary>
        /// Erases "count" sectors starting at "sector".
        /// </summary>
        /// <param name="sector">The starting sector.</param>
        /// <param name="count">The count of sectors to erase.</param>       
        public abstract void EraseSector(int sector, int count);

        /// <summary>
        /// Erases "count" blocks starting at "sector".
        /// </summary>
        /// <param name="block">The starting block.</param>
        /// <param name="count">The count of blocks to erase.</param>        
        public abstract void EraseBlock(int block, int count);

        /// <summary>
        /// Writes data to a memory location.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="index">The starting index in the data array.</param>
        /// <param name="count">The count of bytes to write to memory.</param>
        /// 
        public abstract void WriteData(int address, byte[] data, int index, int count);

        /// <summary>
        /// Reads data at a specific address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="data">An array of bytes containing data read back</param>
        /// <param name="index">The starting index to read in the array.</param>
        /// <param name="count">The count of bytes to read.</param>      
        public abstract void ReadData(int address, byte[] data, int index, int count);

        /// <summary>
        /// Reads a single byte at a specified address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>A byte value</returns>       
        public byte ReadByte(int address) {
            var tmp = new byte[1];
            this.ReadData(address, tmp, 0, 1);
            return tmp[0];
        }

        /// <summary>
        /// Writes a single byte at a specified address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The value to write at the specified address.</param>
        public void WriteByte(int address, byte value) => this.WriteData(address, new[] { value }, 0, 1);
    }
}
