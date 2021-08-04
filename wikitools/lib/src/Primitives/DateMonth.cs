using System;
using System.Globalization;

namespace Wikitools.Lib.Primitives
{
    public sealed record DateMonth(int Year, int Month) :
        IComparable<DateTime>, IEquatable<DateTime>,
        IComparable<DateDay>, IEquatable<DateDay>,    
        IComparable<DateMonth>,
        IFormattable
    {
        public static DateMonth operator +(DateMonth dateMonth, int months) => new(dateMonth._dateTime.AddMonths(months));

        public static TimeSpan operator -(DateMonth left, DateMonth right) => left._dateTime.Subtract(right._dateTime);

        public static implicit operator DateTime(DateMonth dateDay) => dateDay._dateTime;

        public DateMonth(DateTime dateTime) : this(dateTime.Year, dateTime.Month) { }

        public DateMonth PreviousMonth => AddMonths(-1);

        public DateMonth NextMonth => AddMonths(1);

        public DateMonth AddMonths(int months) => new(_dateTime.AddMonths(months));

        public DateDay FirstDay => new(_dateTime);

        public DateDay LastDay => new (_dateTime.AddMonths(1).AddDays(-1));

        public bool Equals(DateTime other) => _dateTime.Equals(other);

        public bool Equals(DateDay? other) => other != null && Equals(new DateMonth(other));

        public bool Equals(DateMonth? other) => _dateTime.Equals(other?._dateTime);

        public int CompareTo(DateTime other) => _dateTime.CompareTo(other);

        // returns 1 on null to duplicate behavior of System.DateTime.CompareTo
        public int CompareTo(DateDay? other) => other == null ? 1 : CompareTo(other);

        public int CompareTo(DateMonth? other) => _dateTime.CompareTo(other?._dateTime);

        public override int GetHashCode() => _dateTime.GetHashCode();

        public override string ToString() => _dateTime.ToString(CultureInfo.InvariantCulture);

        public string ToString(string? format, IFormatProvider? formatProvider) =>
            format == null && formatProvider == null
                ? $"{_dateTime:yyyy/MM}"
                : _dateTime.ToString(format, formatProvider);

        // kj2 fix as in DateDay Known limitation: this doesn't retain DateTimeKind (e.g. Utc)
        private readonly DateTime _dateTime = new(Year, Month, 1);
    }
}