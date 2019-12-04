using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Rtc.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Rtc {
    public enum BatteryChargeMode {
        None = 0,
        Fast = 1,
        Slow = 2
    }

    public sealed class RtcController : IDisposable {
        public IRtcControllerProvider Provider { get; }

        private RtcController(IRtcControllerProvider provider) => this.Provider = provider;

        public static RtcController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.RtcController) is RtcController c ? c : RtcController.FromName(NativeApi.GetDefaultName(NativeApiType.RtcController));
        public static RtcController FromName(string name) => RtcController.FromProvider(new RtcControllerApiWrapper(NativeApi.Find(name, NativeApiType.RtcController)));
        public static RtcController FromProvider(IRtcControllerProvider provider) => new RtcController(provider);

        public void Dispose() => this.Provider.Dispose();

        public bool IsValid => this.Provider.IsValid;

        public RtcDateTime GetTime() => this.IsValid ? this.Provider.GetTime() : throw new InvalidOperationException();
        public void SetTime(RtcDateTime value) => this.Provider.SetTime(value);

        public DateTime Now {
            get => this.GetTime().ToDateTime();
            set => this.SetTime(RtcDateTime.FromDateTime(value));
        }

        public uint BackupMemorySize => this.Provider.BackupMemorySize;

        public void WriteBackupMemory(byte[] sourceData) => this.WriteBackupMemory(sourceData, 0, 0, sourceData.Length);

        public void WriteBackupMemory(byte[] sourceData, uint destinationOffset) => this.WriteBackupMemory(sourceData, 0, destinationOffset, sourceData.Length);

        public void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count) {
            if (sourceData == null) throw new ArgumentNullException(nameof(sourceData));
            if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (sourceOffset + count > sourceData.Length) throw new ArgumentOutOfRangeException(nameof(count));
            if (destinationOffset + count > this.BackupMemorySize) throw new ArgumentOutOfRangeException(nameof(count));

            this.Provider.WriteBackupMemory(sourceData, sourceOffset, destinationOffset, count);
        }

        public int ReadBackupMemory(byte[] destinationData) => this.ReadBackupMemory(destinationData, 0, 0, destinationData.Length);

        public int ReadBackupMemory(byte[] destinationData, uint sourceOffset) => this.ReadBackupMemory(destinationData, 0, sourceOffset, destinationData.Length);

        public int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count) {
            if (destinationData == null) throw new ArgumentNullException(nameof(destinationData));
            if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (sourceOffset + count > this.BackupMemorySize) throw new ArgumentOutOfRangeException(nameof(count));
            if (destinationOffset + count > destinationData.Length) throw new ArgumentOutOfRangeException(nameof(count));

            return this.Provider.ReadBackupMemory(destinationData, destinationOffset, sourceOffset, count);
        }

        public void SetChargeMode(BatteryChargeMode chargeMode) => this.Provider.SetChargeMode(chargeMode);
    }

    public struct RtcDateTime {
        public int Year;
        public int Month;
        public int Week;
        public int DayOfYear;
        public int DayOfMonth;
        public int DayOfWeek;
        public int Hour;
        public int Minute;
        public int Second;
        public int Millisecond;
        public int Microsecond;
        public int Nanosecond;

        public DateTime ToDateTime() => new DateTime(this.Year, this.Month, this.DayOfMonth, this.Hour, this.Minute, this.Second, this.Millisecond).AddTicks((long)((TimeSpan.TicksPerMillisecond / 1_000.0) * this.Microsecond + (TimeSpan.TicksPerMillisecond / 1_000_000.0) * this.Nanosecond));

        public static RtcDateTime FromDateTime(DateTime value) {
            var dt = new RtcDateTime {
                Year = value.Year,
                Month = value.Month,
                Week = int.MaxValue,
                DayOfYear = value.DayOfYear,
                DayOfMonth = value.Day,
                DayOfWeek = (int)value.DayOfWeek,
                Hour = value.Hour,
                Minute = value.Minute,
                Second = value.Second,
                Millisecond = value.Millisecond
            };

            var remaining = (int)(value.TimeOfDay.Ticks % 10_000);

            dt.Microsecond = remaining / 10;
            dt.Nanosecond = (remaining % 10) * 100;

            return dt;
        }
    }

    namespace Provider {
        public interface IRtcControllerProvider : IDisposable {
            bool IsValid { get; }
            uint BackupMemorySize { get; }

            RtcDateTime GetTime();
            void SetTime(RtcDateTime value);
            void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count);
            int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count);
            void SetChargeMode(BatteryChargeMode chargeMode);
        }

        public sealed class RtcControllerApiWrapper : IRtcControllerProvider {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            public RtcControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern RtcDateTime GetTime();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetTime(RtcDateTime value);

            public extern bool IsValid { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count);

            public extern uint BackupMemorySize { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetChargeMode(BatteryChargeMode chargeMode);
        }
    }
}
