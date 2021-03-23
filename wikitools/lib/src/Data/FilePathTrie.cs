using System.Collections.Generic;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Data
{
    public record FilePathTrie(IEnumerable<string> FilePaths) : TrieFromPaths(
        FilePaths, FileSystem.SplitPath)
    { }
}