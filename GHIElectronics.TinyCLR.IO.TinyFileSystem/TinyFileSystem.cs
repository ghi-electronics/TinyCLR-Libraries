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
using System.Collections;
using System.IO;
using System.Diagnostics;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Devices.Storage.Provider;

namespace GHIElectronics.TinyCLR.IO.TinyFileSystem {
    /// <summary>
    /// Tiny File System provides a basic file system which can be overlayed on any device providing
    /// a concrete implementation of the IBlockDriver interface.
    /// </summary>
    public sealed partial class TinyFileSystem
    {
        #region Private state variables
        private readonly IBlockDriver blockDriver;
        private readonly ushort totalSectorCount;
        private readonly ushort clustersPerSector;
        private readonly ushort totalClusterCount;
        private ushort lastObjId;
        private ushort headSectorId;
        private ushort tailClusterId;
        private ushort freeClusterCount;
        private readonly ushort minFreeClusters;

        private ushort orphanedClusterCount;
        private bool mounted;
        private bool compacting;

        private ClusterBuffer cluster;
        private ClusterBuffer defragBuffer;
        private readonly byte[] orphanedPerSector;

        private readonly object syncLock = new object();

        private readonly Hashtable filesIndex = new Hashtable();
        #endregion

        #region .ctor
        /// <summary>
        /// Creates an instance of TinyFileSystem.
        /// </summary>
        
        public TinyFileSystem(IStorageControllerProvider storageProvider, uint clusterSize)
        {
            this.blockDriver = new BlockDriver(storageProvider, clusterSize);

            // Precalculate commonly used values based on the device parameters provided by the block driver.
            this.totalSectorCount = (ushort)(this.blockDriver.DeviceSize / this.blockDriver.SectorSize);
            this.clustersPerSector = (ushort)(this.blockDriver.SectorSize / this.blockDriver.ClusterSize);
            this.totalClusterCount = (ushort)(this.blockDriver.DeviceSize / this.blockDriver.ClusterSize);

            this.orphanedPerSector = new byte[this.totalSectorCount];

            // Initialize the device state tracking data
            this.minFreeClusters = (ushort)(this.clustersPerSector * 2);
            this.headSectorId = 0;
            this.tailClusterId = 0;

            // Precreate buffers that are used by the file system
            this.cluster = new ClusterBuffer(this.blockDriver.ClusterSize);
            this.defragBuffer = new ClusterBuffer(this.blockDriver.ClusterSize);
        }
        #endregion

        #region Public Tiny file system interface

        /// <summary>
        /// Scan the flash module to verify that it is formatted.
        /// </summary>
        /// <returns>true if formatted else false.</returns>
        public bool CheckIfFormatted()
        {
            for (ushort sectorId = 0; sectorId < this.totalSectorCount; ++sectorId)
            {
                var clusterId = (ushort)(sectorId * this.clustersPerSector);

                this.blockDriver.Read(clusterId, 0, this.cluster, 0, ClusterBuffer.CommonHeaderSize);
                var marker = this.cluster.GetMarker();

                if (marker != BlockMarkers.FormattedSector
                  && marker != BlockMarkers.AllocatedCluster
                  && marker != BlockMarkers.OrphanedCluster
                  && marker != BlockMarkers.PendingCluster)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Mount an existing file system from a device.    
        /// </summary>
        /// <remarks>
        /// Before using the file system it must be mounted. The only exception is
        /// for Format, which will format the device and mount it immediately.
        /// 
        /// The user should ensure that the device has a valid file system.
        /// Currently there are only very basic heuristics to determine if the
        /// device has a valid file system. This will be improved over time.
        /// </remarks>
        public void Mount()
        {
            lock (this.syncLock)
            {
                try
                {
                    if (this.mounted) return;

                    var objIds = new Hashtable();

                    var headClusterId = -1;
                    var tailClusterId = -1;

                    this.freeClusterCount = 0;

                    var foundHole = false;
                    var foundData = false;

                    for (ushort clusterId = 0; clusterId < this.totalClusterCount; clusterId++)
                    {
                        this.blockDriver.Read(clusterId, 0, this.cluster, 0, ClusterBuffer.CommonHeaderSize);
                        var marker = this.cluster.GetMarker();

                        var isFirstClusterOfSector = (clusterId % this.clustersPerSector) == 0;

                        if (isFirstClusterOfSector)
                        {
                            if (marker != BlockMarkers.FormattedSector
                            && marker != BlockMarkers.AllocatedCluster
                            && marker != BlockMarkers.OrphanedCluster
                            && marker != BlockMarkers.PendingCluster)
                            {
                                throw new TinyFileSystemException(StringTable.Error_NotFormatted, TinyFileSystemException.TfsErrorCode.NotFormatted);
                            }
                        }

                        var clusterIsAvailable = marker == BlockMarkers.ErasedSector || marker == BlockMarkers.FormattedSector;
                        var clusterIsOrphaned = marker == BlockMarkers.OrphanedCluster || marker == BlockMarkers.PendingCluster;

                        if (headClusterId != -1 && clusterIsAvailable) foundHole = true;
                        if (tailClusterId != -1 && !clusterIsAvailable) foundData = true;

                        if (!clusterIsAvailable && (headClusterId == -1 || foundHole))
                        {
                            headClusterId = clusterId;
                            foundHole = false;
                        }

                        if (clusterIsAvailable && (tailClusterId == -1 || foundData))
                        {
                            tailClusterId = clusterId;
                            foundData = false;
                        }

                        if (marker == BlockMarkers.AllocatedCluster)
                        {
                            var objId = this.cluster.GetObjId();

                            var blockId = this.cluster.GetBlockId();
                            if (blockId > this.totalClusterCount) throw new TinyFileSystemException(StringTable.Error_NotFormatted, TinyFileSystemException.TfsErrorCode.NotFormatted);

                            var length = this.cluster.GetDataLength();
                            if (length > this.blockDriver.ClusterSize) throw new TinyFileSystemException(StringTable.Error_NotFormatted, TinyFileSystemException.TfsErrorCode.NotFormatted);

                            if (objId > this.lastObjId) this.lastObjId = objId;

                            FileRef file;
                            if (!objIds.Contains(objId))
                            {
                                file = new FileRef { objId = objId };
                                objIds[objId] = file;
                            }
                            else
                            {
                                file = (FileRef)objIds[objId];
                            }

                            file.blocks[blockId] = clusterId;
                            file.fileSize += length;
                        }
                        else if (isFirstClusterOfSector && marker == BlockMarkers.FormattedSector)
                        {
                            this.freeClusterCount += this.clustersPerSector;
                            clusterId += (ushort)(this.clustersPerSector - 1);
                        }
                        else if (clusterIsAvailable)
                        {
                            this.freeClusterCount++;
                        }
                        else if (clusterIsOrphaned)
                        {
                            this.orphanedClusterCount++;
                            this.orphanedPerSector[clusterId / this.clustersPerSector]++;
                        }
                    }

                    this.headSectorId = headClusterId != -1 ? (ushort)(headClusterId / this.clustersPerSector) : (ushort)0;

                    this.tailClusterId = (ushort)(tailClusterId == -1 ? 0 : tailClusterId);

                    foreach (FileRef file in objIds.Values)
                    {
                        if (file.blocks.Count > 0)
                        {
                            this.filesIndex.Add(file.objId, file);
                        }
                        else
                        {
                            for (var i = 0; i < file.blocks.Count; i++)
                            {
                                this.MarkClusterOrphaned(file.blocks[i]);
                            }
                        }
                    }

                    this.mounted = true;
                }
                catch
                {
                    // Something went wrong with the mount. Clean-up and re-throw.
                    this.filesIndex.Clear();
                    throw;
                }
            }
        }

        /// <summary>
        /// Formats the device.
        /// </summary>
        /// <remarks>
        /// This will prepare the device for the file system. 
        /// Note that this will destroy any pre-existing data on the device.
        /// </remarks>
        public void Format()
        {
            lock (this.syncLock)
            {
                // Check that there are no open files
                foreach (FileRef fr in this.filesIndex.Values)
                {
                    if (fr.openCount > 0) throw new IOException(StringTable.Error_FileIsInUse, (int)TinyFileSystemException.TfsErrorCode.FileInUse);
                }

                this.blockDriver.EraseChip();

                for (ushort i = 0, clusterId = 0; i < this.totalSectorCount; i++, clusterId += this.clustersPerSector)
                {
                    //  Debug.Print("i = " + i.ToString() + ", ClusterId = " + clusterId.ToString());
                    this.blockDriver.Write(clusterId, 0, BlockMarkers.FormattedSectorBytes, 0, 1);
                    //Thread.Sleep(10);
                }
                this.headSectorId = 0;
                this.tailClusterId = 0;
                this.freeClusterCount = this.totalClusterCount;
                this.filesIndex.Clear();

                this.mounted = true;
            }
        }

        /// <summary>
        /// Compacts the file system.
        /// </summary>
        /// <remarks>
        /// Collects all the active clusters and puts them together on the device and formats add the freed sectors.
        /// Frees up all the space occupied by inactive clusters.
        /// </remarks>
        public void Compact()
        {
            lock (this.syncLock)
            {
                this.CheckState();
                this.compacting = true;
                while (this.orphanedClusterCount > 0)
                {
                    // Find the best sector to migrate, the one that will reclaim the most free space          
                    var sectorId = this.GetSectorToCompact();

                    // Migrate the active data from the sector to tail of the log
                    if (!this.MigrateSector(sectorId))
                        break;

                    // If it was not already the head sector, migrate the head sector into
                    // the newly freed sector.
                    if (sectorId != this.headSectorId)
                    {
                        if (!this.MigrateSector(this.headSectorId, (ushort)(sectorId * this.clustersPerSector)))
                            break;
                    }

                    // Move the head sector forward
                    this.headSectorId = (ushort)((this.headSectorId + 1) % this.totalSectorCount);
                }
                this.compacting = false;
            }
        }

        /// <summary>
        /// Copies a file to a new file.
        /// </summary>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file</param>
        /// <param name="overwrite">Specifies if the destination should be overwritten if it already exists. Default true.</param>
        public void Copy(string sourceFileName, string destFileName, bool overwrite = true)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                var srcFile = this.GetFileRef(sourceFileName);
                if (srcFile == null) throw new IOException(StringTable.Error_FileNotFound, (int)IOException.IOExceptionErrorCode.FileNotFound);

                var destFile = this.GetFileRef(destFileName);
                if (destFile != null)
                {
                    if (overwrite)
                    {
                        this.Delete(destFile);
                    }
                    else
                    {
                        throw new IOException(StringTable.Error_FileAlreadyExists, (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
                    }
                }

                var newObjId = ++this.lastObjId;

                this.blockDriver.Read(srcFile.blocks[0], 0, this.cluster, 0, this.blockDriver.ClusterSize);

                this.cluster.SetMarker(BlockMarkers.PendingCluster);
                this.cluster.SetFileName(destFileName);
                this.cluster.SetCreationTime(DateTime.Now);
                this.cluster.SetObjId(newObjId);

                var newClusterId = this.WriteToLog(this.cluster, 0, this.blockDriver.ClusterSize);
                this.MarkClusterAllocated(newClusterId);

                destFile = new FileRef { objId = newObjId };
                destFile.blocks[0] = newClusterId;

                for (ushort blockId = 1; blockId < srcFile.blocks.Count; blockId++)
                {
                    this.blockDriver.Read(srcFile.blocks[blockId], 0, this.cluster, 0, this.blockDriver.ClusterSize);

                    this.cluster.SetMarker(BlockMarkers.PendingCluster);
                    this.cluster.SetObjId(destFile.objId);

                    newClusterId = this.WriteToLog(this.cluster, 0, this.blockDriver.ClusterSize);
                    this.MarkClusterAllocated(newClusterId);

                    destFile.blocks[blockId] = newClusterId;
                }

                destFile.fileSize = srcFile.fileSize;
                this.filesIndex.Add(newObjId, destFile);
            }
        }

        /// <summary>
        /// Creates or overwrites a file.
        /// </summary>
        /// <param name="fileName">Name of the file to create.</param>
        /// <param name="bufferSize">Size of the read/write buffer. 0 for no buffering.</param>
        /// <returns>TinyFileStream that provides stream based access to the file.</returns>
        public Stream Create(string fileName, int bufferSize = 4096)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                var fs = this.CreateStream(fileName);
                return bufferSize > 0 ? new BufferedStream(fs, bufferSize) : (Stream)fs;
            }
        }

        /// <summary>
        /// Deletes a file from the device.
        /// </summary>
        /// <param name="fileName">Name of the file to delete.</param>
        /// <remarks>
        /// An IOException will be thrown if the file is open.
        /// </remarks>
        public void Delete(string fileName)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                var file = this.GetFileRef(fileName);
                if (file == null) throw new IOException(StringTable.Error_FileNotFound, (int)IOException.IOExceptionErrorCode.FileNotFound);
                this.Delete(file);
            }
        }

        /// <summary>
        /// Determines if the specified file exists.
        /// </summary>
        /// <param name="fileName">Name of the file to check.</param>
        /// <returns>true if the file exists otherwise false.</returns>
        public bool Exists(string fileName)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                return this.GetFileRef(fileName) != null;
            }
        }

        /// <summary>
        /// Moves a file from the source to the destination.
        /// </summary>
        /// <remarks>
        /// Since the Tiny File System does not support directories, move is only used to rename a file.
        /// </remarks>
        /// <param name="sourceFileName">Name of the file to move.</param>
        /// <param name="destFileName">New name of the file.</param>
        public void Move(string sourceFileName, string destFileName)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                var sourceFile = this.GetFileRef(sourceFileName);
                if (this.GetFileRef(destFileName) != null) throw new IOException(StringTable.Error_FileAlreadyExists, (int)IOException.IOExceptionErrorCode.PathAlreadyExists);

                var fileClusterId = sourceFile.blocks[0];

                this.blockDriver.Read(fileClusterId, 0, this.cluster, 0, this.blockDriver.ClusterSize);
                this.cluster.SetMarker(BlockMarkers.PendingCluster);
                this.cluster.SetFileName(destFileName);
                var newClusterId = this.WriteToLog(this.cluster, 0, this.blockDriver.ClusterSize);
                this.MarkClusterOrphaned(fileClusterId);
                this.MarkClusterAllocated(newClusterId);

                sourceFile.blocks[0] = newClusterId;
            }
        }

        /// <summary>
        /// Opens a TinyFileStream for the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file to open.</param>
        /// <param name="fileMode">Specifies what to do when opening the file.</param>
        /// <param name="bufferSize">Size of the read/write buffer. 0 for no buffering.</param>
        /// <returns>A TinyFileStream which provides stream based access to the file.</returns>
        public Stream Open(string fileName, FileMode fileMode, int bufferSize = 4096)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                TinyFileStream fs = null;

                if (fileMode < FileMode.CreateNew || fileMode > FileMode.Append) throw new ArgumentOutOfRangeException("fileMode");

                var file = this.GetFileRef(fileName);
                try
                {
                    if (file != null) fs = new TinyFileStream(this, file);

                    switch (fileMode)
                    {
                        case FileMode.Append:
                            if (fs == null) fs = this.CreateStream(fileName);
                            fs.Seek(0, SeekOrigin.End);
                            break;

                        case FileMode.Create:
                            if (fs == null)
                                fs = this.CreateStream(fileName);
                            else
                                this.Truncate(file, 0);
                            break;

                        case FileMode.CreateNew:
                            if (fs != null) throw new IOException(StringTable.Error_FileAlreadyExists, (int)IOException.IOExceptionErrorCode.PathAlreadyExists);
                            fs = this.CreateStream(fileName);
                            break;

                        case FileMode.Open:
                            if (fs == null) throw new IOException(StringTable.Error_FileNotFound, (int)IOException.IOExceptionErrorCode.FileNotFound);
                            break;

                        case FileMode.OpenOrCreate:
                            if (fs == null) fs = this.CreateStream(fileName);
                            break;

                        case FileMode.Truncate:
                            if (fs == null) throw new IOException(StringTable.Error_FileNotFound);
                            this.Truncate(file, 0);
                            break;
                    }

                    return bufferSize > 0 ? new BufferedStream(fs, bufferSize) : (Stream)fs;
                }
                catch
                {
                    // Something failed so we need to clean-up and re-throw the exception
                    if (fs != null) fs.Close();
                    throw;
                }
            }
        }

        /// <summary>
        /// Opens a file, reads the content into a byte array and then closes the file.
        /// </summary>
        /// <param name="fileName">Name of the file to read.</param>
        /// <returns>A byte array containing the data from the file.</returns>
        public byte[] ReadAllBytes(string fileName)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                using (var fs = this.Open(fileName, FileMode.Open))
                {
                    var data = new byte[(int)fs.Length];
                    fs.Read(data, 0, (int)fs.Length);
                    return data;
                }
            }
        }

        /// <summary>
        /// Creates a new file, writes the byte array to the file and then closes it.
        /// The file is overwritten if it already exists.
        /// </summary>
        /// <param name="fileName">Name of the file to create.</param>
        /// <param name="data">Bytes to be written to the file.</param>
        public void WriteAllBytes(string fileName, byte[] data)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                using (var fs = this.Open(fileName, FileMode.Create))
                {
                    fs.Write(data, 0, data.Length);
                }
            }
        }

        /// <summary>
        /// Returns the names of the files in the file system.
        /// </summary>
        /// <returns>An array of the names of the files.</returns>
        public string[] GetFiles()
        {
            lock (this.syncLock)
            {
                this.CheckState();
                var files = new string[this.filesIndex.Count];
                var i = 0;
                foreach (FileRef f in this.filesIndex.Values)
                {
                    files[i++] = this.GetFileName(f);
                }
                Utilities.Sort(files);
                return files;
            }
        }

        /// <summary>
        /// Gets the size of the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file for which to retrieve the file size.</param>
        /// <returns>Size of the file in bytes.</returns>
        public long GetFileSize(string fileName)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                var file = this.GetFileRef(fileName);
                if (file == null) throw new IOException(StringTable.Error_FileNotFound, (int)IOException.IOExceptionErrorCode.FileNotFound);
                return file.fileSize;
            }
        }

        /// <summary>
        /// Gets the creation time of the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file for which to retrieve the creation time.</param>
        /// <returns>The creation time of the specified file.</returns>
        public DateTime GetFileCreationTime(string fileName)
        {
            lock (this.syncLock)
            {
                this.CheckState();
                var file = this.GetFileRef(fileName);
                if (file == null) throw new IOException(StringTable.Error_FileNotFound, (int)IOException.IOExceptionErrorCode.FileNotFound);
                this.blockDriver.Read(file.blocks[0], 0, this.cluster, 0, ClusterBuffer.FileClusterHeaderSize);
                return this.cluster.GetCreationTime();
            }
        }

        /// <summary>
        /// Get the current statistics of the file system.
        /// </summary>
        /// <returns>Structure with the statistics of the file system.</returns>
        public DeviceStats GetStats()
        {
            lock (this.syncLock)
            {
                return new DeviceStats(this.freeClusterCount * this.blockDriver.ClusterSize, this.orphanedClusterCount * this.blockDriver.ClusterSize);
            }
        }
        #endregion


        private void CheckState()
        {
            if (!this.mounted) throw new InvalidOperationException(StringTable.Error_NotMounted);
        }

        private TinyFileStream CreateStream(string fileName)
        {
            var file = this.CreateFile(fileName);
            return new TinyFileStream(this, file);
        }

        private FileRef CreateFile(string fileName)
        {
            if (Encoding.UTF8.GetBytes(fileName).Length > ClusterBuffer.MaxFileNameLength) throw new ArgumentException("Filename exceeds 16 bytes", "fileName");

            // Check for an existing file with the specified name.
            // If one exists, delete it.
            var file = this.GetFileRef(fileName);
            if (file != null)
            {
                this.Delete(file);
            }

            // Generate the new object id for the new file
            var newObjId = ++this.lastObjId;

            // Fill the custer buffer with the FileCluster data.
            // The cluster is initially marked as pending.
            this.cluster.Clear();
            this.cluster.SetMarker(BlockMarkers.PendingCluster);
            this.cluster.SetObjId(newObjId);
            this.cluster.SetBlockId(0);
            this.cluster.SetFileName(fileName);
            this.cluster.SetCreationTime(DateTime.Now);

            // Create the FileRef
            file = new FileRef { objId = newObjId, fileSize = 0 };

            // Write the FileCluster to the log an get the cluster id
            var newClusterId = this.WriteToLog(this.cluster, 0, this.cluster.MaxWrite);

            // Mark the cluster as Allocated.
            this.MarkClusterAllocated(newClusterId);

            // Update the block 0 entry to reference the cluster on the device
            // containing the FileCluster entry for the new file.
            file.blocks[0] = newClusterId;

            // Add the file to the in-memory index
            this.filesIndex.Add(file.objId, file);

            return file;
        }

        internal int Read(FileRef file, long filePosition, byte[] data, int offset, int length)
        {
            lock (this.syncLock)
            {
                // Reads at or past the end of the file returns 0.
                if (filePosition >= file.fileSize) return 0;

                ushort firstBlockId;
                ushort dataOffset;

                // Dermine if we in the special case of reading data from the first cluster
                // of the file. This data is read from the FileCluster.
                if (filePosition < this.cluster.FileClusterMaxDataLength)
                {
                    firstBlockId = 0;
                    dataOffset = (ushort)filePosition;
                }
                else
                {
                    // Not reading from the first cluster of the file.
                    // Compensate for the asymetric data size between FileClusters and DataClusters.
                    // This requires adjusting the offset into the cluster from where the data is read from.
                    var adjustedOffset = filePosition - this.cluster.FileClusterMaxDataLength;
                    firstBlockId = (ushort)((adjustedOffset / this.cluster.DataClusterMaxDataLength) + 1);
                    dataOffset = (ushort)(adjustedOffset % this.cluster.DataClusterMaxDataLength);
                }

                // Track the number of bytes remaining to be read
                var bytesRemaining = length;

                // Loop through each block of the file reading the data and packing it into the provided
                // byte array until there are either no more blocks to read or bytesRemaining is 0.
                for (int blockId = firstBlockId; bytesRemaining > 0 && blockId < file.blocks.Count; blockId++)
                {
                    this.cluster.Clear();

                    // Get the cluster id for the file block
                    var clusterId = file.blocks[blockId];

                    // Read the cluster into memory
                    this.blockDriver.Read(clusterId, 0, this.cluster, 0, this.blockDriver.ClusterSize);

                    // Calculate how much data we have from this cluster
                    var dataLength = this.cluster.GetDataLength();
                    var bytesToRead = (ushort)System.Math.Min(bytesRemaining, dataLength - dataOffset);

                    // Transfer the data from the cluster buffer to the data array
                    Array.Copy(this.cluster, this.cluster.GetDataStartOffset() + dataOffset, data, offset, bytesToRead);

                    // Update the tracking variables.
                    dataOffset = 0;
                    offset += bytesToRead;
                    bytesRemaining -= bytesToRead;
                }

                // Return the final number of bytes read
                return length - bytesRemaining;
            }
        }

        internal void Write(FileRef file, long filePosition, byte[] data, int srcOffset, int length)
        {
            lock (this.syncLock)
            {
                if (filePosition > file.fileSize) throw new IOException(StringTable.Error_WritePastEnd, (int)IOException.IOExceptionErrorCode.Others);

                ushort firstBlockId;
                ushort clusterDataOffset;

                // Dermine if we in the special case of writing data to the first cluster
                // of the file. This data is written to the FileCluster entry.
                if (filePosition < this.cluster.FileClusterMaxDataLength)
                {
                    firstBlockId = 0;
                    clusterDataOffset = (ushort)filePosition;
                }
                else
                {
                    // Not writing to the first cluster of the file.
                    // Compensate for the asymetric data size between FileClusters and DataClusters.
                    // This requires adjusting the offset into the cluster to where the data will be written. 
                    var adjustedOffset = filePosition - this.cluster.FileClusterMaxDataLength;
                    firstBlockId = (ushort)((adjustedOffset / this.cluster.DataClusterMaxDataLength) + 1);
                    clusterDataOffset = (ushort)(adjustedOffset % this.cluster.DataClusterMaxDataLength);
                }

                // Track the number of byte remaining to be written.
                var bytesRemaining = length;

                // Loop through each block of the file writting the data from the data buffer to the 
                // cluster.
                for (var blockId = firstBlockId; bytesRemaining > 0; blockId++)
                {
                    this.cluster.Clear();

                    // Calculate how much data can be written to the target cluster. If the cluster is a block 0
                    // cluster (first block of the file) the max data that can be written to the cluster is 
                    // _cluster.FileClusterMaxDataLength otherwise it is _cluster.DataClusterMaxDataLength.
                    // the actual values depends on the cluster size supported by the IBlockDriver.
                    var maxDataLength = blockId == 0 ? this.cluster.FileClusterMaxDataLength : this.cluster.DataClusterMaxDataLength;

                    // Calculate the number of bytes to write from the the data buffer to the cluster.
                    var bytesToWrite = (ushort)System.Math.Min(bytesRemaining, maxDataLength - clusterDataOffset);

                    var currentSize = 0;
                    var sizeDelta = 0;
                    ushort newClusterId;

                    // Check if we are writting to an existing block of the file or is the
                    // file gaining a new block.
                    if (blockId < file.blocks.Count)
                    {
                        // We are writting to an existing block.          
                        // The cluster from the existing block will be orphaned and a new replacement cluster
                        // created with the modified data.

                        // Get the cluster id that will be orphaned
                        var orphanedClusterId = file.blocks[blockId];

                        ushort clusterReadOffset;
                        // Update the _cluster.MaxWrite based on the type of cluster we are writting to.
                        // First cluster is a FileCluster subsequent clusters are DataCluster entries.
                        if (blockId == 0)
                        {
                            // Read the cluster header
                            this.blockDriver.Read(orphanedClusterId, 0, this.cluster, 0, ClusterBuffer.FileClusterHeaderSize);
                            clusterReadOffset = ClusterBuffer.FileClusterHeaderSize;

                            currentSize = file.fileSize > 0 ? this.cluster.GetDataLength() : 0;
                            this.cluster.MaxWrite = ClusterBuffer.FileClusterHeaderSize + currentSize;
                        }
                        else
                        {
                            // Read the cluster header
                            this.blockDriver.Read(orphanedClusterId, 0, this.cluster, 0, ClusterBuffer.DataClusterHeaderSize);
                            clusterReadOffset = ClusterBuffer.DataClusterHeaderSize;

                            currentSize = this.cluster.GetDataLength();
                            this.cluster.MaxWrite = ClusterBuffer.DataClusterHeaderSize + currentSize;
                        }

                        // Read the remainder of the cluster
                        if (currentSize > 0)
                        {
                            this.blockDriver.Read(orphanedClusterId, clusterReadOffset, this.cluster, clusterReadOffset, currentSize);
                        }

                        // Calculate if the amount of data on the cluster is growing or are we just
                        // overwritting existing data on the cluster.
                        var excess = (clusterDataOffset + bytesToWrite) - currentSize;
                        if (excess > 0)
                        {
                            sizeDelta = excess;
                        }
                        this.cluster.MaxWrite = clusterReadOffset + currentSize + sizeDelta;

                        // Setup the cluster buffer for the new cluster that will be written
                        // the cluster is initially written as a Pending Cluster.
                        // Note: the cluster buffer already contains the objid and blockid from the inital
                        //       read from the device.
                        this.cluster.SetMarker(BlockMarkers.PendingCluster);
                        this.cluster.SetDataLength((ushort)(currentSize + sizeDelta));
                        this.cluster.SetData(data, srcOffset, clusterDataOffset, bytesToWrite);

                        // Write the new cluster to the log and get the cluster id.
                        newClusterId = this.WriteToLog(this.cluster, 0, this.cluster.MaxWrite);

                        // Mark the old cluster as orphaned
                        this.MarkClusterOrphaned(orphanedClusterId);

                        // Mark the new cluster as allocated (active part of the file).
                        this.MarkClusterAllocated(newClusterId);
                    }
                    else
                    {
                        // We are writting a cluster for the file, so the file will gain a new block.

                        sizeDelta = bytesToWrite;

                        // Setup the cluster buffer with the relevant data or the new block.
                        // The cluster is initialy written as a Pending cluster.
                        this.cluster.SetMarker(BlockMarkers.PendingCluster);
                        this.cluster.SetObjId(file.objId);
                        this.cluster.SetBlockId(blockId);

                        this.cluster.SetDataLength((ushort)(currentSize + sizeDelta));
                        this.cluster.SetData(data, srcOffset, clusterDataOffset, bytesToWrite);

                        // Write the cluster to the log and get the cluster id.
                        newClusterId = this.WriteToLog(this.cluster, 0, this.cluster.MaxWrite);

                        // Mark the cluster as Allocated
                        this.MarkClusterAllocated(newClusterId);
                    }

                    // Update the in-memory file entry with the new cluster id for the file block
                    // that was written to. And update the in file size.
                    file.blocks[blockId] = newClusterId;
                    file.fileSize += sizeDelta;

                    bytesRemaining -= bytesToWrite;
                    srcOffset += bytesToWrite;
                    clusterDataOffset = 0;
                }
            }
        }

        internal void Truncate(FileRef file, long filePosition)
        {
            lock (this.syncLock)
            {
                if (filePosition > file.fileSize) throw new IOException(StringTable.Error_WritePastEnd, (int)IOException.IOExceptionErrorCode.Others);

                ushort firstBlockId;
                ushort dataOffset;

                // Determine if we are truncating the file at a location within 
                // the first block of the file or one of the later blocks.
                if (filePosition < this.cluster.FileClusterMaxDataLength)
                {
                    firstBlockId = 0;
                    dataOffset = (ushort)filePosition;
                }
                else
                {
                    // If it is not the first block of the file we need to compensate for the 
                    // asymetric data size written to the File Cluster vs. that of the subsequent
                    // Data Cluster(s).
                    var adjustedOffset = filePosition - this.cluster.FileClusterMaxDataLength;
                    firstBlockId = (ushort)((adjustedOffset / this.cluster.DataClusterMaxDataLength) + 1);
                    dataOffset = (ushort)(adjustedOffset % this.cluster.DataClusterMaxDataLength);
                }

                if (dataOffset > 0 || firstBlockId == 0)
                {
                    // We are truncating the file starting at a offset into the cluster
                    // so we need to write the earlier data to a new cluster and orphan the 
                    // old cluster.

                    // Store the current cluster id of the block.
                    var orphanedClusterId = file.blocks[firstBlockId];

                    // Read the current cluster data into the cluster buffer.
                    this.blockDriver.Read(file.blocks[firstBlockId], 0, this.cluster, 0, this.blockDriver.ClusterSize);

                    // Update the cluster buffer to prepare it for the new cluster.
                    // The new cluster will be written as a pending cluster.
                    this.cluster.SetMarker(BlockMarkers.PendingCluster);
                    this.cluster.SetDataLength(dataOffset);
                    this.cluster.MaxWrite = this.cluster.GetDataStartOffset() + dataOffset;

                    // Write the cluster to the log and get the new cluster id
                    var newClusterId = this.WriteToLog(this.cluster, 0, this.cluster.MaxWrite);

                    // Mark the old cluster as orphaned
                    this.MarkClusterOrphaned(orphanedClusterId);

                    // Mark the new cluster as the allocated cluster
                    this.MarkClusterAllocated(newClusterId);

                    // Update the in-memory file reference of the file block to point
                    // to the new cluster.
                    file.blocks[firstBlockId] = newClusterId;

                    // Update what we consider the first block to be dropped.
                    // The current first block has already had it's data truncated.
                    firstBlockId++;
                }

                // Loop through all the blocks of the file starting with the calculated first block
                // and mark each cluster as orphaned.
                for (int blockId = firstBlockId; blockId < file.blocks.Count; blockId++)
                {
                    this.MarkClusterOrphaned(file.blocks[blockId]);
                }

                // Remove the unused blocks from the in memory file reference.
                file.blocks.SetLength(firstBlockId);

                // Update the file size to reflect the new truncated size.
                file.fileSize = (int)filePosition;
            }
        }

        private void Delete(FileRef file)
        {
            if (file.openCount > 0) throw new IOException(StringTable.Error_FileIsInUse, (int)TinyFileSystemException.TfsErrorCode.FileInUse);

            try
            {
                // Loop through each block of the file and mark the
                // referenced clusters as deleted.
                for (var i = 0; i < file.blocks.Count; i++)
                {
                    this.MarkClusterOrphaned(file.blocks[i]);
                }
            }
            finally
            {
                // Remove the file from the in memory index.
                this.filesIndex.Remove(file.objId);
            }
        }

        private FileRef GetFileRef(string fileName)
        {
            // Retrieve a FileRef from the file index based
            // on the name of the file.
            var nameToSearch = fileName.ToUpper();
            foreach (FileRef f in this.filesIndex.Values)
            {
                if (nameToSearch == this.GetFileName(f))
                {
                    return f;
                }
            }
            return null;
        }

        private string GetFileName(FileRef file)
        {
            // Extract the file name from the first block of the file
            // which is a File Cluster.
            var clusterId = file.blocks[0];
            this.blockDriver.Read(clusterId, ClusterBuffer.FileNameLengthOffset, this.cluster, ClusterBuffer.FileNameLengthOffset, 2 + ClusterBuffer.MaxFileNameLength);
            //string NameTmp = _cluster.GetFileName();
            //Debug.Print("NameTmp = #" + NameTmp+"#");
            return this.cluster.GetFileName();
        }

        private ushort WriteToLog(byte[] data, int index, int count)
        {
            // Check if we need to perform a compaction to free up some space to write the new log entry      
            if (!this.compacting && this.freeClusterCount <= this.minFreeClusters)
                this.PartialCompact();

            if (!this.compacting && this.freeClusterCount <= this.minFreeClusters)
                throw new TinyFileSystemException(StringTable.Error_DiskFull, TinyFileSystemException.TfsErrorCode.DiskFull);

            var clusterId = this.WriteToLogInternal(data, index, count);

            this.freeClusterCount--;

            return clusterId;
        }

        private ushort WriteToLogInternal(byte[] data, int index, int count)
        {
            // store the current tail cluster id
            var clusterId = this.tailClusterId;

            this.WriteToCluster(clusterId, data, index, count);

            // Move the tail forward wrapping around to the begining once we reach the end
            // of the device.
            this.tailClusterId = (ushort)((this.tailClusterId + 1) % this.totalClusterCount);

            // return the cluster id that was written to.
            return clusterId;
        }

        private void WriteToCluster(ushort clusterId, byte[] data, int index, int count) => this.blockDriver.Write(clusterId, 0, data, index, count);

        private void PartialCompact()
        {
            // ReSharper disable once UnusedVariable
            var starTime = DateTime.Now;
            try
            {
                this.compacting = true;

                // While there are enough orphaned clusters to make compaction worth while and we
                // have less than the threshold of free clusters, keep compacting the sectors.
                while (this.freeClusterCount <= this.minFreeClusters && this.orphanedClusterCount >= this.clustersPerSector)
                {
                    // Find the best sector to migrate, the one that will reclaim the most free space          
                    var sectorId = this.GetSectorToCompact();

                    // Migrate the active data from the sector to tail of the log
                    if (!this.MigrateSector(sectorId))
                        break;

                    // If it was not already the head sector, migrate the head sector into
                    // the newly freed sector.
                    if (sectorId != this.headSectorId)
                    {
                        if (!this.MigrateSector(this.headSectorId, (ushort)(sectorId * this.clustersPerSector)))
                            break;
                    }

                    // Move the head sector forward
                    this.headSectorId = (ushort)((this.headSectorId + 1) % this.totalSectorCount);
                }
            }
            finally
            {
#if DEBUG
                Debug.WriteLine("Partial Compact: " + ((DateTime.Now - starTime).Ticks / TimeSpan.TicksPerSecond).ToString());
#endif
                this.compacting = false;
            }
        }

        private ushort GetSectorToCompact()
        {
            // If the head sector has any orphaned data then we should migrate it directly
            if (this.orphanedPerSector[this.headSectorId] > 0) return this.headSectorId;

            // Locate the sector with the most orphaned data
            var bestSector = this.headSectorId;
            var tailSector = (ushort)(this.tailClusterId / this.clustersPerSector);
            byte mostOrphaned = 0;
            for (ushort i = 0; i < this.totalSectorCount; i++)
            {
                var orphanedCount = this.orphanedPerSector[i];
                if (orphanedCount > mostOrphaned && i != tailSector)
                {
                    bestSector = i;
                    mostOrphaned = orphanedCount;
                }
            }
            return bestSector;
        }

        private bool MigrateSector(ushort fromSectorId, ushort toStartClusterId = ushort.MaxValue)
        {
            var toClusterId = toStartClusterId;

            if (toStartClusterId == ushort.MaxValue) toClusterId = this.tailClusterId;

            // Make sure we are not trying to migrate from/to the same sector.
            var toSectorId = toClusterId / this.clustersPerSector;
            if (fromSectorId == toSectorId) return false;

            // calculate the first cluster of the specified sector.
            var firstClusterId = (ushort)((fromSectorId * this.blockDriver.SectorSize) / this.blockDriver.ClusterSize);

            ushort freedClusterCount = 0;

            // Loop through each cluster in the sector and move the allocated clusters to the tail leaving
            // all the orphaned clusters behind. The will leave the original sector will all inactive cluster
            // allowing us to format the sector and move the head forward. This frees the sector to be used for
            // future writes.
            for (ushort i = 0, clusterId = firstClusterId; i < this.clustersPerSector; i++, clusterId++)
            {
                // Clear the defragmentation buffer
                this.defragBuffer.Clear();

                // Read the first byte from the cluster, this is always the marker byte
                this.blockDriver.Read(clusterId, 0, this.defragBuffer, 0, 1);

                // Get the current state of the cluster from the marker byte
                var marker = this.defragBuffer.GetMarker();

                // Check if this is an active allocated cluster
                if (marker == BlockMarkers.AllocatedCluster)
                {
                    // Read the rest of the header
                    this.blockDriver.Read(clusterId, 1, this.defragBuffer, 1, ClusterBuffer.DataClusterHeaderSize);
                    var blockId = this.defragBuffer.GetBlockId();
                    var dataLength = this.defragBuffer.GetDataLength();

                    if (blockId == 0)
                    {
                        this.blockDriver.Read(clusterId, ClusterBuffer.DataClusterHeaderSize, this.defragBuffer, ClusterBuffer.DataClusterHeaderSize,
                          (ClusterBuffer.FileClusterHeaderSize - ClusterBuffer.DataClusterHeaderSize) + dataLength);
                        this.defragBuffer.MaxWrite = ClusterBuffer.FileClusterHeaderSize + dataLength;
                    }
                    else
                    {
                        this.blockDriver.Read(clusterId, ClusterBuffer.DataClusterHeaderSize, this.defragBuffer, ClusterBuffer.DataClusterHeaderSize, dataLength);
                        this.defragBuffer.MaxWrite = ClusterBuffer.DataClusterHeaderSize + dataLength;
                    }

                    // Write the defrag cluster to the tail of the log          
                    this.WriteToCluster(toClusterId, this.defragBuffer, 0, this.defragBuffer.MaxWrite);

                    // Update any in memory file indexes with the new cluster id for the block of the file.
                    var f = (FileRef)this.filesIndex[this.defragBuffer.GetObjId()];
                    if (f != null)
                    {
                        f.blocks[this.defragBuffer.GetBlockId()] = toClusterId;
                    }
                    else
                    {
                        this.MarkClusterOrphaned(toClusterId);
                        freedClusterCount++;
                        this.orphanedClusterCount++;
                    }

                    toClusterId = (ushort)((toClusterId + 1) % this.totalClusterCount);
                }
                else if (marker != BlockMarkers.FormattedSector && marker != BlockMarkers.ErasedSector)
                {
                    // Count the number of clusters that are not active. This will be the number of cluster
                    // that this mugration of the sector has managed to free to re-use.
                    freedClusterCount++;
                }
            }

            // If we where writing to the tail of the log then we need to update the tail to
            // point to the new tail after the writes have completed.
            if (toStartClusterId == ushort.MaxValue) this.tailClusterId = toClusterId;

            // Once the active data has been migrated we can erase the current sector
            this.blockDriver.EraseSector(fromSectorId);
            this.orphanedPerSector[fromSectorId] = 0;

            // Write the marker to the sectors first byte indicating that this is a formatted sector
            this.blockDriver.Write(firstClusterId, 0, BlockMarkers.FormattedSectorBytes, 0, 1);

            // Update the free cluster and orphaned cluster counters
            this.freeClusterCount += freedClusterCount;
            this.orphanedClusterCount -= freedClusterCount;
            Debug.Assert(this.orphanedClusterCount < this.totalClusterCount);

            return true;
        }

        private void MarkClusterAllocated(ushort clusterId) =>
            // Write the allocated marker to the cluster
            this.blockDriver.Write(clusterId, 0, BlockMarkers.AllocatedClusterBytes, 0, 1);

        private void MarkClusterOrphaned(ushort clusterId)
        {
            // Write the orphaned marker to the cluster
            this.blockDriver.Write(clusterId, 0, BlockMarkers.OrphanedClusterBytes, 0, 1);

            // Update the orphaned cluster counter.
            this.orphanedClusterCount++;
            this.orphanedPerSector[clusterId / this.clustersPerSector]++;
        }
    }
}
