using System.Collections.Generic;
using System.Linq;
using System;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Data;

public record PathPart<TValue>(IEnumerable<string> Segments, TValue Value, IEnumerable<PathPart<TValue>> Suffixes)
{
    public PathPart<TValue> Concat(PathPart<TValue> suffix) 
        => suffix with { Segments = Segments.Concat(suffix.Segments) };

    public override string ToString() =>
        $"{nameof(Segments)}: '{string.Join(" ", Segments)}' " +
        $"{nameof(Value)}: '{Value}' " +
        $"{nameof(Suffixes)}.Count(): {Suffixes.Count()}";

    public virtual bool Equals(PathPart<TValue>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Segments.SequenceEqual(other.Segments) 
               && EqualityComparer<TValue>.Default.Equals(Value, other.Value) 
               && Suffixes.SequenceEqual(other.Suffixes);
    }

    public override int GetHashCode() => this.GetHashCodeOnProperties();
}

/// <summary>
/// Wraps a set of path segments into a corresponding PathPart that is a leaf,
/// meaning it has no trie suffixes attached to it.
/// </summary>
public static class PathPart
{
    public static PathPart<object?> Leaf(IEnumerable<string> segments) 
        => new(segments, null, Array.Empty<PathPart<object?>>());
}