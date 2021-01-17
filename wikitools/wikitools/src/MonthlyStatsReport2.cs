using System;
using System.Collections.Generic;
using Wikitools.Lib.Git;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public class MonthlyStatsReport2 : MarkdownDocument
    {
        public MonthlyStatsReport2(ITimeline timeline, List<List<GitAuthorChangeStats>> stats)
        {
            // kja NEXT
            throw new NotImplementedException();
        }

        public override List<object> Content => new();
    }
}