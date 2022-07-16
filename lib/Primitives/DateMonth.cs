using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives;

public sealed record DateMonth(int Year, int Month, DateTimeKind Kind) :
    IComparable<DateTime>, IEquatable<DateTime>,
    IComparable<DateDay>, IEquatable<DateDay>,    
    IComparable<DateMonth>,
    IFormattable
{
    public static DateMonth operator +(DateMonth dateMonth, int months) => new(dateMonth._dateTime.AddMonths(months));

    public static TimeSpan operator -(DateMonth left, DateMonth right) => left._dateTime.Subtract(right._dateTime);

    public static implicit operator DateTime(DateMonth dateMonth) => dateMonth._dateTime;

    public static DateMonth[] MonthSpan(DaySpan daySpan)
    {
        DateMonth iteratedMonth = daySpan.StartDay.AsDateMonth();
        var output = new List<DateMonth>();
        // kj2-refactor to get rid of this 'while' I need something like "AggregateWhile"
        // on a lazy stream of (month, month.NextMonth, month.NextMonth.NextMonth)  
        // Something like:
        // startDay.AsDateMonth().AggregateWhile(
        //   nextFunc: month => month.NextMonth
        //   predicate: month.CompareTo(endDay) <= 0
        //   func: (months, month) => months.Add(month)
        // );
        // 
        // Maybe alternatively I could make DateMonth implement IEnumerable<DateMonth>.
        // The implementation would use "yield return this.NextMonth" and would go
        // ad infinitum. Then I could instead use:
        // startDay.AsDateMonth().AggregateWhile(
        //   predicate: month.CompareTo(endDay) <= 0
        //   func: (months, month) => months.Add(month)
        // );
        while (iteratedMonth.CompareTo(daySpan.EndDay) <= 0)
        {
            output.Add(iteratedMonth);
            iteratedMonth = iteratedMonth.NextMonth;
        }
        Contract.Assert(output.Any());

        return output.ToArray();
    }

    public DateMonth(DateTime dateTime) : this(dateTime.Year, dateTime.Month, dateTime.Kind) { }

    public DateMonth PreviousMonth => AddMonths(-1);

    public DateMonth NextMonth => AddMonths(1);

    public DateMonth AddMonths(int months) => new(_dateTime.AddMonths(months));

    public DateDay FirstDay => new(_dateTime);

    public DateDay LastDay => new(_dateTime.AddMonths(1).AddDays(-1));

    public DaySpan DaySpan => new(FirstDay, LastDay);

    public bool Equals(DateTime other) => _dateTime.Equals(other);

    public bool Equals(DateDay? other) => other != null && Equals(new DateMonth(other));

    public bool Equals(DateMonth? other) => _dateTime.Equals(other?._dateTime);

    public int CompareTo(DateTime other) => _dateTime.CompareTo(other);

    // returns 1 on null to duplicate behavior of System.DateTime.CompareTo
    public int CompareTo(DateDay? other) => other == null ? 1 : CompareTo(new DateMonth(other));

    public int CompareTo(DateMonth? other) => _dateTime.CompareTo(other?._dateTime);

    public override int GetHashCode() => _dateTime.GetHashCode();

    public override string ToString() => _dateTime.ToString(CultureInfo.InvariantCulture);

    public string ToString(string? format, IFormatProvider? formatProvider) =>
        format == null && formatProvider == null
            ? $"{_dateTime:yyyy/MM}"
            : _dateTime.ToString(format, formatProvider);

    private readonly DateTime _dateTime = DateTime.SpecifyKind(new DateTime(Year, Month, 1), Kind);
}