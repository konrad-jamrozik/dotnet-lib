using System;

namespace Wikitools.Lib.Primitives
{
    public sealed record DateDay(int Year, int Month, int Day) : IComparable<DateDay>, IComparable<DateTime>, IEquatable<DateTime>
    {
        public DateDay(DateTime dateTime) : this(dateTime.Year, dateTime.Month, dateTime.Day) { }

        public DateDay(DateTime dateTime, bool roundUpDay = false) : this(dateTime.Year, dateTime.Month, GetDay(dateTime, roundUpDay)) { }

        private static int GetDay(DateTime dateTime, bool roundDayUp) => 
            dateTime == new DateDay(dateTime)._dateTime || !roundDayUp 
                ? dateTime.Day 
                : dateTime.Day + 1;

        private readonly DateTime _dateTime = new(Year, Month, Day);

        public int CompareTo(DateTime other) => _dateTime.CompareTo(other);

        public int CompareTo(DateDay? other) => _dateTime.CompareTo(other?._dateTime);

        public bool Equals(DateTime other) => _dateTime.Equals(other);

        public bool Equals(DateDay? other) => _dateTime.Equals(other?._dateTime);

        public override int GetHashCode() => _dateTime.GetHashCode();

        public static implicit operator DateTime(DateDay dateDay) => dateDay._dateTime;

        public DateDay AddDays(int days) => new(_dateTime.AddDays(days));

        public DateDay AddMonths(int months) => new(_dateTime.AddMonths(months));

        public static DateDay operator +(DateDay dateDay, int days) => new(dateDay._dateTime.AddDays(days));

        public static TimeSpan operator -(DateDay left, DateDay right) => left._dateTime.Subtract(right._dateTime);

    }
}