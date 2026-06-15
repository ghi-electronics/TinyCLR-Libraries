using System.Collections;
using GHIElectronics.TinyCLR.IO;

namespace System.IO {
    /// <summary>Classification of a mounted volume.</summary>
    public enum DriveType {
        /// <summary>The drive type is unknown.</summary>
        Unknown = 0,
        /// <summary>The drive has no root directory.</summary>
        NoRootDirectory = 1,
        /// <summary>The drive is removable, such as an SD card or USB stick.</summary>
        Removable = 2,
        /// <summary>The drive is a fixed disk.</summary>
        Fixed = 3,
        /// <summary>The drive is a network drive.</summary>
        Network = 4,
        /// <summary>The drive is an optical disc.</summary>
        CDRom = 5,
        /// <summary>The drive is a RAM disk.</summary>
        Ram = 6
    }

    /// <summary>Information about a mounted volume — total/available space, type, root path.</summary>
    public sealed class DriveInfo {
        private static readonly Hashtable driveProviders = new Hashtable();
        private static Stack driveNames;

        private readonly IDriveProvider provider;

        /// <summary>The root name of the drive.</summary>
        public string Name { get; }
        /// <summary>The root directory of the drive.</summary>
        public DirectoryInfo RootDirectory => new DirectoryInfo(this.Name);

        /// <summary>The type of the drive.</summary>
        public DriveType DriveType => this.provider.DriveType;
        /// <summary>The name of the file system format on the drive.</summary>
        public string DriveFormat => this.provider.DriveFormat;
        /// <summary>Whether the drive is ready for access.</summary>
        public bool IsReady => this.provider.IsReady;
        /// <summary>The amount of free space available on the drive in bytes.</summary>
        public long AvailableFreeSpace => this.provider.AvailableFreeSpace;
        /// <summary>The total free space on the drive in bytes.</summary>
        public long TotalFreeSpace => this.provider.TotalFreeSpace;
        /// <summary>The total size of the drive in bytes.</summary>
        public long TotalSize => this.provider.TotalSize;
        /// <summary>The volume label of the drive.</summary>
        public string VolumeLabel => this.provider.VolumeLabel;

        /// <summary>Creates a new instance for the registered drive with the given root name.</summary>
        public DriveInfo(string driveName) {
            lock (DriveInfo.driveProviders) {
                if (!DriveInfo.driveProviders.Contains(driveName)) throw new ArgumentException();

                this.provider = (IDriveProvider)DriveInfo.driveProviders[driveName];

                this.Name = driveName;
            }
        }

        /// <summary>Returns information about all mounted drives.</summary>
        public static DriveInfo[] GetDrives() {
            var drives = Directory.GetLogicalDrives();
            var di = new DriveInfo[drives.Length];

            for (var i = 0; i < drives.Length; i++)
                di[i] = new DriveInfo(drives[i]);

            return di;
        }

        /// <summary>Assigns the next free root name to the provider and registers it as a drive.</summary>
        public static IDriveProvider RegisterDriveProvider(IDriveProvider provider) {
            if (provider == null) throw new ArgumentNullException();

            var root = string.Empty;

            lock (DriveInfo.driveProviders) {
                if (DriveInfo.driveNames == null) {
                    var s = new Stack();

                    for (var i = 'Z'; i >= 'A'; i--)
                        s.Push(i + ":\\");

                    DriveInfo.driveNames = s;
                }

                root = (string)DriveInfo.driveNames.Pop();

                DriveInfo.driveProviders.Add(root, provider);
            }

            provider.Initialize(root);

            return provider;
        }

        /// <summary>Unregisters the provider and frees its root name.</summary>
        public static void DeregisterDriveProvider(IDriveProvider provider) {
            if (provider == null) throw new ArgumentNullException();

            lock (DriveInfo.driveProviders) {
                var n = default(string);

                foreach (DictionaryEntry p in DriveInfo.driveProviders) {
                    if (p.Value == provider) {
                        n = (string)p.Key;
                        break;
                    }
                }

                if (n == null) throw new ArgumentException();

                DriveInfo.driveProviders.Remove(n);
                DriveInfo.driveNames.Push(n);
            }
        }

        internal static string[] GetLogicalDrives() {
            lock (DriveInfo.driveProviders) {
                var drives = new string[DriveInfo.driveProviders.Count];

                var i = 0;
                foreach (DictionaryEntry p in DriveInfo.driveProviders)
                    drives[i++] = (string)p.Key;

                return drives;
            }
        }

        internal static IDriveProvider GetForPath(string path) {
            var root = Path.GetPathRoot(path);

            lock (DriveInfo.driveProviders)
                foreach (DictionaryEntry p in DriveInfo.driveProviders)
                    if (p.Value is IDriveProvider d && (string)p.Key == root)
                        return d;

            return null;
        }
    }
}
