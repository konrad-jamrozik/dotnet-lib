using System;

namespace Wikitools.Lib.Primitives;

public interface ITimeline
{
    DateTime UtcNow { get; }
}