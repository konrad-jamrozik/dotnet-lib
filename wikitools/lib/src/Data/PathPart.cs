using System.Collections.Generic;

namespace Wikitools.Lib.Data
{
    public record PathPart<TValue>(IList<string> Segments, TValue Value, IList<PathPart<TValue>> Children)
    {
    }
}