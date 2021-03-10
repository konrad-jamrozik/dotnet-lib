using System.Collections.Generic;
using System.IO;

namespace Wikitools.Lib.Data
{
    public record FilePathTrie(IEnumerable<string> FilePaths) : TrieFromPaths(
        FilePaths, SplitPath)
    {
        // kj2 need to think about better home for this. (I)Filesystem?
        public static IEnumerable<string> SplitPath(string path) => path.Split(Path.DirectorySeparatorChar);
    }
}