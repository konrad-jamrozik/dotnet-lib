using System.Collections.Generic;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Lib.Tests
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void TestUnionUsing()
        {
            // kja NEXT curr work. Simplify; add test using separate props.
            var first  = new List<dynamic>
            {
                new { Foo = "val1" },
                new { Foo = "val2" }
            };
            var second = new List<dynamic>
            {
                new { Foo = "val2" },
                new { Foo = "val3" }
            };

            var expected = new List<dynamic>
            {
                new { Foo = "val1" },
                new { Foo = "val2val2" },
                new { Foo = "val3" }
            };

            // Act
            var union = first.UnionUsing(second, obj => obj.Foo, (obj1, obj2) => new { Foo = obj1.Foo + obj2.Foo });

            new JsonDiffAssertion(expected, union).Assert();
        }
    }
}