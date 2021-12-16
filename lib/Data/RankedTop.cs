using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Data;

// kja use it in TopStatsReport
public record RankedTop<T>(IEnumerable<T> Seq, int? Top) : IEnumerable<(int rank, T elem)>
{
    private IEnumerable<(int rank, T elem)> RankedTopSeq
        => Seq.Take(Top != null ? new Range(0, (int)Top) : Range.All)
            .Select((elem, rank) => (rank + 1, elem));
    
    public IEnumerator<(int rank, T elem)> GetEnumerator()
        => RankedTopSeq.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}