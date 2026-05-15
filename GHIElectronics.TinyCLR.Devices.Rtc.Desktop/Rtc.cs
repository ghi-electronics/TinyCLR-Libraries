using System;
using GHIElectronics.TinyCLR.Devices.Rtc.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Rtc\Rtc.cs.
// Bodies on Desktop are safe no-ops:
//   * IsValid -> true so user code that gates GetTime() on IsValid proceeds
//   * GetTime returns the host's current time (no surprise with DateTime.Now flow)
//   * SetTime stored only in field; backup memory backed by a byte[]
namespace GHIElectronics.TinyCLR.Devices.Rtc {
    /// <summary>Trickle-charge setting applied to the RTC backup battery.</summary>
    public enum BatteryChargeMode {
        /// <summary>Charging disabled.</summary>
        None = 0,
        /// <summary>Higher charge current.</summary>
        Fast = 1,
        /// <summary>Lower charge current.</summary>
        Slow = 2
    }

    /// <summary>
    /// Real-time clock controller. Tracks calendar time across power cycles when
    /// backed by a coin cell, and exposes a small region of battery-backed RAM.
    /// </summary>
    public class RtcController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IRtcControllerProvider Provider { get; }

        private RtcController(IRtcControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default RTC controller for this device.</summary>
        public static RtcController GetDefault() => RtcController.FromName("Simulator");
        /// <summary>Returns an RTC controller identified by its native API name.</summary>
        /// <param name="name">Native API name.</param>
        public static RtcController FromName(string name) => RtcController.FromProvider(new RtcControllerApiWrapper(NativeApi.Find(name, NativeApiType.RtcController)));
        /// <summary>Creates a controller from a custom <see cref="IRtcControllerProvider"/>.</summary>
        /// <param name="provider">Provider implementing the clock operations.</param>
        public static RtcController FromProvider(IRtcControllerProvider provider) => new RtcController(provider);

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>True when the RTC has been initialized with a valid time at least once.</summary>
        public bool IsValid => this.Provider.IsValid;
        /// <summary>True when the RTC is clocked from the internal RC oscillator.</summary>
        public bool InternalRC => this.Provider.InternalRC;
        /// <summary>Returns the current RTC time.</summary>
        public RtcDateTime GetTime() => this.IsValid ? this.Provider.GetTime() : throw new InvalidOperationException();
        /// <summary>Sets the RTC time.</summary>
        public void SetTime(RtcDateTime value) => this.Provider.SetTime(value);

        /// <summary>Convenience accessor that returns/accepts a managed <see cref="DateTime"/>.</summary>
        public DateTime Now {
            get => this.GetTime().ToDateTime();
            set => this.SetTime(RtcDateTime.FromDateTime(value));
        }

        /// <summary>Size in bytes of the battery-backed memory region.</summary>
        public uint BackupMemorySize => this.Provider.BackupMemorySize;

        /// <summary>Writes the entire array to backup memory starting at offset 0.</summary>
        public void WriteBackupMemory(byte[] sourceData) => this.WriteBackupMemory(sourceData, 0, 0, sourceData.Length);
        /// <summary>Writes the entire array to backup memory at <paramref name="destinationOffset"/>.</summary>
        public void WriteBackupMemory(byte[] sourceData, uint destinationOffset) => this.WriteBackupMemory(sourceData, 0, destinationOffset, sourceData.Length);

        /// <summary>Writes a slice of <paramref name="sourceData"/> to backup memory.</summary>
        public void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count) {
            if (sourceData == null) throw new ArgumentNullException(nameof(sourceData));
            if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (sourceOffset + count > sourceData.Length) throw new ArgumentOutOfRangeException(nameof(count));
            if (destinationOffset + count > this.BackupMemorySize) throw new ArgumentOutOfRangeException(nameof(count));

            this.Provider.WriteBackupMemory(sourceData, sourceOffset, destinationOffset, count);
        }

        /// <summary>Reads up to destinationData.Length bytes from backup memory.</summary>
        public int ReadBackupMemory(byte[] destinationData) => this.ReadBackupMemory(destinationData, 0, 0, destinationData.Length);
        /// <summary>Reads up to destinationData.Length bytes starting at <paramref name="sourceOffset"/>.</summary>
        public int ReadBackupMemory(byte[] destinationData, uint sourceOffset) => this.ReadBackupMemory(destinationData, 0, sourceOffset, destinationData.Length);

        /// <summary>Reads a slice from backup memory.</summary>
        public int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count) {
            if (destinationData == null) throw new ArgumentNullException(nameof(destinationData));
            if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (sourceOffset + count > this.BackupMemorySize) throw new ArgumentOutOfRangeException(nameof(count));
            if (destinationOffset + count > destinationData.Length) throw new ArgumentOutOfRangeException(nameof(count));

            return this.Provider.ReadBackupMemory(destinationData, destinationOffset, sourceOffset, count);
        }

        /// <summary>Configures the backup-battery trickle charger.</summary>
        public void SetChargeMode(BatteryChargeMode chargeMode) => this.Provider.SetChargeMode(chargeMode);
        /// <summary>Applies a frequency calibration pulse.</summary>
        public void Calibrate(int pulse) => this.Provider.Calibrate(pulse);
    }

    /// <summary>RTC-native calendar time representation.</summary>
    public struct RtcDateTime {
        /// <summary>Calendar year.</summary>
        public int Year;
        /// <summary>Month, 1..12.</summary>
        public int Month;
        /// <summary>ISO week-of-year (platform-dependent; may be unset).</summary>
        public int Week;
        /// <summary>Day-of-year, 1..366.</summary>
        public int DayOfYear;
        /// <summary>Day-of-month, 1..31.</summary>
        public int DayOfMonth;
        /// <summary>Day-of-week (0 = Sunday).</summary>
        public int DayOfWeek;
        /// <summary>Hour, 0..23.</summary>
        public int Hour;
        /// <summary>Minute, 0..59.</summary>
        public int Minute;
        /// <summary>Second, 0..59.</summary>
        public int Second;
        /// <summary>Millisecond, 0..999.</summary>
        public int Millisecond;
        /// <summary>Microsecond component, 0..999.</summary>
        public int Microsecond;
        /// <summary>Nanosecond component (rounded to 100 ns), 0..900.</summary>
        public int Nanosecond;

        /// <summary>Converts to a managed <see cref="DateTime"/>.</summary>
        public DateTime ToDateTime() => new DateTime(this.Year, this.Month, this.DayOfMonth, this.Hour, this.Minute, this.Second, this.Millisecond).AddTicks((long)((TimeSpan.TicksPerMillisecond / 1_000.0) * this.Microsecond + (TimeSpan.TicksPerMillisecond / 1_000_000.0) * this.Nanosecond));

        /// <summary>Builds an <see cref="RtcDateTime"/> from a managed <see cref="DateTime"/>.</summary>
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
        /// <summary>Provider contract for an RTC controller.</summary>
        public interface IRtcControllerProvider : IDisposable {
            /// <summary>True when the RTC has been initialized with a valid time.</summary>
            bool IsValid { get; }
            /// <summary>True when clocked from the internal RC oscillator.</summary>
            bool InternalRC { get; }
            /// <summary>Size in bytes of the battery-backed memory region.</summary>
            uint BackupMemorySize { get; }

            /// <summary>Reads the current calendar time.</summary>
            RtcDateTime GetTime();
            /// <summary>Sets the RTC time.</summary>
            void SetTime(RtcDateTime value);
            /// <summary>Writes a slice of bytes to backup memory.</summary>
            void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count);
            /// <summary>Reads a slice from backup memory.</summary>
            int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count);
            /// <summary>Configures the backup-battery trickle charger.</summary>
            void SetChargeMode(BatteryChargeMode chargeMode);
            /// <summary>Applies a frequency calibration pulse.</summary>
            void Calibrate(int pulse);
        }

        /// <summary>Concrete <see cref="IRtcControllerProvider"/> backed by the native TinyCLR RTC HAL.</summary>
        public sealed class RtcControllerApiWrapper : IRtcControllerProvider {
            private readonly byte[] backup = new byte[4096];
            private RtcDateTime currentTime;
            private bool timeSet;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            public RtcControllerApiWrapper(NativeApi api) => this.Api = api;

            /// <summary>Releases the native controller.</summary>
            public void Dispose() { }

            /// <inheritdoc/>
            public bool IsValid => true;
            /// <inheritdoc/>
            public bool InternalRC => false;
            /// <inheritdoc/>
            public uint BackupMemorySize => (uint)this.backup.Length;

            /// <inheritdoc/>
            public RtcDateTime GetTime() => this.timeSet ? this.currentTime : RtcDateTime.FromDateTime(DateTime.Now);

            /// <inheritdoc/>
            public void SetTime(RtcDateTime value) {
                this.currentTime = value;
                this.timeSet = true;
            }

            /// <inheritdoc/>
            public void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count) =>
                Array.Copy(sourceData, (int)sourceOffset, this.backup, (int)destinationOffset, count);

            /// <inheritdoc/>
            public int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count) {
                Array.Copy(this.backup, (int)sourceOffset, destinationData, (int)destinationOffset, count);
                return count;
            }

            /// <inheritdoc/>
            public void SetChargeMode(BatteryChargeMode chargeMode) { }
            /// <inheritdoc/>
            public void Calibrate(int pulse) { }
        }
    }
}
