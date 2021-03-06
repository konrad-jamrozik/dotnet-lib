using System.Collections.Generic;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Lib.Tests
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void UnionMerges()
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
            var actual = first.UnionMerge(
                second,
                i => i[0],
                (i1, i2) => new List<string> { i1[0], i1[1] + "_" + i2[1], i1[2] + "|" + i2[2] });

            new JsonDiffAssertion(expected, actual).Assert();
        }


        [Fact]
        public void UnionMergesReusingKeyForMerge()
        {
            var first    = new List<dynamic> { new { Foo = "i1" }, new { Foo = "i2" } };
            var second   = new List<dynamic> { new { Foo = "i2" }, new { Foo = "i3" } };
            var expected = new List<dynamic> { new { Foo = "i1" }, new { Foo = "i2i2_" }, new { Foo = "i3" } };

            // Act
            var actual = first.UnionMerge(second, i => i.Foo, (i1, i2) => new { Foo = i1.Foo + i2.Foo + "_" });

            new JsonDiffAssertion(expected, actual).Assert();
        }
    }
}