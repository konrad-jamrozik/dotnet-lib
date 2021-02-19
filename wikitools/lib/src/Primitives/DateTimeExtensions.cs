using System;

namespace Wikitools.Lib.Primitives
{
    public static class DateTimeExtensions
    {
        public static DateTime Trim(this DateTime date, DateTimePrecision precision) =>
            precision switch
            {
                DateTimePrecision.Month => new DateTime(date.Year, date.Month, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(precision), precision, null)
            };

        public static DateTime Utc(this DateTime date) =>
            new(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);
    }
}