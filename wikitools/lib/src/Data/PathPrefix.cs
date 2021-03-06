using System.Collections.Generic;

namespace Wikitools.Lib.Data
{
    public record PathPrefix<TValue>(IList<string> PathSegments, TValue Value, IList<PathPrefix<TValue>> PathSuffixes)
    {
    }
}