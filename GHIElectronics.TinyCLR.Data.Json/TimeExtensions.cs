using System;

namespace GHIElectronics.TinyCLR.Data.Json
{
	public static class DateTimeExtensions
	{
		public static DateTime FromiCalendar(string iCalendar)
		{
			DateTime dt = DateTime.MinValue;
			string result = string.Empty;

			if ((iCalendar.Contains("DTSTART")) || (iCalendar.Contains("DTEND")))
			{
				result = iCalendar.Split(':')[1];
			}
			else
			{
				result = iCalendar;
			}

			// Check to see if format contains the timezone ID, or contains UTC reference
			// Neither means it's localtime
			bool tzid = iCalendar.Contains("TZID");
			bool utc = iCalendar.EndsWith("Z");

			string tzName = string.Empty;
			string time = string.Empty;
			if (tzid)
			{
				string[] parts = iCalendar.Split(new char[] { ';', '=', ':' });

				// parts[0] == DTSTART or DTEND
				// parts[1] == TZID
				// parts[2] == the timezone string
				// parts[3] == localtime string
				tzName = parts[2];
				time = parts[3];
			}
			else if (utc)
			{
				time = result.Substring(0, result.Length - 1);  // truncate the trailing 'Z'
			}
			else
			{
				time = result;  // localtime
			}

			// We now have the time string to parse, and we'll adjust
			// to UTC or timezone after parsing
			string year = time.Substring(0, 4);
			string month = time.Substring(4, 2);
			string day = time.Substring(6, 2);
			string hour = time.Substring(9, 2);
			string minute = time.Substring(11, 2);
			string second = time.Substring(13, 2);

			dt = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day), Convert.ToInt32(hour), Convert.ToInt32(minute), Convert.ToInt32(second));
			if (utc)
			{
				// Convert the Kind to DateTimeKind.Utc
				dt = new DateTime(0, DateTimeKind.Utc).AddTicks(dt.Ticks);
			}
			else if (tzid)
			{
				// not sure what to do here
			}

			return dt;
		}

		public static DateTime FromIso8601(string date)
		{
			string result = string.Empty;

			// Check to see if format contains the timezone ID, or contains UTC reference
			// Neither means it's localtime
			bool utc = date.EndsWith("Z");

			string[] parts = date.Split(new char[] { 'T', 'Z', ':', '-', '.', '+', });

			string tzName = string.Empty;
			string time = string.Empty;

			// We now have the time string to parse, and we'll adjust
			// to UTC or timezone after parsing
			string year = parts[0];
			string month = (parts.Length > 1) ? parts[1] : "1";
			string day = (parts.Length > 2) ? parts[2] : "1";
			string hour = (parts.Length > 3) ? parts[3] : "0";
			string minute = (parts.Length > 4) ? parts[4] : "0";
			string second = (parts.Length > 5) ? parts[5] : "0";
			string ms = (parts.Length > 6) ? parts[6] : "0";

			DateTime dt = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day), Convert.ToInt32(hour), Convert.ToInt32(minute), Convert.ToInt32(second), Convert.ToInt32(ms));

			// If a time offset was specified instead of the UTC marker,
			// add/subtract in the hours/minutes
			if ((utc == false) && (parts.Length >= 9))
			{
				// There better be a timezone offset
				string hourOffset = (parts.Length > 7) ? parts[7] : "";
				string minuteOffset = (parts.Length > 8) ? parts[8] : "";
				if (date.Contains("+"))
				{
					dt = dt.AddHours(Convert.ToDouble(hourOffset));
					dt = dt.AddMinutes(Convert.ToDouble(minuteOffset));
				}
				else
				{
					dt = dt.AddHours(-(Convert.ToDouble(hourOffset)));
					dt = dt.AddMinutes(-(Convert.ToDouble(minuteOffset)));
				}
			}

			if (utc)
			{
				// Convert the Kind to DateTimeKind.Utc if string Z present
				dt = new DateTime(dt.Ticks, DateTimeKind.Utc);
			}

			return dt;
		}

		public static string ToIso8601(DateTime dt)
		{
			string result = dt.Year.ToString() + "-" +
							TwoDigits(dt.Month) + "-" +
							TwoDigits(dt.Day) + "T" +
							TwoDigits(dt.Hour) + ":" +
							TwoDigits(dt.Minute) + ":" +
							TwoDigits(dt.Second) + "." +
							ThreeDigits(dt.Millisecond) + "Z";

			return result;
		}

		private static string TwoDigits(int value)
		{
			if (value < 10)
			{
				return "0" + value.ToString();
			}

			return value.ToString();

		}

		private static string ThreeDigits(int value)
		{
			if (value < 10)
			{
				return "00" + value.ToString();
			}
			else if (value < 100)
			{
				return "0" + value.ToString();
			}

			return value.ToString();
		}

		public static string ToASPNetAjax(DateTime dt)
		{
			string value = dt.Ticks.ToString();

			return @"\/Date(" + value + @")\/";
		}

		public static DateTime FromASPNetAjax(string ajax)
		{
			string[] parts = ajax.Split(new char[] { '(', ')' });

			long ticks = Convert.ToInt64(parts[1]);

			// Create a Utc DateTime based on the tick count
			DateTime dt = new DateTime(ticks, DateTimeKind.Utc);

			return dt;
		}

	}
}
