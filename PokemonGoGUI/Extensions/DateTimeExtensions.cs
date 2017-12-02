using System;

namespace PokemonGoGUI.Extensions
{
    public static class DateTimeExtensions
    {
        private static DateTime _posixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalMilliseconds);
        }

        public static DateTime GetDateTimeFromMilliseconds(ulong timestampMilliseconds)
        {
            return _posixTime.AddMilliseconds(timestampMilliseconds);
        }

        public static DateTime GetDateTimeFromSeconds(uint timestampSeconds)
        {
            return _posixTime.AddSeconds(timestampSeconds);
        }
    }
}