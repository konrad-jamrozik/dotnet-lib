using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Wikitools.Lib.Data;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Xunit;

namespace Wikitools.Lib.Tests.Data;

public class FilePathTrieTests
{
    [Fact] 
    public void TrieFromEmptyInput() => VerifyPreorderTraversal(
        new string[] {}, 
        new List<string[]>());

    [Fact] 
    public void TrieFromThreeSegmentPath() => VerifyPreorderTraversal(
        new[]
        {
            "foo\\bar\\baz"
        }, 
        new[]
        {
            "foo", "bar", "baz"
        });

    [Fact]
    public void TrieFromTwoSingleSegmentPaths() => VerifyPreorderTraversal(
        new[]
        {
            "foo", 
            "bar"
        }, 
        new[]
        {
            new[] { "foo" }, 
            new[] { "bar" },
        });

    [Fact]
    public void TrieFromRepeatedPaths() => VerifyPreorderTraversal(
        new[]
        {
            "foo\\bar", 
            "foo\\bar"
        }, 
        new[]
        {
            new[] { "foo", "bar" }
        });

    [Fact]
    public void TrieFromReversedPaths() => VerifyPreorderTraversal(
        new[]
        {
            "foo\\bar", 
            "bar\\foo"
        }, 
        new[] 
        { 
            new[] { "foo", "bar" }, 
            new[] { "bar", "foo" }
        });

    [Fact]
    public void TrieFromStaircasePaths() => VerifyPreorderTraversal(
        new[]
        {
            "foo", 
            "foo\\bar",
            "foo\\bar\\baz"
        }, 
        new[]
        {
            new[] { "foo", "bar", "baz" }
        });

    [Fact]
    public void TrieFromStaircasePathsWithFiles() => VerifyPreorderTraversal(
        new[]
        {
            "foo\\f1", 
            "foo\\bar\\f2",
            "foo\\bar\\qux\\f3"
        }, 
        new[]
        {
            new[] { "foo" },
            new[] { "foo", "f1" },
            new[] { "foo", "bar" },
            new[] { "foo", "bar", "f2" },
            // This entry is missing because there was only 1 suffix after "qux", i.e. there was no "fork".
            // new[] { "foo", "bar" "qux"},
            new[] { "foo", "bar", "qux", "f3" }
        });

    [Fact]
    public void TrieFromStaircasePathsWithFilesLeafsOnly() => VerifyPreorderTraversal(
        new[]
        {
            "foo\\f1", 
            "foo\\bar\\f2",
            "foo\\bar\\qux\\f3"
        }, 
        new[]
        {
            new[] { "foo", "f1" },
            new[] { "foo", "bar", "f2" },
            new[] { "foo", "bar", "qux", "f3" }
        },
        leafsOnly: true);

    [Fact]
    public void TrieFromReverseStaircasePaths() => VerifyPreorderTraversal(
        new[]
        {
            "foo\\bar\\baz",
            "foo\\bar",
            "foo", 
        }, 
        new[]
        {
            new[] { "foo", "bar", "baz" }
        });

    [Fact]
    public void TrieFromBranchingPaths() => VerifyPreorderTraversal(
        new[] 
        { 
            "foo\\bar1", 
            "foo\\bar2"
        },
        new[]
        {
            new[] { "foo" }, 
            new[] { "foo", "bar1" }, 
            new[] { "foo", "bar2" }
        });

    [Fact]
    public void TrieWithEmptyPathsSegmentsWhenComputingSuffixes() => VerifyPreorderTraversal(
        new[] { 
            "foo\\bar", 
            "foo\\baz",
            "foo" },
        new[]
        {
            new[] { "foo" }, 
            new[] { "foo", "bar" }, 
            new[] { "foo", "baz" }, 
        });

    [Fact]
    public void TrieFromDoublyBranchingPaths() => VerifyPreorderTraversal(
        new[] { 
            "foo\\bar2\\baz1",
            "foo\\baz1\\foo",
            "foo\\bar2\\baz2\\qux",
            "foo\\bar1\\baz1", 
            "foo\\bar1\\baz2\\quux"
        },
        new[]
        {
            new[] { "foo" }, 
            new[] { "foo", "bar2" }, 
            new[] { "foo", "bar2", "baz1" }, 
            new[] { "foo", "bar2", "baz2", "qux" },
            new[] { "foo", "baz1", "foo" }, 
            new[] { "foo", "bar1" }, 
            new[] { "foo", "bar1", "baz1" }, 
            new[] { "foo", "bar1", "baz2", "quux" }, 
        });

    [Fact]
    public void TrieFromJaggedPaths() => VerifyPreorderTraversal(
        new[] { 
            "foo\\bar\\baz\\qux\\quux", 
            "bar",
            "foo\\bar\\baz",
            "foo\\bar\\bar",
            "foo" },
        new[]
        {
            new[] { "foo", "bar" }, 
            new[] { "foo", "bar", "baz", "qux", "quux" }, 
            new[] { "foo", "bar", "bar" }, 
            new[] { "bar" }, 
        });


    private static void VerifyPreorderTraversal(string[] pathsUT, string[] expectedPathSegments)
        => VerifyPreorderTraversal(pathsUT, expectedPathSegments.WrapInList());

    private static void VerifyPreorderTraversal(
        string[] pathsUT,
        IList<string[]> expectedPathsSegments,
        bool leafsOnly = false)
    {
        var trieUT = new FilePathTrie(pathsUT);
            
        // Act
        var preorderTraversalUT = trieUT.PreorderTraversal(leafsOnly);

        // Filter out Suffixes by calling PathPart.Leaf, as we don't test for correctness of suffixes.
        PathPart<object?>[] expectedPaths = expectedPathsSegments.Select(PathPart.Leaf).ToArray();
            
        PathPart<object?>[] actualPaths =
            preorderTraversalUT.Select(path => PathPart.Leaf(path.Segments)).ToArray();

        expectedPaths.Zip(actualPaths).Assert(
            pathsPair => pathsPair.First == pathsPair.Second,
            pathsPair => new Exception($"expected: {pathsPair.First} actual: {pathsPair.Second}")).Consume();
        Assert.Equal(expectedPaths.Length, actualPaths.Length);
    }
}