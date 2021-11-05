using System.Collections.Generic;
using Wikitools.Lib.Data;

namespace Wikitools.Lib.OS
{
    // kj2 FilePaths -> Paths
    public record FilePathTrie(IEnumerable<string> FilePaths) : TrieFromPaths(
        FilePaths, FileSystem.SplitPath)
    { }
}