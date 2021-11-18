using System.Collections.Generic;

namespace Wikitools.Lib.Primitives
{
    public record JoinSets<T, U>(IEnumerable<(T Left, U Right)> Both, IEnumerable<T> Left, IEnumerable<U> Right);
}