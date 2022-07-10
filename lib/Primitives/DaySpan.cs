using System;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives;

public record DaySpan
{
    public DateDay StartDay { get; private init; }

    public DateDay EndDay { get; private init; }
    
    public DaySpan(DateDay singleDay) : this(singleDay, singleDay) { }

    public DaySpan(DateDay startDay, DateDay endDay)
    {
        Contract.Assert(startDay.Kind == endDay.Kind);
        Contract.Assert(startDay.CompareTo(endDay) <= 0);
        StartDay = startDay;
        EndDay = endDay;
    }

    public DateTimeKind Kind => StartDay.Kind;

    /// <summary>
    /// Check if date is at AfterDay or later, and at BeforeDay or before.
    /// The check is inclusive.
    /// For example, if AfterDay is May 11th and BeforeDay is May 15th,
    /// the first accepted date is at midnight from May 10th to May 11th,
    /// and the last accepted date is at midnight from May 15th to May 16th.
    /// </summary>
    public bool Contains(DateTime date)
        => StartDay.CompareTo(date) <= 0 && 0 <= EndDay.AddDays(1).CompareTo(date);

    public bool IsSubsetOf(DaySpan daySpan)
        => StartDay.CompareTo(daySpan.StartDay) >= 0 &&
           EndDay.CompareTo(daySpan.EndDay) <= 0;

    public int Count => (int) (EndDay - StartDay).TotalDays + 1;

    public int MonthsCount => DateMonth.MonthSpan(this).Length;

    public bool IsWithinOneMonth => StartDay.AsDateMonth() == EndDay.AsDateMonth();

    public DateMonth Month
    {
        get
        {
            Contract.Assert(IsWithinOneMonth);
            return MonthsSpan[0];
        }
    }

    public DateMonth[] MonthsSpan => DateMonth.MonthSpan(this);

    public bool IsExactlyForEntireMonth(DateMonth month)
        => StartDay == month.FirstDay && EndDay == month.LastDay;

    public DaySpan Until(DateTime dateTime)
        => this with { StartDay = StartDay, EndDay = new DateDay(dateTime) };

    public DaySpan Merge(DaySpan other, bool allowGap = false)
    {
        AssertNoLaterThan(other);
        if (!allowGap)
            AssertNoGap(other);

        return new DaySpan(StartDay, other.EndDay);
    }

    public string ToPrettyString()
    {
        return
            $"a day span from {FormatDay(StartDay)} to {FormatDay(EndDay)} ({Count} days)";

        string FormatDay(DateDay day) => ((DateTime)day).Format(includeTimezone: true);
    }

    private void AssertNoLaterThan(DaySpan other)
    {
        Contract.Assert(StartDay.CompareTo(other.StartDay) <= 0);
        Contract.Assert(EndDay.CompareTo(other.EndDay) <= 0);
    }

    private void AssertNoGap(DaySpan other)
    {
        Contract.Assert(EndDay.AddDays(1).CompareTo(other.StartDay) >= 0);
    }
}