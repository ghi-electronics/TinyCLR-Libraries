using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Rtc.Provider;
using GHIElectronics.TinyCLR.Native;

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
    /// backed by a coin cell, and exposes a small region of battery-backed RAM
    /// via <see cref="WriteBackupMemory(byte[])"/> / <see cref="ReadBackupMemory(byte[])"/>.
    /// </summary>
    public class RtcController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IRtcControllerProvider Provider { get; }

        private RtcController(IRtcControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default RTC controller for this device.</summary>
        public static RtcController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.RtcController) is RtcController c ? c : RtcController.FromName(NativeApi.GetDefaultName(NativeApiType.RtcController));
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
        /// <summary>True when the RTC is clocked from the internal RC oscillator rather than an external crystal.</summary>
        public bool InternalRC => this.Provider.InternalRC;

        /// <summary>Returns the current RTC time.</summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="IsValid"/> is false.</exception>
        public RtcDateTime GetTime() => this.IsValid ? this.Provider.GetTime() : throw new InvalidOperationException();

        /// <summary>Sets the RTC time. After a successful call, <see cref="IsValid"/> becomes true.</summary>
        /// <param name="value">Calendar time to write.</param>
        public void SetTime(RtcDateTime value) => this.Provider.SetTime(value);

        /// <summary>Convenience accessor that returns/accepts a managed <see cref="DateTime"/>.</summary>
        public DateTime Now {
            get => this.GetTime().ToDateTime();
            set => this.SetTime(RtcDateTime.FromDateTime(value));
        }

        /// <summary>Size in bytes of the battery-backed memory region.</summary>
        public uint BackupMemorySize => this.Provider.BackupMemorySize;

        /// <summary>Writes the entire <paramref name="sourceData"/> array to backup memory starting at offset 0.</summary>
        /// <param name="sourceData">Bytes to write.</param>
        public void WriteBackupMemory(byte[] sourceData) => this.WriteBackupMemory(sourceData, 0, 0, sourceData.Length);

        /// <summary>Writes the entire <paramref name="sourceData"/> array to backup memory at <paramref name="destinationOffset"/>.</summary>
        /// <param name="sourceData">Bytes to write.</param>
        /// <param name="destinationOffset">Offset in backup memory where the write begins.</param>
        public void WriteBackupMemory(byte[] sourceData, uint destinationOffset) => this.WriteBackupMemory(sourceData, 0, destinationOffset, sourceData.Length);

        /// <summary>Writes a slice of <paramref name="sourceData"/> to backup memory.</summary>
        /// <param name="sourceData">Source buffer.</param>
        /// <param name="sourceOffset">Starting offset within <paramref name="sourceData"/>.</param>
        /// <param name="destinationOffset">Starting offset in backup memory.</param>
        /// <param name="count">Number of bytes to write.</param>
        public void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count) {
            if (sourceData == null) throw new ArgumentNullException(nameof(sourceData));
            if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (sourceOffset + count > sourceData.Length) throw new ArgumentOutOfRangeException(nameof(count));
            if (destinationOffset + count > this.BackupMemorySize) throw new ArgumentOutOfRangeException(nameof(count));

            this.Provider.WriteBackupMemory(sourceData, sourceOffset, destinationOffset, count);
        }

        /// <summary>Reads <paramref name="destinationData"/>.Length bytes from backup memory starting at offset 0.</summary>
        /// <param name="destinationData">Destination buffer.</param>
        /// <returns>Number of bytes read.</returns>
        public int ReadBackupMemory(byte[] destinationData) => this.ReadBackupMemory(destinationData, 0, 0, destinationData.Length);

        /// <summary>Reads <paramref name="destinationData"/>.Length bytes from backup memory starting at <paramref name="sourceOffset"/>.</summary>
        /// <param name="destinationData">Destination buffer.</param>
        /// <param name="sourceOffset">Starting offset in backup memory.</param>
        /// <returns>Number of bytes read.</returns>
        public int ReadBackupMemory(byte[] destinationData, uint sourceOffset) => this.ReadBackupMemory(destinationData, 0, sourceOffset, destinationData.Length);

        /// <summary>Reads a slice from backup memory into <paramref name="destinationData"/>.</summary>
        /// <param name="destinationData">Destination buffer.</param>
        /// <param name="destinationOffset">Starting offset within <paramref name="destinationData"/>.</param>
        /// <param name="sourceOffset">Starting offset in backup memory.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Number of bytes read.</returns>
        public int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count) {
            if (destinationData == null) throw new ArgumentNullException(nameof(destinationData));
            if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (sourceOffset + count > this.BackupMemorySize) throw new ArgumentOutOfRangeException(nameof(count));
            if (destinationOffset + count > destinationData.Length) throw new ArgumentOutOfRangeException(nameof(count));

            return this.Provider.ReadBackupMemory(destinationData, destinationOffset, sourceOffset, count);
        }

        /// <summary>Configures the backup-battery trickle charger.</summary>
        /// <param name="chargeMode">Charging policy.</param>
        public void SetChargeMode(BatteryChargeMode chargeMode) => this.Provider.SetChargeMode(chargeMode);

        /// <summary>Applies a frequency calibration pulse to compensate crystal drift.</summary>
        /// <param name="pulse">Platform-specific calibration value.</param>
        public void Calibrate(int pulse) => this.Provider.Calibrate(pulse);
    }

    /// <summary>
    /// RTC-native calendar time representation. Mirrors the underlying hardware
    /// registers — most app code should round-trip through <see cref="ToDateTime"/>
    /// / <see cref="FromDateTime(DateTime)"/> rather than touching fields directly.
    /// </summary>
    public struct RtcDateTime {
        /// <summary>Calendar year (e.g. 2026).</summary>
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

        /// <summary>Converts this <see cref="RtcDateTime"/> to a managed <see cref="DateTime"/>.</summary>
        public DateTime ToDateTime() => new DateTime(this.Year, this.Month, this.DayOfMonth, this.Hour, this.Minute, this.Second, this.Millisecond).AddTicks((long)((TimeSpan.TicksPerMillisecond / 1_000.0) * this.Microsecond + (TimeSpan.TicksPerMillisecond / 1_000_000.0) * this.Nanosecond));

        /// <summary>Builds an <see cref="RtcDateTime"/> from a managed <see cref="DateTime"/>.</summary>
        /// <param name="value">Calendar time to convert.</param>
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
            /// <summary>True when the RTC has been initialized with a valid time at least once.</summary>
            bool IsValid { get; }

            /// <summary>True when clocked from the internal RC oscillator.</summary>
            bool InternalRC { get; }
            /// <summary>Size in bytes of the battery-backed memory region.</summary>
            uint BackupMemorySize { get; }

            /// <summary>Reads the current calendar time.</summary>
            RtcDateTime GetTime();
            /// <summary>Sets the RTC time.</summary>
            /// <param name="value">Calendar time to write.</param>
            void SetTime(RtcDateTime value);
            /// <summary>Writes a slice of bytes to backup memory.</summary>
            /// <param name="sourceData">Source buffer.</param>
            /// <param name="sourceOffset">Starting offset within <paramref name="sourceData"/>.</param>
            /// <param name="destinationOffset">Starting offset in backup memory.</param>
            /// <param name="count">Number of bytes to write.</param>
            void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count);
            /// <summary>Reads a slice from backup memory.</summary>
            /// <param name="destinationData">Destination buffer.</param>
            /// <param name="destinationOffset">Starting offset within <paramref name="destinationData"/>.</param>
            /// <param name="sourceOffset">Starting offset in backup memory.</param>
            /// <param name="count">Number of bytes to read.</param>
            /// <returns>Number of bytes read.</returns>
            int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count);
            /// <summary>Configures the backup-battery trickle charger.</summary>
            /// <param name="chargeMode">Charging policy.</param>
            void SetChargeMode(BatteryChargeMode chargeMode);
            /// <summary>Applies a frequency calibration pulse.</summary>
            /// <param name="pulse">Platform-specific calibration value.</param>
            void Calibrate(int pulse);
        }

        /// <summary>
        /// Concrete <see cref="IRtcControllerProvider"/> backed by the native
        /// TinyCLR RTC HAL.
        /// </summary>
        public sealed class RtcControllerApiWrapper : IRtcControllerProvider {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            /// <param name="api">The native RTC API to bind to.</param>
            public RtcControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern RtcDateTime GetTime();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetTime(RtcDateTime value);

            /// <inheritdoc/>
            public extern bool IsValid { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            public extern bool InternalRC { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void WriteBackupMemory(byte[] sourceData, uint sourceOffset, uint destinationOffset, int count);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int ReadBackupMemory(byte[] destinationData, uint destinationOffset, uint sourceOffset, int count);

            /// <inheritdoc/>
            public extern uint BackupMemorySize { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetChargeMode(BatteryChargeMode chargeMode);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Calibrate(int pulse);
        }
    }
}
