using System;
using System.Linq;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Primitives;

public static class MathExtensions
{
    public static int MinWith(this int subject, int? target) => Min(subject, target);

    public static int Min(int? left, int? right)
    {
        Contract.Assert(left != null || right != null, "At least one of the minimized values needs to be nonnull");
        return left != null && right != null
            ? Math.Min((int) left, (int) right)
            : (int) new[] { left, right }.Single(e => e != null)!;
    }
}