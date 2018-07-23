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
