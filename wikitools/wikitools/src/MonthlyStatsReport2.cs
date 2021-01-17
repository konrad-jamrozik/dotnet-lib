using System.Collections.Generic;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    // kja NEXT
    public record MonthlyStatsReport2(ITimeline Timeline,  GitLogCommit[] Stats) : MarkdownDocument
    {
        public override List<object> Content => new();
    }
}