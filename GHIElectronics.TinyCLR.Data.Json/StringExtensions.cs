using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Data.Json
{
    internal static class StringExtensions
    {
		public static bool Contains(this string source, string search)
		{
			return source.IndexOf(search) >= 0;
		}

		public static bool EndsWith(this string source, string search)
		{
			return source.IndexOf(search) == source.Length - search.Length;
		}

	}
}
