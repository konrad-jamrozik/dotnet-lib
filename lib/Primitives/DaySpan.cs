using System;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives
{
    public record DaySpan
    {
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

        public bool Contains(DateTime date)
            => AfterDay.CompareTo(date) <= 0 && 0 <= BeforeDay.CompareTo(date);
    }
}