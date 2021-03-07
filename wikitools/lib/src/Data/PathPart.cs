using System.Collections.Generic;

namespace Wikitools.Lib.Data
{
    public record PathPart<TValue>(IEnumerable<string> Segments, TValue Value, IEnumerable<PathPart<TValue>> Suffixes)
    {
    }
}