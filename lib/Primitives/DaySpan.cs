using System;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives
{
    public record DaySpan
    {
        public DaySpan(DateTime now, int daysAgo) : this(
            new DateDay(now.AddDays(-daysAgo)),
            new DateDay(now)) { }

        public DaySpan(DateDay afterDay, DateDay beforeDay)
        {
            // Note this setup of invariant checks in ctor has some problems.
            // Details here: https://github.com/dotnet/csharplang/issues/4453#issuecomment-782807066
            Contract.Assert(afterDay.Kind == beforeDay.Kind);
            Contract.Assert(afterDay.CompareTo(beforeDay) <= 0);
            AfterDay = afterDay;
            BeforeDay = beforeDay;
        }

        public DateDay BeforeDay { get; }

        public DateDay AfterDay { get; }

        public DateTimeKind Kind => AfterDay.Kind;

        /// <summary>
        /// Check if date is at AfterDay or later, and at BeforeDay or before.
        /// The check is inclusive.
        /// For example, if AfterDay is May 11th and BeforeDay is May 15th,
        /// the first accepted date is at midnight from May 10th to May 11th,
        /// and the last accepted date is at midnight from May 15th to May 16th.
        /// </summary>
        /// <param name="date"></param>
        public bool Contains(DateTime date)
            => AfterDay.CompareTo(date) <= 0 && 0 <= BeforeDay.AddDays(1).CompareTo(date);
    }
}