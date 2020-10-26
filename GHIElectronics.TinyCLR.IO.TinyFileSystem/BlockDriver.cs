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

using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Storage.Provider;

namespace GHIElectronics.TinyCLR.IO.TinyFileSystem {
    public partial class TinyFileSystem {
        private class BlockDriver : IBlockDriver {
            private readonly IStorageControllerProvider storageProvider;

            public BlockDriver(IStorageControllerProvider storageProvider, uint clusterSize = 1024) {
                if (storageProvider == null || storageProvider.Descriptor == null || storageProvider.Descriptor.RegionAddresses == null || storageProvider.Descriptor.RegionSizes == null)
                    throw new System.ArgumentNullException();

                if (storageProvider.Descriptor.RegionAddresses.Length != 1
                    || storageProvider.Descriptor.RegionsContiguous == false
                    || storageProvider.Descriptor.RegionsEqualSized == false
                    || storageProvider.Descriptor.RegionSizes.Length != 1)

                    throw new System.ArgumentException();

                this.ClusterSize = (ushort)clusterSize;
                this.storageProvider = storageProvider;
            }

            public void EraseChip() => this.storageProvider.EraseAll(System.TimeSpan.MaxValue);

            public void EraseSector(int sectorId) {
                var address = sectorId * this.SectorSize;

                if (!this.storageProvider.IsErased(address, this.SectorSize))
                    this.storageProvider.Erase(address, this.SectorSize, System.TimeSpan.MaxValue);
            }

            public void Read(ushort clusterId, int clusterOffset, byte[] data, int index, int count) {
                var address = (clusterId * this.ClusterSize) + clusterOffset;
                this.storageProvider.Read(address, count, data, clusterOffset, System.TimeSpan.MaxValue);
            }

            public void Write(ushort clusterId, int clusterOffset, byte[] data, int index, int count) {
                var address = (clusterId * this.ClusterSize) + clusterOffset;
                this.storageProvider.Write(address, count, data, clusterOffset, System.TimeSpan.MaxValue);
            }

            public int DeviceSize => this.storageProvider.Descriptor.RegionCount * this.storageProvider.Descriptor.RegionSizes[0];

            public int SectorSize => this.storageProvider.Descriptor.RegionSizes[0];

            public ushort ClusterSize { get; }
        }
    }
}
