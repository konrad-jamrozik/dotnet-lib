using System.Collections.Generic;

namespace Wikitools.Lib.Primitives
{
    public static class ListExtensions
    {
        // kja rename to "List"
        public static IList<TValue> WrapInList<TValue>(this TValue source) => 
            new List<TValue> { source };
    }
}