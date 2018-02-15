using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.SPOT.IO {
    internal enum StorageEventType : byte {
        Invalid = 0,
        Insert = 1,
        Eject = 2,
    }

    internal class StorageEvent : BaseEvent {
        public StorageEventType EventType;
        public uint Handle;
        public DateTime Time;
    }

    internal class StorageEventProcessor : IEventProcessor {
        public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time) {
            var ev = new StorageEvent {
                EventType = (StorageEventType)(data1 & 0xFF),
                Handle = data2,
                Time = time
            };

            return ev;
        }
    }

    internal class StorageEventListener : IEventListener {
        public void InitializeForEventSource() {
        }

        public bool OnEvent(BaseEvent ev) {
            if (ev is StorageEvent) {
                RemovableMedia.PostEvent((StorageEvent)ev);
            }

            return true;
        }
    }

    public sealed class VolumeInfo {
        public readonly string Name;

        public readonly string VolumeLabel;
        public readonly uint VolumeID;

        public readonly string FileSystem;
        public readonly uint FileSystemFlags;

        public readonly uint DeviceFlags;
        public readonly uint SerialNumber;

        public readonly long TotalFreeSpace;
        public readonly long TotalSize;

        internal uint VolumePtr;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern VolumeInfo(string volumeName);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern VolumeInfo(uint volumePtr);

        // This is used internally to create a VolumeInfo for removable volumes that have been ejected
        internal VolumeInfo(VolumeInfo ejectedVolumeInfo) => this.Name = ejectedVolumeInfo.Name;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void Refresh();

        public string RootDirectory => "\\" + this.Name;

        public bool IsFormatted => this.FileSystem != null && this.TotalSize > 0;

        public void Format(uint parameter) => Format(this.FileSystem, parameter, this.VolumeLabel, false);

        public void Format(uint parameter, bool force) => Format(this.FileSystem, parameter, this.VolumeLabel, force);

        public void Format(string fileSystem, uint parameter) => Format(fileSystem, parameter, this.VolumeLabel, false);

        public void Format(string fileSystem, uint parameter, bool force) => Format(fileSystem, parameter, this.VolumeLabel, force);

        public void Format(string fileSystem, uint parameter, string volumeLabel, bool force) {
            var rootedNameSpace = "\\" + this.Name;

            var restoreCD = FileSystemManager.CurrentDirectory == rootedNameSpace;

            if (FileSystemManager.IsInDirectory(FileSystemManager.CurrentDirectory, rootedNameSpace)) {
                FileSystemManager.SetCurrentDirectory(NativeIO.FSRoot);
            }

            if (force) {
                FileSystemManager.ForceRemoveNameSpace(this.Name);
            }

            var record = FileSystemManager.LockDirectory(rootedNameSpace);

            try {
                NativeIO.Format(this.Name, fileSystem, volumeLabel, parameter);
                Refresh();
            }
            finally {
                FileSystemManager.UnlockDirectory(record);
            }

            if (restoreCD) {
                FileSystemManager.SetCurrentDirectory(rootedNameSpace);
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static VolumeInfo[] GetVolumes();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static string[] GetFileSystems();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void FlushAll();
    }

    public static class RemovableMedia {
        public static event InsertEventHandler Insert;
        public static event EjectEventHandler Eject;

        private static ArrayList _volumes;
        private static Queue _events;

        //--//

        static RemovableMedia() {
            try {
                Microsoft.SPOT.EventSink.AddEventProcessor(EventCategory.Storage, new StorageEventProcessor());
                Microsoft.SPOT.EventSink.AddEventListener(EventCategory.Storage, new StorageEventListener());

                _volumes = new ArrayList();
                _events = new Queue();


                MountRemovableVolumes();
            }
            catch {

            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static void MountRemovableVolumes();

        internal static void PostEvent(StorageEvent ev) {
            /// We are using timer to process events instead of a separate message loop
            /// thread, to keep it light weight.
            try {
                // create a time and push it in a queue to make sure GC does not eat it up before it fires
                // the timer will pop itself in the callback and GC will take it from there
                // be trasaction-safe and create and queue the timer before setting it up to fire
                var messagePseudoThread = new Timer(MessageHandler, ev, Timeout.Infinite, Timeout.Infinite);
                _events.Enqueue(messagePseudoThread);
                // now that all operation that can fail have been done, enable the timer to fire
                messagePseudoThread.Change(10, Timeout.Infinite);
            }
            catch // eat up all exceptions, nothing we can do
            {
            }
        }

        private static void MessageHandler(object args) {
            try {
                var ev = args as StorageEvent;

                if (ev == null)
                    return;

                lock (_volumes) {
                    if (ev.EventType == StorageEventType.Insert) {
                        var volume = new VolumeInfo(ev.Handle);

                        _volumes.Add(volume);

                        if (Insert != null) {
                            var mediaEventArgs = new MediaEventArgs(volume, ev.Time);

                            Insert(null, mediaEventArgs);
                        }
                    }
                    else if (ev.EventType == StorageEventType.Eject) {
                        var volumeInfo = RemoveVolume(ev.Handle);

                        if (volumeInfo != null) {
                            FileSystemManager.ForceRemoveNameSpace(volumeInfo.Name);

                            if (Eject != null) {
                                var mediaEventArgs = new MediaEventArgs(new VolumeInfo(volumeInfo), ev.Time);

                                Eject(null, mediaEventArgs);
                            }
                        }
                    }
                }
            }
            finally {
                // get rid of this timer
                _events.Dequeue();
            }
        }

        private static VolumeInfo RemoveVolume(uint handle) {
            VolumeInfo volumeInfo;
            var count = _volumes.Count;

            for (var i = 0; i < count; i++) {
                volumeInfo = ((VolumeInfo)_volumes[i]);
                if (volumeInfo.VolumePtr == handle) {
                    _volumes.RemoveAt(i);
                    return volumeInfo;
                }
            }

            return null;
        }
    }

    //--//

    public class MediaEventArgs {
        public readonly DateTime Time;
        public readonly VolumeInfo Volume;

        public MediaEventArgs(VolumeInfo volume, DateTime time) {
            this.Time = time;
            this.Volume = volume;
        }
    }

    public delegate void InsertEventHandler(object sender, MediaEventArgs e);
    public delegate void EjectEventHandler(object sender, MediaEventArgs e);
}


