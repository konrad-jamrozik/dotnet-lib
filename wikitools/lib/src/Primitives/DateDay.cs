using System;

namespace Wikitools.Lib.Primitives
{
    public record DateDay(int Year, int Month, int Day) : IComparable<DateDay>, IComparable<DateTime>, IEquatable<DateTime>
    {
        public DateDay(DateTime dateTime) : this(dateTime.Year, dateTime.Month, dateTime.Day) { }

        private readonly DateTime _dateTime = new(Year, Month, Day);

        public int CompareTo(DateTime other) => _dateTime.CompareTo(other);

        public int CompareTo(DateDay? other) => _dateTime.CompareTo(other?._dateTime);

        public virtual bool Equals(DateTime other) => _dateTime.Equals(other);

        public virtual bool Equals(DateDay? other) => _dateTime.Equals(other?._dateTime);

        public override int GetHashCode() => _dateTime.GetHashCode();
    }
}