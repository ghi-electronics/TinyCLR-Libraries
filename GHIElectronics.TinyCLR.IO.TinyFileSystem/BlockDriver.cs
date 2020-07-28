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
    public partial class TinyFileSystem
    {
        private class BlockDriver : IBlockDriver
        {
            private readonly StorageDriver storage;

            public BlockDriver(StorageDriver storage, int pagesPerCluster = 4)
            {
                this.ClusterSize = (ushort)(pagesPerCluster * storage.PageSize);
                this.storage = storage;
            }

            public void EraseChip() => this.storage.EraseChip();

            public void EraseSector(int sectorId) => this.storage.EraseSector(sectorId, 1);

            public void Read(ushort clusterId, int clusterOffset, byte[] data, int index, int count)
            {
                var address = (clusterId* this.ClusterSize) + clusterOffset;
                this.storage.ReadData(address, data, index, count);
            }

            public void Write(ushort clusterId, int clusterOffset, byte[] data, int index, int count)
            {
                var address = (clusterId* this.ClusterSize) + clusterOffset;
                this.storage.WriteData(address, data, index, count);
            }

            public int DeviceSize => this.storage.Capacity;

            public int SectorSize => this.storage.SectorSize;

            public ushort ClusterSize { get; }
        }
    }
}
