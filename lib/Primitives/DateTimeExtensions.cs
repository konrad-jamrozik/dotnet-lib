using System;
using System.Globalization;

namespace Wikitools.Lib.Primitives;

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

    public static DateDay MonthFirstDay(this DateTime date) => new DateDay(date.Year, date.Month, 1, date.Kind);

    public static DateDay MonthLastDay(this DateTime date) => date.MonthFirstDay().AddMonths(1).AddDays(-1);

    public static DateMonth Month(this DateTime date) => new(date);

    public static string Format(this DateTime date, bool includeTime = false, bool includeTimezone = false)
    {
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings
        var timeSuffix = includeTime ? " HH:mm:ss" : "";
        var timezoneSuffix = includeTimezone ? " " + FormatTimezone(date) : "";
        return date.ToString("yyyy-MM-dd" + timeSuffix, CultureInfo.InvariantCulture) + timezoneSuffix;
    }

    public static string FormatTimezone(DateTime date)
    {
        var timezone = date.Kind switch
        {
            DateTimeKind.Utc => "UTC",
            DateTimeKind.Local => "local time",
            DateTimeKind.Unspecified => "",
            _ => throw new InvalidOperationException(date.Kind.ToString())
        };
        return timezone;
    }
}