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
    }
}