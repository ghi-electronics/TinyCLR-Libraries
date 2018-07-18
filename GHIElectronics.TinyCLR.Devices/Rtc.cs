using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Rtc.Provider;

namespace GHIElectronics.TinyCLR.Devices.Rtc {
    public sealed class RtcController : IDisposable {
        public IRtcControllerProvider Provider { get; }

        private RtcController(IRtcControllerProvider provider) => this.Provider = provider;

        public static RtcController GetDefault() => Api.GetDefaultFromCreator(ApiType.RtcController) is RtcController c ? c : RtcController.FromName(Api.GetDefaultName(ApiType.RtcController));
        public static RtcController FromName(string name) => RtcController.FromProvider(new RtcControllerApiWrapper(Api.Find(name, ApiType.RtcController)));
        public static RtcController FromProvider(IRtcControllerProvider provider) => new RtcController(provider);

        public void Dispose() => this.Provider.Dispose();

        public RtcDateTime GetTime() => this.Provider.GetTime();
        public void SetTime(RtcDateTime value) => this.Provider.SetTime(value);
    }

    public struct RtcDateTime {
        public uint Year;
        public uint Month;
        public uint Week;
        public uint DayOfYear;
        public uint DayOfMonth;
        public uint DayOfWeek;
        public uint Hour;
        public uint Minute;
        public uint Second;
        public uint Millisecond;
        public uint Microsecond;
        public uint Nanosecond;

        public DateTime ToDateTime() => new DateTime((int)this.Year, (int)this.Month, (int)this.DayOfMonth, (int)this.Hour, (int)this.Minute, (int)this.Second, (int)this.Millisecond).AddTicks((long)((TimeSpan.TicksPerMillisecond / 1_000.0) * this.Microsecond + (TimeSpan.TicksPerMillisecond / 1_000_000.0) * this.Nanosecond));

        public static RtcDateTime FromDateTime(DateTime value) {
            var dt = new RtcDateTime {
                Year = (uint)value.Year,
                Month = (uint)value.Month,
                Week = uint.MaxValue,
                DayOfYear = (uint)value.DayOfYear,
                DayOfMonth = (uint)value.Day,
                DayOfWeek = (uint)value.DayOfWeek,
                Hour = (uint)value.Hour,
                Minute = (uint)value.Minute,
                Second = (uint)value.Second,
                Millisecond = (uint)value.Millisecond
            };

            var remaining = (uint)(value.TimeOfDay.Ticks % 10_000);

            dt.Microsecond = remaining / 10;
            dt.Nanosecond = (remaining % 10) * 100;

            return dt;
        }
    }

    namespace Provider {
        public interface IRtcControllerProvider : IDisposable {
            RtcDateTime GetTime();
            void SetTime(RtcDateTime value);
        }

        public sealed class RtcControllerApiWrapper : IRtcControllerProvider {
            private readonly IntPtr impl;

            public Api Api { get; }

            public RtcControllerApiWrapper(Api api) {
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
        }
    }
}
