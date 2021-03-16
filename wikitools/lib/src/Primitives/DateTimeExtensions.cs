using System;

namespace Wikitools.Lib.Primitives
{
    public static class DateTimeExtensions
    {
        public static DateTime Trim(this DateTime date, DateTimePrecision precision) =>
            precision switch
            {
                DateTimePrecision.Month => new DateTime(date.Year, date.Month, 1),
                DateTimePrecision.Day => new DateTime(date.Year, date.Month, date.Day),
                _ => throw new ArgumentOutOfRangeException(nameof(precision), precision, null)
            };

        public static DateTime Utc(this DateTime date) =>
            new(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc);

        public static DateTime MonthFirstDay(this DateTime date) => new(date.Year, date.Month, 1);

        public static DateTime MonthLastDay(this DateTime date) => date.MonthFirstDay().AddMonths(1).AddDays(-1);
    }
}