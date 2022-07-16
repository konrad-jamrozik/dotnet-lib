using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Wikitools.Lib.Primitives;

public sealed record DateDay(int Year, int Month, int Day, DateTimeKind Kind) : 
    IComparable<DateTime>, IEquatable<DateTime>,
    IComparable<DateDay>,
    IComparable<DateMonth>, IEquatable<DateMonth>,
    IFormattable
{
    public static int Compare(DateTime left, DateTime right)
        => new DateDay(left).CompareTo(new DateDay(right));

    public static bool operator <=(DateDay left, DateDay right) => Compare(left, right) <= 0;

    public static bool operator >=(DateDay left, DateDay right) => Compare(left, right) >= 0;
        
    public static DateDay operator +(DateDay dateDay, int days) => new(dateDay._dateTime.AddDays(days));

    public static TimeSpan operator -(DateDay left, DateDay right) => left._dateTime.Subtract(right._dateTime);

    public static implicit operator DateTime(DateDay dateDay) => dateDay._dateTime;

    public DateMonth AsDateMonth() => new DateMonth(_dateTime);

    public DateDay(DateTime dateTime) : this(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Kind) { }

    public DateDay AddDays(int days) => new(_dateTime.AddDays(days));

    public DateDay AddMonths(int months) => new(_dateTime.AddMonths(months));

    public bool Equals(DateTime other) => _dateTime.Equals(other);

    public bool Equals(DateMonth? other) => other != null && Equals(new DateDay(other));

    public bool Equals(DateDay? other) => _dateTime.Equals(other?._dateTime);

    // Returns 1 on null. This is to ensure the same behavior as System.DateTime.CompareTo.
    public int CompareTo(DateMonth? other) => other == null ? 1 : CompareTo(new DateDay(other));

    public int CompareTo(DateDay? other) => _dateTime.CompareTo(other?._dateTime);

    public int CompareTo(DateTime other) => _dateTime.CompareTo(other);

    public override int GetHashCode() => _dateTime.GetHashCode();

    public override string ToString() => _dateTime.ToString(CultureInfo.InvariantCulture);

    [JsonIgnore]
    public DateDay Yesterday => AddDays(-1);

    public string ToString(string? format, IFormatProvider? formatProvider) =>
        format == null && formatProvider == null
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
            ? $"{_dateTime:M/d/yyyy}"
            : _dateTime.ToString(format, formatProvider);

    private readonly DateTime _dateTime = DateTime.SpecifyKind(new DateTime(Year, Month, Day), Kind);
}