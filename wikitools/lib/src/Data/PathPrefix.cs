using System.Collections.Generic;

namespace Wikitools.Lib.Data
{
    // kja this is really more like "PathSegmentSet" and PathSuffixes are "Children"
    public record PathPrefix<TValue>(IList<string> PathSegments, TValue Value, IList<PathPrefix<TValue>> PathSuffixes)
    {
    }
}