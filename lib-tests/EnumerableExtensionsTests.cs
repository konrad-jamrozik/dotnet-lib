using System;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Lib.Tests;

public class EnumerableExtensionsTests
{
    [Fact]
    public void ConcatMerges()
    {
        var first = new List<List<string>>
        {
            new() { "key1A", "bodyA", "foo" },
            new() { "key2", "bodyA", "bar" },
            new() { "key3", "bodyB", "foo" },
            new() { "key4A", "bodyB", "bar" },
        };

        var second = new List<List<string>>
        {
            new() { "key1B", "bodyA", "foo" },
            new() { "key2", "bodyC", "foo" },
            new() { "key3", "bodyD", "bar" },
            new() { "key4B", "bodyB", "bar" },
        };

        var expected = new List<List<string>>
        {
            new() { "key1A", "bodyA", "foo" },
            new() { "key4A", "bodyB", "bar" },
            new() { "key2", "bodyA_bodyC", "bar|foo" },
            new() { "key3", "bodyB_bodyD", "foo|bar" },
            new() { "key1B", "bodyA", "foo" },
            new() { "key4B", "bodyB", "bar" },
        };

        // Act
        var actual = first.ConcatMerge(
            second,
            i => i[0],
            (i1, i2) => new List<string> { i1[0], i1[1] + "_" + i2[1], i1[2] + "|" + i2[2] });

        new JsonDiffAssertion(expected, actual).Assert();
    }


    [Fact]
    public void MergesUsingKeyInMerge()
    {
        var first    = new List<dynamic> { new { Foo = "i1" }, new { Foo = "i2" } };
        var second   = new List<dynamic> { new { Foo = "i2" }, new { Foo = "i3" } };
        var expected = new List<dynamic> { new { Foo = "i1" }, new { Foo = "i2i2_" }, new { Foo = "i3" } };

        // Act
        var actual = first.ConcatMerge(second, i => i.Foo, (i1, i2) => new { Foo = i1.Foo + i2.Foo + "_" });

        new JsonDiffAssertion(expected, actual).Assert();
    }

    [Fact]
    public void ZipsMatching()
    {
        // Act
        var actual =
            new List<int> { 1, 2, 3 }.ZipMatching(
                new List<int> { 10, 20, 30 },
                match: (i1, i2) => i1 * 10 == i2,
                selectResult: (i1, i2) => i1 + i2);

        Assert.Collection(
            actual,
            i1 => Assert.Equal(11, i1),
            i2 => Assert.Equal(22, i2),
            i3 => Assert.Equal(33, i3)
        );
    }

    [Fact]
    public void ZipMatchingThrowsOnWrongMatch()
    {
        Assert.Throws<InvalidOperationException>(() =>
            {
                // Act
                var actual =
                    new List<int> { 1, 2, 3 }.ZipMatching(
                        new List<int> { 1, 4, 3 },
                        match: (i1, i2) => i1 == i2,
                        selectResult: (i1, i2) => i1 + i2).ToList();
            }
        );
    }

    [Fact]
    public void ZipMatchingThrowsOnFirstTooLong()
    {
        Assert.Throws<InvalidOperationException>(() =>
            {
                // Act
                var actual =
                    new List<int> { 1, 2, 3 }.ZipMatching(
                        new List<int> { 1, 2 },
                        match: (i1, i2) => i1 == i2,
                        selectResult: (i1, i2) => i1 + i2).ToList();
            }
        );
    }

    [Fact]
    public void ZipMatchingThrowsOnSecondTooLong()
    {
        Assert.Throws<InvalidOperationException>(() =>
            {
                // Act
                var actual =
                    new List<int> { 1, 2 }.ZipMatching(
                        new List<int> { 1, 2, 3 },
                        match: (i1, i2) => i1 == i2,
                        selectResult: (i1, i2) => i1 + i2).ToList();
            }
        );
    }
}