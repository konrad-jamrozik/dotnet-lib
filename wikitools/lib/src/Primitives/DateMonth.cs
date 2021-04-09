﻿using System;

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

        public DateMonth AddMonths(int months) => new(_dateTime.AddMonths(months));

        public bool Equals(DateTime other) => _dateTime.Equals(other);

        public bool Equals(DateDay? other) => other != null && Equals(other);

        public bool Equals(DateMonth? other) => _dateTime.Equals(other?._dateTime);

        public int CompareTo(DateTime other) => _dateTime.CompareTo(other);

        // returns 1 on null to duplicate behavior of System.DateTime.CompareTo
        public int CompareTo(DateDay? other) => other == null ? 1 : CompareTo(other);

        public int CompareTo(DateMonth? other) => _dateTime.CompareTo(other?._dateTime);

        public override int GetHashCode() => _dateTime.GetHashCode();
        
        public string ToString(string? format, IFormatProvider? formatProvider) => 
            _dateTime.ToString(format, formatProvider);

        private readonly DateTime _dateTime = new(Year, Month, 1);

    }
}