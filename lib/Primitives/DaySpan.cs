using System;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives;

public record DaySpan
{
    public DaySpan(DateTime now, int daysAgo) : this(
        new DateDay(now.AddDays(-daysAgo)),
        new DateDay(now)) { }

    public DaySpan(DateDay singleDay) : this(singleDay, singleDay) { }

    public DaySpan(DateDay startDay, DateDay endDay)
    {
        // Note this setup of invariant checks in ctor has some problems.
        // Details here: https://github.com/dotnet/csharplang/issues/4453#issuecomment-782807066
        Contract.Assert(startDay.Kind == endDay.Kind);
        Contract.Assert(startDay.CompareTo(endDay) <= 0);
        StartDay = startDay;
        EndDay = endDay;
    }

    public DateDay StartDay { get; }

    public DateDay EndDay { get; }

    public DateTimeKind Kind => StartDay.Kind;

    /// <summary>
    /// Check if date is at AfterDay or later, and at BeforeDay or before.
    /// The check is inclusive.
    /// For example, if AfterDay is May 11th and BeforeDay is May 15th,
    /// the first accepted date is at midnight from May 10th to May 11th,
    /// and the last accepted date is at midnight from May 15th to May 16th.
    /// </summary>
    /// <param name="date"></param>
    public bool Contains(DateTime date)
        => StartDay.CompareTo(date) <= 0 && 0 <= EndDay.AddDays(1).CompareTo(date);

    public bool IsSubsetOf(DaySpan daySpan)
        => StartDay.CompareTo(daySpan.StartDay) >= 0 &&
           EndDay.CompareTo(daySpan.EndDay) <= 0;

    public int Count => (int) (EndDay - StartDay).TotalDays + 1;
}