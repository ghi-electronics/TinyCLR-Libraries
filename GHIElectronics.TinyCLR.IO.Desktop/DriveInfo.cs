using System.Collections;
using GHIElectronics.TinyCLR.IO;

namespace System.IO {
    public enum DriveType {
        Unknown = 0,
        NoRootDirectory = 1,
        Removable = 2,
        Fixed = 3,
        Network = 4,
        CDRom = 5,
        Ram = 6
    }

    public sealed class DriveInfo {
        private static readonly Hashtable driveProviders = new Hashtable();
        private static Stack driveNames;

        private readonly IDriveProvider provider;

        public string Name { get; }
        public DirectoryInfo RootDirectory => new DirectoryInfo(this.Name);

        public DriveType DriveType => this.provider.DriveType;
        public string DriveFormat => this.provider.DriveFormat;
        public bool IsReady => this.provider.IsReady;
        public long AvailableFreeSpace => this.provider.AvailableFreeSpace;
        public long TotalFreeSpace => this.provider.TotalFreeSpace;
        public long TotalSize => this.provider.TotalSize;
        public string VolumeLabel => this.provider.VolumeLabel;

        public DriveInfo(string driveName) {
            lock (DriveInfo.driveProviders) {
                if (!DriveInfo.driveProviders.Contains(driveName)) throw new ArgumentException();

                this.provider = (IDriveProvider)DriveInfo.driveProviders[driveName];

                this.Name = driveName;
            }
        }

        public static DriveInfo[] GetDrives() {
            var drives = Directory.GetLogicalDrives();
            var di = new DriveInfo[drives.Length];

            for (var i = 0; i < drives.Length; i++)
                di[i] = new DriveInfo(drives[i]);

            return di;
        }

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
